using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Fast, single-call resolver that simulates an entire basketball game outcome
/// from team + player attributes. Updates each player's match line (minutes, PTS,
/// FGM/FGA (2s/3s/FT), REB, AST, STL, BLK) and sets a final score.
/// No possession-by-possession work; intended for season sims.
/// </summary>
public class BasketballQuickSim : MatchSimulator
{
    // Pace & scoring knobs (tune once for your game’s balance)
    const int MinutesPerGamePerTeam = 48;            // NBA-style
    const int PlayersOnCourt = 5;
    const int RotationSize = 9;             // 8–10 realistic
    const float BasePossessions = 97f;           // baseline pace
    const float PossessionNoise = 6.5f;          // RNG spread in pace
    const float BaseORtg = 90f;          // points/100 poss baseline
    const float RatingScale = 15f;           // how strongly ratings shift ORtg/DRtg
    const float ThreePointBaseRate = 0.38f;         // fraction of FGA that are 3PA baseline
    const float ThreePointRateSpread = 0.14f;         // +/- driven by skills
    const float AssistOnFGMBase = 0.58f;         // fraction of made FGs that are assisted
    const float ORebRateBase = 0.27f;         // team offensive rebound rate baseline
    const float STL_per100_Base = 7.5f;
    const float BLK_per100_Base = 5.2f;
    const float FTr_Base = 0.23f;         // FT attempts per FGA
    const float FT_Pct_Base = 0.75f;
    const int STAR_TOP_N = 3;    // how many “stars” to favor
    const float STAR_USAGE_MULT = 1.85f;// how much extra share stars get
    const float USAGE_GAMMA = 1.35f;// global sharpness ( >1 = more top-heavy )

    // Optional: bias a winner (e.g., story mode)
    public int TeamToForceWin { get; set; } = -1;

    public void SimulateMatch(Match match, int teamToForceWin = -1)
    {
        TeamToForceWin = teamToForceWin;

        Team home = LeagueSystem.Instance.GetTeam(match.GetHomeTeamID());
        Team away = LeagueSystem.Instance.GetTeam(match.GetAwayTeamID());

        // 1) Compute team summaries from player skills
        var hSummary = BuildTeamSummary(home);
        var aSummary = BuildTeamSummary(away);

        // 2) Draw a pace for the game
        float basePaceShift = 0.5f * (hSummary.Pace + aSummary.Pace - 2f); // centered near 0
        float poss = Mathf.Clamp(
            RandNormal(BasePossessions + basePaceShift * 6f, PossessionNoise),
            88f, 110f
        );

        // 3) Expected ratings vs opponent defense
        float hORtg = BaseORtg + RatingScale * (hSummary.Offense - aSummary.Defense) + UnityEngine.Random.Range(-2.5f, 2.5f);
        float aORtg = BaseORtg + RatingScale * (aSummary.Offense - hSummary.Defense) + UnityEngine.Random.Range(-2.5f, 2.5f);

        // 4) Turn ratings into points
        int hPointsTarget = Mathf.RoundToInt(poss * hORtg / 100f);
        int aPointsTarget = Mathf.RoundToInt(poss * aORtg / 100f);

        // Small endgame variance
        hPointsTarget += UnityEngine.Random.Range(-6, 7);
        aPointsTarget += UnityEngine.Random.Range(-6, 7);

        // 5) Force winner if requested
        if (TeamToForceWin == home.GetTeamID() && hPointsTarget <= aPointsTarget) hPointsTarget = aPointsTarget + UnityEngine.Random.Range(1, 7);
        if (TeamToForceWin == away.GetTeamID() && aPointsTarget <= hPointsTarget) aPointsTarget = hPointsTarget + UnityEngine.Random.Range(1, 7);

        // 6) Minute distribution (48 * 5 = 240 per team)
        var hMinutes = DistributeMinutes(home, hSummary);
        var aMinutes = DistributeMinutes(away, aSummary);

        // 7) Shot profile per team
        var hProfile = BuildShotProfile(hSummary);
        var aProfile = BuildShotProfile(aSummary);

        // 8) Convert team points target into FGA/3PA/FTA, then allocate to players
        var hBox = AllocateTeamBoxScore(home, hSummary, hMinutes, hPointsTarget, poss, hProfile);
        var aBox = AllocateTeamBoxScore(away, aSummary, aMinutes, aPointsTarget, poss, aProfile);

        // 9) Apply player stats to season match lines
        CommitBoxScore(match.GetMatchID(), hBox);
        CommitBoxScore(match.GetMatchID(), aBox);

        match.SetResult(hBox.TeamPoints, aBox.TeamPoints);

        match.EndMatch();
    }

