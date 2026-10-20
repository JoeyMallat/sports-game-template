using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class BasketballMatchSimulator : MatchSimulator
{
    [SerializeField] BasketballShotHeatmap _shotHeatmap;
    [SerializeField] int _seedSalt = 7919;
    [SerializeField][Min(1)] int _timelineCheckpointInterval = 50;
    [SerializeField] BasketballSimulationSettings _settings = new BasketballSimulationSettings();

    public void SimulateMatch(Match match, int teamToForceWin)
    {
        if (match == null)
            throw new ArgumentNullException(nameof(match));
        if (match.GetMatchStatus())
            throw new InvalidOperationException($"Match {match.GetMatchID()} has already been played.");

        Team homeTeam = LeagueSystem.Instance.GetTeam(match.GetHomeTeamID());
        Team awayTeam = LeagueSystem.Instance.GetTeam(match.GetAwayTeamID());
        BasketballSimulationRequest request = BuildRequest(match, homeTeam, awayTeam, teamToForceWin);
        BasketballSimulationResult result = SimulateWithForcedWinner(request);
        BasketballValidationResult validation = BasketballSimulationValidator.Validate(request, result.Events);
        if (!validation.IsValid)
            throw new InvalidOperationException($"Detailed match simulation failed validation: {string.Join(" | ", validation.Errors)}");

        BasketballMatchTimeline timeline = new BasketballMatchTimeline(request, result.Events, Math.Max(1, _timelineCheckpointInterval));
        match.AttachTimeline(timeline);
        match.SetResult(result.FinalState.HomeScore, result.FinalState.AwayScore);
        CommitBoxScores(homeTeam, result.FinalState, match.GetMatchID());
        CommitBoxScores(awayTeam, result.FinalState, match.GetMatchID());
        match.EndMatch();
    }

    private static BasketballSimulationResult SimulateWithForcedWinner(BasketballSimulationRequest request)
    {
        const int maximumAttempts = 64;
        int originalSeed = request.Seed;
        for (int attempt = 0; attempt < maximumAttempts; attempt++)
        {
            request.Seed = unchecked(originalSeed + attempt * 104729);
            BasketballSimulationResult result = new EventDrivenBasketballEngine().Simulate(request);
            if (request.TeamToForceWin < 0 ||
                (request.TeamToForceWin == request.HomeTeamId && result.FinalState.HomeScore > result.FinalState.AwayScore) ||
                (request.TeamToForceWin == request.AwayTeamId && result.FinalState.AwayScore > result.FinalState.HomeScore))
                return result;
        }
        throw new InvalidOperationException($"Unable to produce the requested winner after {maximumAttempts} deterministic attempts.");
    }

    public BasketballSimulationRequest BuildRequest(Match match, Team homeTeam, Team awayTeam, int teamToForceWin = -1)
    {
        if (_settings == null)
            _settings = new BasketballSimulationSettings();

        int seed = unchecked(match.GetMatchID() * 73856093 ^ homeTeam.GetTeamID() * 19349663 ^ awayTeam.GetTeamID() * 83492791 ^ _seedSalt);
        return new BasketballSimulationRequest
        {
            MatchId = match.GetMatchID(),
            Seed = seed,
            HomeTeamId = homeTeam.GetTeamID(),
            AwayTeamId = awayTeam.GetTeamID(),
            TeamToForceWin = teamToForceWin,
            HomeTeam = BuildTeamProfile(homeTeam, _settings),
            AwayTeam = BuildTeamProfile(awayTeam, _settings),
            ShotHeatmap = _shotHeatmap,
            Settings = _settings
        };
    }

    private static BasketballTeamProfile BuildTeamProfile(Team team, BasketballSimulationSettings settings)
    {
        BasketballTeamProfile profile = new BasketballTeamProfile
        {
            TeamId = team.GetTeamID(),
            Tactics = team.GetBasketballTactics()
        };
        foreach (Player player in team.GetPlayersFromTeam())
        {
            profile.Players.Add(new BasketballPlayerProfile
            {
                PlayerId = player.GetTradeableID(),
                TeamId = team.GetTeamID(),
                Position = player.GetPosition(),
                TargetMinutes = player.GetMinutes(),
                Overall = SafeOverall(player),
                Speed = SkillRating(player, Skill.Speed),
                ShortRange = SkillRating(player, Skill.Short_Range_Shooting),
                MidRange = SkillRating(player, Skill.Mid_Range_Shooting),
                ThreePoint = SkillRating(player, Skill.Three_Point_Shooting),
                Dunking = SkillRating(player, Skill.Dunking),
                Layups = SkillRating(player, Skill.Layups),
                Stealing = SkillRating(player, Skill.Stealing),
                Passing = SkillRating(player, Skill.Passing),
                Dribbling = SkillRating(player, Skill.Dribbling),
                Playmaking = SkillRating(player, Skill.Playmaking),
                Rebounding = SkillRating(player, Skill.Rebounding),
                Blocking = SkillRating(player, Skill.Blocking),
                Stamina = Mathf.RoundToInt(
                    SkillRating(player, Skill.Speed) * settings.StaminaSpeedWeight +
                    SafeOverall(player) * settings.StaminaOverallWeight)
            });
        }
        return profile;
    }

    private static int SkillRating(Player player, Skill skill)
    {
        PlayerSkill playerSkill = player.GetSkills().FirstOrDefault(candidate => candidate.GetSkill() == skill);
        return playerSkill == null ? 50 : Mathf.Clamp(player.GetRatingForSkillWithItem(playerSkill), 0, 99);
    }

    private static int SafeOverall(Player player)
    {
        try
        {
            return Mathf.Clamp(player.CalculateRatingForPosition(), 0, 99);
        }
        catch
        {
            return Mathf.RoundToInt((float)player.GetSkills().Average(skill => skill.GetRatingForSkill()));
        }
    }

    private static void CommitBoxScores(Team team, BasketballMatchState state, int matchId)
    {
        Dictionary<string, Player> players = team.GetPlayersFromTeam().ToDictionary(player => player.GetTradeableID());
        foreach (BasketballPlayerBoxScore boxScore in state.PlayerBoxScores.Values.Where(box => box.TeamId == team.GetTeamID()))
        {
            if (!players.TryGetValue(boxScore.PlayerId, out Player player))
                continue;
            int totalRebounds = boxScore.OffensiveRebounds + boxScore.DefensiveRebounds;
            player.GetLatestSeason().UpdateMatch(matchId, new List<(string, int)>
            {
                ("minutes", boxScore.Minutes),
                ("assists", boxScore.Assists),
                ("steals", boxScore.Steals),
                ("rebounds", totalRebounds),
                ("offensiveRebounds", boxScore.OffensiveRebounds),
                ("defensiveRebounds", boxScore.DefensiveRebounds),
                ("blocks", boxScore.Blocks),
                ("turnovers", boxScore.Turnovers),
                ("personalFouls", boxScore.PersonalFouls),
                ("freeThrowsAttempted", boxScore.FreeThrowsAttempted),
                ("freeThrowsMade", boxScore.FreeThrowsMade),
                ("twoPointersAttempted", boxScore.TwoPointersAttempted),
                ("twoPointersMade", boxScore.TwoPointersMade),
                ("twoPointersPoints", boxScore.TwoPointersMade * 2),
                ("threePointersAttempted", boxScore.ThreePointersAttempted),
                ("threePointersMade", boxScore.ThreePointersMade),
                ("threePointersPoints", boxScore.ThreePointersMade * 3)
            });
        }
    }
}
