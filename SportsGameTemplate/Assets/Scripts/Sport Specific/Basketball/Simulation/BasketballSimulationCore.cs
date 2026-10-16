using System;
using System.Collections.Generic;
using System.Linq;

public sealed class BasketballRandom
{
    private readonly Random _random;

    public BasketballRandom(int seed)
    {
        _random = new Random(seed);
    }

    public double Value => _random.NextDouble();
    public bool Chance(float probability) => Value < Math.Max(0d, Math.Min(1d, probability));
    public int Range(int minimumInclusive, int maximumExclusive) => _random.Next(minimumInclusive, maximumExclusive);
    public float Range(float minimumInclusive, float maximumInclusive) => minimumInclusive + (float)Value * (maximumInclusive - minimumInclusive);

    public T Weighted<T>(IReadOnlyList<T> items, Func<T, float> weightSelector)
    {
        if (items == null || items.Count == 0)
            throw new InvalidOperationException("Cannot select from an empty collection.");

        float total = 0f;
        for (int i = 0; i < items.Count; i++)
            total += Math.Max(0f, weightSelector(items[i]));

        if (total <= 0f)
            return items[Range(0, items.Count)];

        float roll = Range(0f, total);
        for (int i = 0; i < items.Count; i++)
        {
            roll -= Math.Max(0f, weightSelector(items[i]));
            if (roll <= 0f)
                return items[i];
        }

        return items[items.Count - 1];
    }
}

public static class BasketballEventReducer
{
    public static BasketballMatchState CreateInitialState(BasketballSimulationRequest request)
    {
        IEnumerable<BasketballPlayerProfile> players = request.HomeTeam.Players.Concat(request.AwayTeam.Players);
        return new BasketballMatchState(request.HomeTeamId, request.AwayTeamId, players);
    }

    public static void Apply(BasketballMatchState state, BasketballMatchEvent matchEvent)
    {
        AddElapsedTime(state, matchEvent.TimeSincePreviousEvent);
        state.CurrentClock = matchEvent.Clock;
        state.LastEventIndex = matchEvent.Index;
        state.CurrentEvent = matchEvent;
        if (matchEvent.PossessionTeamId >= 0)
            state.PossessionTeamId = matchEvent.PossessionTeamId;

        switch (matchEvent.EventType)
        {
            case BasketballEventType.PeriodStarted:
                state.TeamFoulsInternal[state.HomeTeamId] = 0;
                state.TeamFoulsInternal[state.AwayTeamId] = 0;
                break;
            case BasketballEventType.ShotAttempt:
                ApplyShot(state, matchEvent);
                break;
            case BasketballEventType.FreeThrow:
                ApplyFreeThrow(state, matchEvent);
                break;
            case BasketballEventType.Rebound:
                ApplyRebound(state, matchEvent);
                break;
            case BasketballEventType.Turnover:
                GetPlayer(state, matchEvent.PlayerId).Turnovers++;
                break;
            case BasketballEventType.Steal:
                GetPlayer(state, matchEvent.PlayerId).Steals++;
                break;
            case BasketballEventType.Block:
                GetPlayer(state, matchEvent.PlayerId).Blocks++;
                break;
            case BasketballEventType.Foul:
                ApplyFoul(state, matchEvent);
                break;
            case BasketballEventType.Substitution:
                ApplySubstitution(state, matchEvent);
                break;
        }

        if (matchEvent.Points > 0)
        {
            if (matchEvent.TeamId == state.HomeTeamId)
                state.HomeScore += matchEvent.Points;
            else if (matchEvent.TeamId == state.AwayTeamId)
                state.AwayScore += matchEvent.Points;
        }
    }

    public static BasketballMatchState Reduce(BasketballSimulationRequest request, IEnumerable<BasketballMatchEvent> events)
    {
        BasketballMatchState state = CreateInitialState(request);
        foreach (BasketballMatchEvent matchEvent in events)
            Apply(state, matchEvent);
        return state;
    }

    private static void AddElapsedTime(BasketballMatchState state, float elapsedSeconds)
    {
        int seconds = Math.Max(0, (int)Math.Round(elapsedSeconds));
        foreach (string playerId in state.HomeLineupInternal.Concat(state.AwayLineupInternal))
            GetPlayer(state, playerId).SecondsPlayed += seconds;
    }

    private static void ApplyShot(BasketballMatchState state, BasketballMatchEvent matchEvent)
    {
        BasketballPlayerBoxScore shooter = GetPlayer(state, matchEvent.PlayerId);
        if (matchEvent.ShotType == BasketballShotType.ThreePointer)
        {
            shooter.ThreePointersAttempted++;
            if (matchEvent.IsMade)
                shooter.ThreePointersMade++;
        }
        else
        {
            shooter.TwoPointersAttempted++;
            if (matchEvent.IsMade)
                shooter.TwoPointersMade++;
        }

        if (matchEvent.IsMade && !string.IsNullOrEmpty(matchEvent.SecondaryPlayerId))
            GetPlayer(state, matchEvent.SecondaryPlayerId).Assists++;
    }