    // -------------------- Core helpers --------------------

    private TeamSummary BuildTeamSummary(Team t)
    {
        var players = t.GetPlayersFromTeam();

        // Offensive composite: shooting + playmaking + finishing
        float off =
            Avg(players, S(Skill.Three_Point_Shooting)) * 0.28f +
            Avg(players, S(Skill.Mid_Range_Shooting)) * 0.15f +
            Avg(players, S(Skill.Short_Range_Shooting)) * 0.12f +
            Avg(players, S(Skill.Dunking)) * 0.10f +
            Avg(players, S(Skill.Layups)) * 0.15f +
            Avg(players, S(Skill.Playmaking)) * 0.20f;

        // Defensive composite: on-ball, rim, events
        float def =
            Avg(players, S(Skill.Blocking)) * 0.40f +
            Avg(players, S(Skill.Stealing)) * 0.25f +
            Avg(players, S(Skill.Speed)) * 0.15f +   // If you have a general defense skill
            Avg(players, S(Skill.Rebounding)) * 0.2f;

        // Pace proxy: speed + dribbling + playmaking
        float pace =
            Avg(players, S(Skill.Speed)) * 0.45f +
            Avg(players, S(Skill.Dribbling)) * 0.30f +
            Avg(players, S(Skill.Playmaking)) * 0.25f;

        // 3PT tendency from skill mix
        float threeRate =
            Normalize01(Avg(players, S(Skill.Three_Point_Shooting)) * 0.65f +
                        Avg(players, S(Skill.Mid_Range_Shooting)) * 0.35f);

        return new TeamSummary
        {
            TeamID = t.GetTeamID(),
            Players = players,
            Offense = To01(off),
            Defense = To01(def),
            Pace = To01(pace),
            ThreeRate = Mathf.Clamp01(ThreePointBaseRate + (threeRate - 0.5f) * ThreePointRateSpread)
        };
    }

    private Dictionary<Player, int> DistributeMinutes(Team t, TeamSummary summary)
    {
        // Total team minutes
        int teamMinutes = MinutesPerGamePerTeam * PlayersOnCourt; // 240

        var pool = t.GetPlayersFromTeam()
            .Select(p => new
            {
                Player = p,
                Weight = Mathf.Max(1f, p.CalculateRatingForPosition()) * (0.75f + 0.5f) // favor rating + stamina if you have it
            })
            .OrderByDescending(x => x.Weight)
            .ToList();

        // Limit to rotation
        var rotation = pool.Take(Mathf.Min(RotationSize, pool.Count)).ToList();
        float totalW = rotation.Sum(x => x.Weight);

        // Soft caps to keep sims realistic
        int maxStarter = 38;
        int minBench = 6;

        // First pass proportional
        var result = rotation.ToDictionary(x => x.Player, x => Mathf.RoundToInt(teamMinutes * (x.Weight / Mathf.Max(1f, totalW))));

        // Enforce caps
        // Make top 5 "starters"
        foreach (var x in rotation.Take(5))
            result[x.Player] = Mathf.Clamp(result[x.Player], 30, maxStarter);

        // Bench floor
        foreach (var x in rotation.Skip(5))
            result[x.Player] = Mathf.Clamp(result[x.Player], minBench, 26);

        // Fix rounding drift to sum exactly to teamMinutes
        BalanceToTotal(result, teamMinutes);

        return result;
    }

