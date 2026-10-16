using System;

public static class BasketballMatchSimulationRouter
{
    public static MatchSimulator Select(Match match)
    {
        if (match == null)
            throw new ArgumentNullException(nameof(match));

        Config config = ConfigManager.Instance.GetCurrentConfig();
        int userTeamId = GameManager.Instance.GetTeamID();
        bool isUserMatch = match.IsMyTeamMatch(userTeamId);
        if (isUserMatch && config.MatchSimulator != null)
            return config.MatchSimulator;
        if (config.QuickMatchSimulator != null)
            return config.QuickMatchSimulator;
        if (config.MatchSimulator != null)
            return config.MatchSimulator;
        throw new InvalidOperationException("No match simulator is configured.");
    }

    public static void Simulate(Match match, int teamToForceWin = -1)
    {
        Select(match).SimulateMatch(match, teamToForceWin);
    }
}