    private static void ApplyFreeThrow(BasketballMatchState state, BasketballMatchEvent matchEvent)
    {
        BasketballPlayerBoxScore shooter = GetPlayer(state, matchEvent.PlayerId);
        shooter.FreeThrowsAttempted++;
        if (matchEvent.IsMade)
            shooter.FreeThrowsMade++;
    }

    private static void ApplyRebound(BasketballMatchState state, BasketballMatchEvent matchEvent)
    {
        BasketballPlayerBoxScore rebounder = GetPlayer(state, matchEvent.PlayerId);
        if (matchEvent.ReboundType == BasketballReboundType.Offensive)
            rebounder.OffensiveRebounds++;
        else
            rebounder.DefensiveRebounds++;
    }

    private static void ApplyFoul(BasketballMatchState state, BasketballMatchEvent matchEvent)
    {
        GetPlayer(state, matchEvent.PlayerId).PersonalFouls++;
        if (!state.TeamFoulsInternal.ContainsKey(matchEvent.TeamId))
            state.TeamFoulsInternal[matchEvent.TeamId] = 0;
        state.TeamFoulsInternal[matchEvent.TeamId]++;
    }

    private static void ApplySubstitution(BasketballMatchState state, BasketballMatchEvent matchEvent)
    {
        List<string> lineup = matchEvent.TeamId == state.HomeTeamId ? state.HomeLineupInternal : state.AwayLineupInternal;
        if (!string.IsNullOrEmpty(matchEvent.OutgoingPlayerId))
            lineup.Remove(matchEvent.OutgoingPlayerId);
        if (!string.IsNullOrEmpty(matchEvent.IncomingPlayerId) && !lineup.Contains(matchEvent.IncomingPlayerId))
            lineup.Add(matchEvent.IncomingPlayerId);
    }

    private static BasketballPlayerBoxScore GetPlayer(BasketballMatchState state, string playerId)
    {
        if (string.IsNullOrEmpty(playerId) || !state.PlayerStatsInternal.TryGetValue(playerId, out BasketballPlayerBoxScore player))
            throw new InvalidOperationException($"Event references unknown player '{playerId}'.");
        return player;
    }
}

public sealed class BasketballValidationResult
{
    private readonly List<string> _errors = new List<string>();
    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<string> Errors => _errors.AsReadOnly();
    internal void Add(string error) => _errors.Add(error);
}

public static class BasketballSimulationValidator
{
    public static BasketballValidationResult Validate(BasketballSimulationRequest request, IReadOnlyList<BasketballMatchEvent> events)
    {
        BasketballValidationResult result = new BasketballValidationResult();
        BasketballMatchState state = BasketballEventReducer.CreateInitialState(request);
        BasketballGameClock previousClock = new BasketballGameClock(1, 720);

        for (int i = 0; i < events.Count; i++)
        {
            BasketballMatchEvent matchEvent = events[i];
            if (matchEvent.Index != i)
                result.Add($"Event at collection position {i} has index {matchEvent.Index}.");
            if (i > 0 && matchEvent.Clock.CompareTo(previousClock) < 0)
                result.Add($"Event {i} is not chronologically ordered.");

            try
            {
                BasketballEventReducer.Apply(state, matchEvent);
            }
            catch (Exception exception)
            {
                result.Add($"Event {i} cannot be reduced: {exception.Message}");
                continue;
            }

            if (state.HomeScore != matchEvent.HomeScore || state.AwayScore != matchEvent.AwayScore)
                result.Add($"Event {i} stores {matchEvent.HomeScore}-{matchEvent.AwayScore}, but events reduce to {state.HomeScore}-{state.AwayScore}.");

            if (matchEvent.EventType == BasketballEventType.PossessionStarted &&
                (state.HomeLineup.Count != 5 || state.AwayLineup.Count != 5))
                result.Add($"Possession at event {i} began without five players per team.");

            previousClock = matchEvent.Clock;
        }

        int homePlayerPoints = state.PlayerStatsInternal.Values.Where(x => x.TeamId == request.HomeTeamId).Sum(x => x.Points);
        int awayPlayerPoints = state.PlayerStatsInternal.Values.Where(x => x.TeamId == request.AwayTeamId).Sum(x => x.Points);
        if (homePlayerPoints != state.HomeScore)
            result.Add($"Home player points ({homePlayerPoints}) do not reconcile with score ({state.HomeScore}).");
        if (awayPlayerPoints != state.AwayScore)
            result.Add($"Away player points ({awayPlayerPoints}) do not reconcile with score ({state.AwayScore}).");
        if (events.Count == 0 || events[events.Count - 1].EventType != BasketballEventType.GameEnded)
            result.Add("Timeline does not end with GameEnded.");
        if (state.HomeScore == state.AwayScore)
            result.Add("Completed basketball match is tied.");

        return result;
    }
}