    private ShotProfile BuildShotProfile(TeamSummary s)
    {
        // Slight team-level biases
        float threeRate = s.ThreeRate;
        float rimBias = Mathf.Lerp(0.24f, 0.36f, s.Pace); // more pace -> more rim pressure
        float midBias = Mathf.Clamp01(1f - (threeRate + rimBias));

        // Normalize
        float total = threeRate + rimBias + midBias;
        threeRate /= total; rimBias /= total; midBias /= total;

        return new ShotProfile
        {
            ThreeShare = threeRate,
            RimShare = rimBias,
            MidShare = midBias
        };
    }

    private TeamBox AllocateTeamBoxScore(Team t, TeamSummary sum, Dictionary<Player, int> minutes, int pointsTarget, float possessions, ShotProfile profile)
    {
        // Team shooting efficiency from skills (convert to probabilities)
        float p3 = Mathf.Clamp01(0.22f + 0.58f * Avg(sum.Players, S(Skill.Three_Point_Shooting)) / 99f);   // ~0.30–0.50
        float p2m = Mathf.Clamp01(0.32f + 0.50f * Avg(sum.Players, S(Skill.Mid_Range_Shooting)) / 99f);      // midrange
        float p2r = Mathf.Clamp01(0.44f + 0.40f * Avg(sum.Players, S(Skill.Short_Range_Shooting)) / 99f);    // paint
        float pft = Mathf.Clamp01(FT_Pct_Base - 0.06f + 0.16f * Avg(sum.Players, S(Skill.Mid_Range_Shooting)) / 99f);

        // Target made FGs from points target, accounting for 3s, 2s, and FTs later
        float threeShare = profile.ThreeShare;
        float rimShare = profile.RimShare;
        float midShare = profile.MidShare;

        // Initial attempt mix via target points
        // Start with FGA guess from league-y conversion
        float e2 = (rimShare * p2r + midShare * p2m);       // expected 2pt make%
        float e3 = p3;                                      // expected 3pt make%
        float eFG = e2 * (1f - threeShare) + e3 * threeShare * 1.5f;

        float fgaGuess = Mathf.Max(60f, (pointsTarget * 1.0f) / (2f * eFG)); // coarse
        float threePA = Mathf.Round(fgaGuess * threeShare);
        float twoPA = Mathf.Round(fgaGuess - threePA);

        // FTs via FTr (tied to rim pressure)
        float ftr = Mathf.Clamp01(FTr_Base + 0.08f * (rimShare - 0.30f));
        int fta = Mathf.Max(10, Mathf.RoundToInt(fgaGuess * ftr + UnityEngine.Random.Range(-4f, 6f)));

        // Calculate makes from probabilities (with randomness)
        int threePM = ClampRound(threePA * (p3 + UnityEngine.Random.Range(-0.03f, 0.03f)), 0, (int)threePA);
        int rimFGA = Mathf.RoundToInt(twoPA * rimShare / Mathf.Max(0.0001f, rimShare + midShare));
        int midFGA = Mathf.Max(0, (int)twoPA - rimFGA);

        int rimFGM = ClampRound(rimFGA * (p2r + UnityEngine.Random.Range(-0.03f, 0.03f)), 0, rimFGA);
        int midFGM = ClampRound(midFGA * (p2m + UnityEngine.Random.Range(-0.03f, 0.03f)), 0, midFGA);
        int twoPM = rimFGM + midFGM;

        int ftm = ClampRound(fta * (pft + UnityEngine.Random.Range(-0.03f, 0.03f)), 0, fta);

        int teamPts = threePM * 3 + twoPM * 2 + ftm;

        // Small correction: nudge FTs to hit target
        int delta = pointsTarget - teamPts;
        if (delta != 0)
        {
            int addFTA = Mathf.Clamp(Mathf.RoundToInt(Mathf.Abs(delta) / Mathf.Max(0.5f, pft)), 0, 16);
            if (delta > 0) { fta += addFTA; ftm += ClampRound(addFTA * pft, 0, addFTA); }
            else { int take = Mathf.Min(addFTA, fta - ftm); fta -= take; } // keep ftm ≤ fta
            teamPts = threePM * 3 + twoPM * 2 + ftm;
        }

        // Rebounds: estimate from misses & offensive rate
        int teamFGA = rimFGA + midFGA + (int)threePA;
        int teamFGM = twoPM + threePM;
        int teamMisses = Mathf.Max(0, teamFGA - teamFGM);
        float orebRate = Mathf.Clamp01(ORebRateBase + 0.10f * (Avg(sum.Players, S(Skill.Rebounding)) / 99f - 0.5f));
        int oreb = ClampRound(teamMisses * orebRate + UnityEngine.Random.Range(-3f, 3f), 0, teamMisses);
        int dreb = ClampRound((int)(possessions * 0.9f) + UnityEngine.Random.Range(-4f, 4f), 15, 40); // coarse other-end reb

        // Assists: based on made FGs & playmaking
        float astRate = Mathf.Clamp01(AssistOnFGMBase + 0.20f * (Avg(sum.Players, S(Skill.Playmaking)) / 99f - 0.5f));
        int teamAst = Mathf.Min(teamFGM, ClampRound(teamFGM * astRate + UnityEngine.Random.Range(-3f, 3f), 0, teamFGM));

        // Stocks
        int teamStl = ClampRound((STL_per100_Base + 5f * (Avg(sum.Players, S(Skill.Stealing)) / 99f - 0.5f)) * possessions / 100f + UnityEngine.Random.Range(-2f, 2f), 2, 16);
        int teamBlk = ClampRound((BLK_per100_Base + 5f * (Avg(sum.Players, S(Skill.Blocking)) / 99f - 0.5f)) * possessions / 100f + UnityEngine.Random.Range(-2f, 2f), 2, 12);

        // Allocate per-player using role weights
        var box = new TeamBox { TeamID = t.GetTeamID(), TeamPoints = teamPts };

        // Usage weights (pre-star boost): minutes * offensive impact
        var usageBase = minutes.ToDictionary(
            kv => kv.Key,
            kv => Mathf.Max(0.1f, kv.Value) * Mathf.Max(0.05f, OffensiveImpact(kv.Key))
        );

        // Star boost: multiply top N by STAR_USAGE_MULT
        var ranked = usageBase.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).ToList();
        for (int i = 0; i < Mathf.Min(STAR_TOP_N, ranked.Count); i++)
        {
            var p = ranked[i];
            usageBase[p] *= STAR_USAGE_MULT;
        }

