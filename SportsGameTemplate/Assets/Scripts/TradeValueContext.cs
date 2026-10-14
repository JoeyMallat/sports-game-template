using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TradeValueContext
{
    public TradeValueSettings Settings { get; }
    public int CurrentSeason { get; }
    public float LeagueAverageRating { get; }
    public float LeagueAveragePoints { get; }
    public float LeagueAverageAssists { get; }
    public float LeagueAverageRebounds { get; }
    public float LeagueAverageSalary { get; }

    private readonly Dictionary<int, float> _draftClassQuality;

    private TradeValueContext(
        TradeValueSettings settings,
        int currentSeason,
        float leagueAverageRating,
        float leagueAveragePoints,
        float leagueAverageAssists,
        float leagueAverageRebounds,
        float leagueAverageSalary,
        Dictionary<int, float> draftClassQuality)
    {
        Settings = settings;
        CurrentSeason = currentSeason;
        LeagueAverageRating = leagueAverageRating;
        LeagueAveragePoints = leagueAveragePoints;
        LeagueAverageAssists = leagueAverageAssists;
        LeagueAverageRebounds = leagueAverageRebounds;
        LeagueAverageSalary = leagueAverageSalary;
        _draftClassQuality = draftClassQuality;
    }

    public static TradeValueContext Create(IEnumerable<Team> teams, TradeValueSettings settings = null, Dictionary<int, float> draftClassQuality = null)
    {
        settings ??= TradeValueSettings.Default;
        List<Player> players = teams?.SelectMany(team => team.GetPlayersFromTeam()).ToList() ?? new List<Player>();
        int currentSeason = GameManager.Instance == null ? 0 : GameManager.Instance.GetCurrentSeason();

        if (players.Count == 0)
            return new TradeValueContext(settings, currentSeason, 50f, 0f, 0f, 0f, 1f, draftClassQuality ?? new Dictionary<int, float>());

        return new TradeValueContext(
            settings,
            currentSeason,
            players.Average(player => (float)player.CalculateRatingForPosition()),
            players.Average(player => player.GetLatestSeason().GetAveragePoints()),
            players.Average(player => player.GetLatestSeason().GetAverageOfStat("assists")),
            players.Average(player => player.GetLatestSeason().GetAverageOfStat("rebounds")),
            Mathf.Max(1f, players.Average(player => (float)player.GetContract().GetYearlySalary())),
            draftClassQuality ?? new Dictionary<int, float>());
    }

    public float GetDraftClassQuality(int season)
    {
        return _draftClassQuality.TryGetValue(season, out float quality) ? quality : 1f;
    }
}
