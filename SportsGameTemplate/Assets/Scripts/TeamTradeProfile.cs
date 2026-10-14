using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TeamTradeDirection
{
    Rebuilding,
    Retooling,
    Neutral,
    PlayoffTeam,
    Contender
}

public class TeamTradeProfile
{
    public Team Team { get; }
    public TeamTradeDirection Direction { get; }
    public float AverageAge { get; }
    public float SalaryPressure { get; }

    private readonly Dictionary<string, float> _positionNeed;

    private TeamTradeProfile(Team team, TeamTradeDirection direction, float averageAge, float salaryPressure, Dictionary<string, float> positionNeed)
    {
        Team = team;
        Direction = direction;
        AverageAge = averageAge;
        SalaryPressure = salaryPressure;
        _positionNeed = positionNeed;
    }

    public static TeamTradeProfile Create(Team team, IReadOnlyList<Team> leagueTeams)
    {
        List<Player> roster = team.GetPlayersFromTeam();
        float averageAge = roster.Count == 0 ? 25f : roster.Sum(player => player.GetAge()) / (float)roster.Count;
        float winPercentage = team.GetCurrentSeasonStats().GetWinPercentage();
        float teamRating = team.GetAverageTeamRating();
        float leagueRating = leagueTeams.Count == 0 ? teamRating : leagueTeams.Sum(candidate => candidate.GetAverageTeamRating()) / (float)leagueTeams.Count;
        TeamTradeDirection direction = GetDirection(winPercentage, teamRating, leagueRating, averageAge);
        float effectiveCap = ConfigManager.Instance.GetCurrentConfig().SalaryCap;
        float salaryPressure = Mathf.Clamp01(team.GetTotalSalaryAmount() / Mathf.Max(1f, effectiveCap));

        Dictionary<string, float> positionNeed = roster
            .GroupBy(player => player.GetPosition())
            .ToDictionary(
                group => group.Key,
                group =>
                {
                    List<Player> topPlayers = group.OrderByDescending(player => player.CalculateRatingForPosition()).Take(2).ToList();
                    float depthRating = topPlayers.Sum(player => player.CalculateRatingForPosition()) / (float)topPlayers.Count;
                    return Mathf.Clamp(1.2f - depthRating / 75f, 0f, 0.35f);
                });

        return new TeamTradeProfile(team, direction, averageAge, salaryPressure, positionNeed);
    }

    public float GetAssetMultiplier(ITradeable asset, TradeValueSettings settings)
    {
        float adjustment = 0f;
        if (asset is DraftPick)
        {
            adjustment = Direction == TeamTradeDirection.Rebuilding ? 0.18f
                : Direction == TeamTradeDirection.Retooling ? 0.1f
                : Direction == TeamTradeDirection.Contender ? -0.12f
                : Direction == TeamTradeDirection.PlayoffTeam ? -0.06f
                : 0f;
        }
        else if (asset is Player player)
        {
            bool winNow = Direction == TeamTradeDirection.Contender || Direction == TeamTradeDirection.PlayoffTeam;
            bool building = Direction == TeamTradeDirection.Rebuilding || Direction == TeamTradeDirection.Retooling;
            adjustment += winNow ? Mathf.InverseLerp(68f, 90f, player.CalculateRatingForPosition()) * 0.12f : 0f;
            adjustment += building ? Mathf.InverseLerp(30f, 19f, player.GetAge()) * 0.14f : 0f;
            adjustment += _positionNeed.TryGetValue(player.GetPosition(), out float need) ? need : 0.2f;

            if (SalaryPressure > 0.9f && player.GetContract().GetYearlySalary() > ConfigManager.Instance.GetCurrentConfig().SalaryCap * 0.15f)
                adjustment -= 0.12f;
        }

        return 1f + Mathf.Clamp(adjustment, -settings.TeamFitMaximumAdjustment, settings.TeamFitMaximumAdjustment);
    }

    private static TeamTradeDirection GetDirection(float winPercentage, float rating, float leagueRating, float averageAge)
    {
        if (winPercentage >= 0.65f || rating >= leagueRating + 5f)
            return TeamTradeDirection.Contender;
        if (winPercentage >= 0.52f || rating >= leagueRating + 1f)
            return TeamTradeDirection.PlayoffTeam;
        if (winPercentage <= 0.3f || rating <= leagueRating - 5f)
            return averageAge <= 27f ? TeamTradeDirection.Rebuilding : TeamTradeDirection.Retooling;
        if (winPercentage <= 0.42f)
            return TeamTradeDirection.Retooling;

        return TeamTradeDirection.Neutral;
    }
}