        // Global sharpness (softmax-like gamma)
        var usage = usageBase.ToDictionary(kv => kv.Key, kv => Mathf.Pow(kv.Value, USAGE_GAMMA));
        float totalUsage = Mathf.Max(0.001f, usage.Values.Sum());

        int rimFGA_left = rimFGA;
        int midFGA_left = midFGA;
        int tpa_left = (int)threePA;
        int fta_left = fta;
        int ast_left = teamAst;
        int oreb_left = oreb;
        int dreb_left = dreb;
        int stl_left = teamStl;
        int blk_left = teamBlk;

        // Precompute player shot probs
        var pRim = minutes.Keys.ToDictionary(p => p, p => Mathf.Clamp01(0.40f + 0.40f * GetSkill01(p, Skill.Short_Range_Shooting)));
        var pMid = minutes.Keys.ToDictionary(p => p, p => Mathf.Clamp01(0.30f + 0.40f * GetSkill01(p, Skill.Mid_Range_Shooting)));
        var p3p = minutes.Keys.ToDictionary(p => p, p => Mathf.Clamp01(0.25f + 0.50f * GetSkill01(p, Skill.Three_Point_Shooting)));
        var pFT = minutes.Keys.ToDictionary(p => p, p => Mathf.Clamp01(FT_Pct_Base - 0.06f + 0.16f * GetSkill01(p, Skill.Mid_Range_Shooting)));

