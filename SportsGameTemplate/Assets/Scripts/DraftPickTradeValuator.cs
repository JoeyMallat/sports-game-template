using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DraftPickTradeValuator
{
    public static TradeValueResult Evaluate(DraftPick pick, TradeValueContext context, IReadOnlyList<Team> teams)
    {
        TradeValueSettings settings = context.Settings;
        int totalPicks = Mathf.Max(1, ConfigManager.Instance.GetCurrentConfig().GetTotalDraftPicks());
        int projectedPick = GetProjectedPickNumber(pick, teams, totalPicks);
        float normalizedSlot = (projectedPick - 1f) / totalPicks;
        float slotMultiplier = Mathf.Pow(1f - Mathf.Clamp01(normalizedSlot), settings.PickValueExponent);
        float slotValue = settings.FirstPickValue * Mathf.Max(0.08f, slotMultiplier);

        int yearsAway = Mathf.Max(0, pick.GetDraftSeason() - context.CurrentSeason);
        float futureMultiplier = Mathf.Pow(settings.FuturePickDiscount, yearsAway);
        float classQuality = Mathf.Clamp(
            context.GetDraftClassQuality(pick.GetDraftSeason()),
            settings.MinimumClassMultiplier,
            settings.MaximumClassMultiplier);

        return new TradeValueResult(slotValue, futureMultiplier, classQuality);
    }

    private static int GetProjectedPickNumber(DraftPick pick, IReadOnlyList<Team> teams, int totalPicks)
    {
        if (pick.GetDraftSeason() <= (GameManager.Instance == null ? 0 : GameManager.Instance.GetCurrentSeason()))
            return Mathf.Clamp(pick.GetTotalPickNumber(), 1, totalPicks);

        Team originalTeam = teams?.FirstOrDefault(team => team.GetTeamID() == pick.GetOriginalTeamID());
        if (originalTeam == null || teams.Count == 0)
            return Mathf.Clamp(pick.GetTotalPickNumber(), 1, totalPicks);

        List<Team> rankedTeams = teams
            .OrderBy(team => team.GetCurrentSeasonStats().GetWinPercentage())
            .ThenBy(team => team.GetAverageTeamRating())
            .ToList();
        int projectedFirstRoundSlot = rankedTeams.IndexOf(originalTeam) + 1;
        int picksPerRound = ConfigManager.Instance.GetCurrentConfig().PlayersPerDraftRound;
        int round = pick.GetPickData().Item1;

        return Mathf.Clamp((round - 1) * picksPerRound + projectedFirstRoundSlot, 1, totalPicks);
    }
}