        // Rebound weights
        var rebW = minutes.ToDictionary(kv => kv.Key,
            kv => Mathf.Max(0.05f, kv.Value * (0.6f * GetSkill01(kv.Key, Skill.Rebounding) + 0.2f * GetSkill01(kv.Key, Skill.Blocking) + 0.2f)) // Height if you have it
        );
        float orebWTotal = rebW.Values.Sum();
        float drebWTotal = orebWTotal;

        // Playmaking weights (for assists)
        var playW = minutes.ToDictionary(kv => kv.Key,
            kv => Mathf.Max(0.05f, kv.Value * (0.7f * GetSkill01(kv.Key, Skill.Playmaking) + 0.3f * GetSkill01(kv.Key, Skill.Passing)))
        );
        float playWTotal = playW.Values.Sum();

        // Stocks weights
        var stlW = minutes.ToDictionary(kv => kv.Key, kv => Mathf.Max(0.05f, kv.Value * GetSkill01(kv.Key, Skill.Stealing)));
        var blkW = minutes.ToDictionary(kv => kv.Key, kv => Mathf.Max(0.05f, kv.Value * GetSkill01(kv.Key, Skill.Blocking)));
        float stlWTotal = stlW.Values.Sum();
        float blkWTotal = blkW.Values.Sum();

        foreach (var p in minutes.Keys)
        {
            var line = new PlayerLine();
            line.Minutes = minutes[p];

            int pfga_rim = Mathf.RoundToInt(rimFGA * (usage[p] / totalUsage) * (0.6f * GetSkill01(p, Skill.Layups) + 0.4f * GetSkill01(p, Skill.Dunking)));
            int pfga_mid = Mathf.RoundToInt(midFGA * (usage[p] / totalUsage) * (0.7f * GetSkill01(p, Skill.Mid_Range_Shooting) + 0.3f));
            int p3a = Mathf.RoundToInt(threePA * (usage[p] / totalUsage) * (0.8f * GetSkill01(p, Skill.Three_Point_Shooting) + 0.2f));
            int pfta = Take(ref fta_left, Mathf.RoundToInt(fta * (usage[p] / totalUsage) * (0.6f * GetSkill01(p, Skill.Layups) + 0.4f * GetSkill01(p, Skill.Dunking))));

            // Clamp leftovers
            pfga_rim = Take(ref rimFGA_left, pfga_rim);
            pfga_mid = Take(ref midFGA_left, pfga_mid);
            p3a = Take(ref tpa_left, p3a);

            int rim_m = ClampRound(pfga_rim * (pRim[p] + UnityEngine.Random.Range(-0.04f, 0.04f)), 0, pfga_rim);
            int mid_m = ClampRound(pfga_mid * (pMid[p] + UnityEngine.Random.Range(-0.04f, 0.04f)), 0, pfga_mid);
            int p3m = ClampRound(p3a * (p3p[p] + UnityEngine.Random.Range(-0.04f, 0.04f)), 0, p3a);

            // Free throws: tied to rim attempts & usage
            int pftm = ClampRound(pfta * (pFT[p] + UnityEngine.Random.Range(-0.04f, 0.04f)), 0, pfta);

            line.TwoPA = pfga_rim + pfga_mid;
            line.TwoPM = rim_m + mid_m;
            line.ThreePA = p3a;
            line.ThreePM = p3m;
            line.FTA = pfta;
            line.FTM = pftm;

            // Rebounds
            line.OReb = Take(ref oreb_left, ClampRound(oreb * (rebW[p] / Mathf.Max(0.001f, orebWTotal)) + UnityEngine.Random.Range(-1f, 1f), 0, 20));
            line.DReb = Take(ref dreb_left, ClampRound(dreb * (rebW[p] / Mathf.Max(0.001f, drebWTotal)) + UnityEngine.Random.Range(-2f, 2f), 0, 30));

            // Assists (cannot assist own made shots exactly here; we approximate by weights)
            line.Assists = Take(ref ast_left, ClampRound(teamAst * (playW[p] / Mathf.Max(0.001f, playWTotal)) + UnityEngine.Random.Range(-1f, 1f), 0, 20));

            // Stocks
            line.Steals = Take(ref stl_left, ClampRound(teamStl * (stlW[p] / Mathf.Max(0.001f, stlWTotal)) + UnityEngine.Random.Range(-1f, 1f), 0, 8));
            line.Blocks = Take(ref blk_left, ClampRound(teamBlk * (blkW[p] / Mathf.Max(0.001f, blkWTotal)) + UnityEngine.Random.Range(-1f, 1f), 0, 7));

            box.Lines[p] = line;
        }

        // Distribute any remaining team totals (from rounding) randomly
        DistributeRemainder(box.Lines.Keys.ToList(), ref oreb_left, l => box.Lines[l].OReb++);
        DistributeRemainder(box.Lines.Keys.ToList(), ref dreb_left, l => box.Lines[l].DReb++);
        DistributeRemainder(box.Lines.Keys.ToList(), ref ast_left, l => box.Lines[l].Assists++);
        DistributeRemainder(box.Lines.Keys.ToList(), ref stl_left, l => box.Lines[l].Steals++);
        DistributeRemainder(box.Lines.Keys.ToList(), ref blk_left, l => box.Lines[l].Blocks++);
        DistributeRemainder(box.Lines.Keys.ToList(), ref rimFGA_left, l => box.Lines[l].TwoPA++);
        DistributeRemainder(box.Lines.Keys.ToList(), ref midFGA_left, l => box.Lines[l].TwoPA++);
        DistributeRemainder(box.Lines.Keys.ToList(), ref tpa_left, l => box.Lines[l].ThreePA++);
        DistributeRemainder(box.Lines.Keys.ToList(), ref fta_left, l => box.Lines[l].FTA++);

        return box;
    }

    private void CommitBoxScore(int matchID, TeamBox box)
    {
        foreach (var kv in box.Lines)
        {
            var p = kv.Key;
            var ln = kv.Value;

            // Minutes
            p.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)> { ("minutes", ln.Minutes) });

            // 2PT
            if (ln.TwoPM > 0) p.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)> { ("twoPointersMade", ln.TwoPM), ("twoPointersAttempted", ln.TwoPA), ("twoPointersPoints", ln.TwoPM * 2) });
            else if (ln.TwoPA > 0) p.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)> { ("twoPointersAttempted", ln.TwoPA) });

            // 3PT
            if (ln.ThreePM > 0) p.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)> { ("threePointersMade", ln.ThreePM), ("threePointersAttempted", ln.ThreePA), ("threePointersPoints", ln.ThreePM * 3) });
            else if (ln.ThreePA > 0) p.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)> { ("threePointersAttempted", ln.ThreePA) });

            // FT
            if (ln.FTM > 0) p.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)> { ("freeThrowsMade", ln.FTM), ("freeThrowsAttempted", ln.FTA) });
            else if (ln.FTA > 0) p.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)> { ("freeThrowsAttempted", ln.FTA) });

            // Misc
            if (ln.Assists > 0) p.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)> { ("assists", ln.Assists) });
            if (ln.OReb + ln.DReb > 0) p.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)> { ("rebounds", ln.OReb + ln.DReb) });
            if (ln.Steals > 0) p.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)> { ("steals", ln.Steals) });
            if (ln.Blocks > 0) p.GetLatestSeason().UpdateMatch(matchID, new List<(string, int)> { ("blocks", ln.Blocks) });
        }
    }

    // -------------------- Utility --------------------

    private static float To01(float v) => Mathf.Clamp01(v / 99f);
    private static float Normalize01(float v) => Mathf.Clamp01(v / 99f);

    private float OffensiveImpact(Player p)
    {
        // Any weighted mix you prefer; higher = more shots
        return
            0.55f * GetSkill01(p, Skill.Three_Point_Shooting) +
            0.55f * GetSkill01(p, Skill.Short_Range_Shooting) +
            0.35f * GetSkill01(p, Skill.Mid_Range_Shooting) +
            0.35f * GetSkill01(p, Skill.Dunking) +
            0.40f * GetSkill01(p, Skill.Layups) +
            0.25f * GetSkill01(p, Skill.Playmaking);
    }

    private static Func<Player, float> S(Skill skill) =>
        p =>
        {
            var it = p.GetSkills().FirstOrDefault(x => x.GetSkill() == skill);
            return it != null ? p.GetRatingForSkillWithItem(it) : 50f;
        };

    private static float GetSkill01(Player p, Skill skill)
    {
        var it = p.GetSkills().FirstOrDefault(x => x.GetSkill() == skill);
        float val = it != null ? p.GetRatingForSkillWithItem(it) : 50f;
        return Mathf.Clamp01(val / 99f);
    }

    private static float Avg(IEnumerable<Player> players, Func<Player, float> f) =>
        players.Any() ? players.Average(f) : 50f;

    private static int ClampRound(float v, int min, int max) => Mathf.Clamp(Mathf.RoundToInt(v), min, max);

    private static int Take(ref int pool, int want)
    {
        int take = Mathf.Clamp(want, 0, Mathf.Max(0, pool));
        pool -= take;
        return take;
    }

    private static void DistributeRemainder(List<Player> players, ref int remainder, Action<Player> giveOne)
    {
        while (remainder > 0 && players.Count > 0)
        {
            var p = players[UnityEngine.Random.Range(0, players.Count)];
            giveOne(p);
            remainder--;
        }
    }

    private static void BalanceToTotal(Dictionary<Player, int> minutes, int targetTotal)
    {
        int sum = minutes.Values.Sum();
        while (sum != targetTotal)
        {
            var p = minutes.Keys.ElementAt(UnityEngine.Random.Range(0, minutes.Count));
            int delta = sum < targetTotal ? +1 : -1;
            int newVal = Mathf.Clamp(minutes[p] + delta, 0, 48);
            if (newVal != minutes[p]) { minutes[p] = newVal; sum += delta; }
        }
    }

    private static float RandNormal(float mean, float stdev)
    {
        // Box–Muller
        float u1 = 1f - UnityEngine.Random.value;
        float u2 = 1f - UnityEngine.Random.value;
        float randStdNormal = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Sin(2f * Mathf.PI * u2);
        return mean + stdev * randStdNormal;
    }

    // -------------------- Data structs --------------------

    private class TeamSummary
    {
        public int TeamID;
        public List<Player> Players;
        public float Offense;   // 0..1
        public float Defense;   // 0..1
        public float Pace;      // 0..1
        public float ThreeRate; // 0..1
    }

    private class ShotProfile
    {
        public float ThreeShare;
        public float RimShare;
        public float MidShare;
    }

    private class PlayerLine
    {
        public int Minutes;
        public int TwoPM, TwoPA;
        public int ThreePM, ThreePA;
        public int FTM, FTA;
        public int OReb, DReb;
        public int Assists;
        public int Steals;
        public int Blocks;
    }

    private class TeamBox
    {
        public int TeamID;
        public int TeamPoints;
        public Dictionary<Player, PlayerLine> Lines = new Dictionary<Player, PlayerLine>();
    }
}
