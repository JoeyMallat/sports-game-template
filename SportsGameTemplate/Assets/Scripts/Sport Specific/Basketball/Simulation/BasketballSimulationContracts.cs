using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public enum BasketballEventType
{
    PeriodStarted,
    PeriodEnded,
    PossessionStarted,
    ShotAttempt,
    FreeThrow,
    Rebound,
    Turnover,
    Steal,
    Block,
    Foul,
    Substitution,
    Timeout,
    GameEnded
}

public enum BasketballShotType
{
    None,
    TwoPointJumpShot,
    ThreePointer,
    Layup,
    Dunk,
    FreeThrow
}

public enum BasketballFoulType
{
    None,
    Personal,
    Shooting,
    Offensive
}

public enum BasketballReboundType
{
    None,
    Offensive,
    Defensive
}

public enum BasketballEventImportance
{
    Routine,
    Notable,
    Important,
    Critical
}

[Flags]
public enum BasketballDramaticMoment
{
    None = 0,
    LeadChange = 1,
    TieGame = 2,
    CloseFourthQuarter = 4,
    ClutchAttempt = 8,
    GameWinningAttempt = 16,
    BuzzerBeater = 32,
    Overtime = 64
}

public enum BasketballOffensiveTactic
{
    Balanced,
    PaceAndSpace,
    Perimeter,
    AttackTheRim,
    PostUp,
    Motion
}

[Serializable]
public sealed class BasketballTacticProfile
{
    [SerializeField] BasketballOffensiveTactic _offensiveTactic = BasketballOffensiveTactic.Balanced;
    [Range(0.75f, 1.25f)][SerializeField] float _pace = 1f;
    [Range(0.5f, 1.5f)][SerializeField] float _threePointEmphasis = 1f;
    [Range(0.5f, 1.5f)][SerializeField] float _rimEmphasis = 1f;
    [Range(0.5f, 1.5f)][SerializeField] float _ballMovement = 1f;
    [Range(0.5f, 1.5f)][SerializeField] float _offensiveRebounding = 1f;
    [Range(0.5f, 1.5f)][SerializeField] float _usageConcentration = 1f;
    [Range(0.5f, 1.5f)][SerializeField] float _defensivePressure = 1f;

    public BasketballOffensiveTactic OffensiveTactic => _offensiveTactic;
    public float Pace => Mathf.Clamp(_pace, 0.75f, 1.25f);
    public float ThreePointEmphasis => Mathf.Clamp(_threePointEmphasis, 0.5f, 1.5f);
    public float RimEmphasis => Mathf.Clamp(_rimEmphasis, 0.5f, 1.5f);
    public float BallMovement => Mathf.Clamp(_ballMovement, 0.5f, 1.5f);
    public float OffensiveRebounding => Mathf.Clamp(_offensiveRebounding, 0.5f, 1.5f);
    public float UsageConcentration => Mathf.Clamp(_usageConcentration, 0.5f, 1.5f);
    public float DefensivePressure => Mathf.Clamp(_defensivePressure, 0.5f, 1.5f);

    public static BasketballTacticProfile Balanced => new BasketballTacticProfile();
}

public readonly struct BasketballGameClock : IComparable<BasketballGameClock>
{
    public BasketballGameClock(int period, int secondsRemaining, int regulationPeriods = 4)
    {
        Period = Mathf.Max(1, period);
        SecondsRemaining = Mathf.Max(0, secondsRemaining);
        RegulationPeriods = Mathf.Max(1, regulationPeriods);
    }

    public int Period { get; }
    public int SecondsRemaining { get; }
    public int RegulationPeriods { get; }
    public bool IsOvertime => Period > RegulationPeriods;
    public int Minutes => SecondsRemaining / 60;
    public int Seconds => SecondsRemaining % 60;
    public string Display => $"{Minutes}:{Seconds:00}";

    public int CompareTo(BasketballGameClock other)
    {
        int periodComparison = Period.CompareTo(other.Period);
        return periodComparison != 0 ? periodComparison : other.SecondsRemaining.CompareTo(SecondsRemaining);
    }
}

[Serializable]
public sealed class BasketballMatchEvent
{
    public BasketballMatchEvent(BasketballEventType eventType, BasketballGameClock clock)
    {
        EventType = eventType;
        Clock = clock;
        PlayerId = string.Empty;
        SecondaryPlayerId = string.Empty;
        OutgoingPlayerId = string.Empty;
        IncomingPlayerId = string.Empty;
        Description = string.Empty;
        LinkedEventIndex = -1;
        PossessionTeamId = -1;
        TeamId = -1;
        OpponentTeamId = -1;
        RecommendedAnimationDuration = 0.35f;
    }

    public int Index { get; internal set; }
    public BasketballEventType EventType { get; internal set; }
    public BasketballGameClock Clock { get; internal set; }
    public int TeamId { get; internal set; }
    public int OpponentTeamId { get; internal set; }
    public int PossessionTeamId { get; internal set; }
    public string PlayerId { get; internal set; }
    public string SecondaryPlayerId { get; internal set; }
    public string OutgoingPlayerId { get; internal set; }
    public string IncomingPlayerId { get; internal set; }
    public BasketballShotType ShotType { get; internal set; }
    public BasketballFoulType FoulType { get; internal set; }
    public BasketballReboundType ReboundType { get; internal set; }
    public Vector2 CourtPosition { get; internal set; }
    public float MakeProbability { get; internal set; }
    public bool IsMade { get; internal set; }
    public bool IsBlocked { get; internal set; }
    public bool IsShootingFoul { get; internal set; }
    public int FreeThrowNumber { get; internal set; }
    public int FreeThrowTotal { get; internal set; }
    public int Points { get; internal set; }
    public int HomeScore { get; internal set; }
    public int AwayScore { get; internal set; }
    public int LinkedEventIndex { get; internal set; }
    public float TimeSincePreviousEvent { get; internal set; }
    public BasketballEventImportance Importance { get; internal set; }
    public BasketballDramaticMoment DramaticMoment { get; internal set; }
    public float RecommendedAnimationDuration { get; internal set; }
    public string Description { get; internal set; }
}

[Serializable]
public sealed class BasketballPlayerBoxScore
{
    public BasketballPlayerBoxScore(string playerId, int teamId)
    {
        PlayerId = playerId;
        TeamId = teamId;
    }

    public string PlayerId { get; }
    public int TeamId { get; }
    public int SecondsPlayed { get; internal set; }
    public int Minutes => Mathf.RoundToInt(SecondsPlayed / 60f);
    public int Points => FreeThrowsMade + TwoPointersMade * 2 + ThreePointersMade * 3;
    public int FieldGoalsAttempted => TwoPointersAttempted + ThreePointersAttempted;
    public int FieldGoalsMade => TwoPointersMade + ThreePointersMade;
    public int TwoPointersAttempted { get; internal set; }
    public int TwoPointersMade { get; internal set; }
    public int ThreePointersAttempted { get; internal set; }
    public int ThreePointersMade { get; internal set; }
    public int FreeThrowsAttempted { get; internal set; }
    public int FreeThrowsMade { get; internal set; }
    public int OffensiveRebounds { get; internal set; }
    public int DefensiveRebounds { get; internal set; }
    public int Rebounds => OffensiveRebounds + DefensiveRebounds;
    public int Assists { get; internal set; }
    public int Steals { get; internal set; }
    public int Turnovers { get; internal set; }
    public int Blocks { get; internal set; }
    public int PersonalFouls { get; internal set; }

    internal BasketballPlayerBoxScore Clone()
    {
        return (BasketballPlayerBoxScore)MemberwiseClone();
    }
}

public sealed class BasketballTeamBoxScore
{
    internal BasketballTeamBoxScore(int teamId, Dictionary<string, BasketballPlayerBoxScore> players)
    {
        TeamId = teamId;
        Players = new ReadOnlyDictionary<string, BasketballPlayerBoxScore>(players);
    }

    public int TeamId { get; }
    public IReadOnlyDictionary<string, BasketballPlayerBoxScore> Players { get; }
    public int Points => Players.Values.Sum(x => x.Points);
    public int FieldGoalsAttempted => Players.Values.Sum(x => x.FieldGoalsAttempted);
    public int FieldGoalsMade => Players.Values.Sum(x => x.FieldGoalsMade);
    public int TwoPointersAttempted => Players.Values.Sum(x => x.TwoPointersAttempted);
    public int TwoPointersMade => Players.Values.Sum(x => x.TwoPointersMade);
    public int ThreePointersAttempted => Players.Values.Sum(x => x.ThreePointersAttempted);
    public int ThreePointersMade => Players.Values.Sum(x => x.ThreePointersMade);
    public int FreeThrowsAttempted => Players.Values.Sum(x => x.FreeThrowsAttempted);
    public int FreeThrowsMade => Players.Values.Sum(x => x.FreeThrowsMade);
    public int OffensiveRebounds => Players.Values.Sum(x => x.OffensiveRebounds);
    public int DefensiveRebounds => Players.Values.Sum(x => x.DefensiveRebounds);
    public int Assists => Players.Values.Sum(x => x.Assists);
    public int Rebounds => Players.Values.Sum(x => x.Rebounds);
    public int Steals => Players.Values.Sum(x => x.Steals);
    public int Turnovers => Players.Values.Sum(x => x.Turnovers);
    public int Blocks => Players.Values.Sum(x => x.Blocks);
    public int PersonalFouls => Players.Values.Sum(x => x.PersonalFouls);
}

public sealed class BasketballMatchState
{
    internal BasketballMatchState(int homeTeamId, int awayTeamId, IEnumerable<BasketballPlayerProfile> players)
    {
        HomeTeamId = homeTeamId;
        AwayTeamId = awayTeamId;
        CurrentClock = new BasketballGameClock(1, 720);
        HomeLineupInternal = new List<string>();
        AwayLineupInternal = new List<string>();
        PlayerStatsInternal = players.ToDictionary(x => x.PlayerId, x => new BasketballPlayerBoxScore(x.PlayerId, x.TeamId));
        TeamFoulsInternal = new Dictionary<int, int> { [homeTeamId] = 0, [awayTeamId] = 0 };
        PossessionTeamId = -1;
        LastEventIndex = -1;
    }

    public int HomeTeamId { get; }
    public int AwayTeamId { get; }
    public BasketballGameClock CurrentClock { get; internal set; }
    public int HomeScore { get; internal set; }
    public int AwayScore { get; internal set; }
    public int PossessionTeamId { get; internal set; }
    public int LastEventIndex { get; internal set; }
    public BasketballMatchEvent CurrentEvent { get; internal set; }
    public BasketballMatchEvent PreviousEvent { get; internal set; }
    public BasketballMatchEvent NextEvent { get; internal set; }
    public IReadOnlyList<string> HomeLineup => HomeLineupInternal.AsReadOnly();
    public IReadOnlyList<string> AwayLineup => AwayLineupInternal.AsReadOnly();
    public IReadOnlyDictionary<string, BasketballPlayerBoxScore> PlayerBoxScores => new ReadOnlyDictionary<string, BasketballPlayerBoxScore>(PlayerStatsInternal);
    public IReadOnlyDictionary<int, int> TeamFouls => new ReadOnlyDictionary<int, int>(TeamFoulsInternal);
    public BasketballTeamBoxScore HomeBoxScore => BuildTeamBoxScore(HomeTeamId);
    public BasketballTeamBoxScore AwayBoxScore => BuildTeamBoxScore(AwayTeamId);

    internal List<string> HomeLineupInternal { get; }
    internal List<string> AwayLineupInternal { get; }
    internal Dictionary<string, BasketballPlayerBoxScore> PlayerStatsInternal { get; }
    internal Dictionary<int, int> TeamFoulsInternal { get; }

    internal BasketballMatchState Clone()
    {
        BasketballMatchState clone = new BasketballMatchState(HomeTeamId, AwayTeamId, Array.Empty<BasketballPlayerProfile>())
        {
            CurrentClock = CurrentClock,
            HomeScore = HomeScore,
            AwayScore = AwayScore,
            PossessionTeamId = PossessionTeamId,
            LastEventIndex = LastEventIndex,
            CurrentEvent = CurrentEvent,
            PreviousEvent = PreviousEvent,
            NextEvent = NextEvent
        };
        clone.HomeLineupInternal.AddRange(HomeLineupInternal);
        clone.AwayLineupInternal.AddRange(AwayLineupInternal);
        clone.TeamFoulsInternal.Clear();
        foreach (KeyValuePair<int, int> pair in TeamFoulsInternal)
            clone.TeamFoulsInternal[pair.Key] = pair.Value;
        foreach (KeyValuePair<string, BasketballPlayerBoxScore> pair in PlayerStatsInternal)
            clone.PlayerStatsInternal[pair.Key] = pair.Value.Clone();
        return clone;
    }

    private BasketballTeamBoxScore BuildTeamBoxScore(int teamId)
    {
        Dictionary<string, BasketballPlayerBoxScore> players = PlayerStatsInternal
            .Where(x => x.Value.TeamId == teamId)
            .ToDictionary(x => x.Key, x => x.Value.Clone());
        return new BasketballTeamBoxScore(teamId, players);
    }
}

public sealed class BasketballPlayerProfile
{
    public string PlayerId;
    public int TeamId;
    public string Position;
    public int TargetMinutes;
    public int Overall;
    public int Speed;
    public int ShortRange;
    public int MidRange;
    public int ThreePoint;
    public int Dunking;
    public int Layups;
    public int Stealing;
    public int Passing;
    public int Dribbling;
    public int Playmaking;
    public int Rebounding;
    public int Blocking;
    public int Stamina = 75;
}

public sealed class BasketballTeamProfile
{
    public int TeamId;
    public List<BasketballPlayerProfile> Players = new List<BasketballPlayerProfile>();
    public BasketballTacticProfile Tactics = BasketballTacticProfile.Balanced;
}

public sealed class BasketballSimulationRequest
{
    public int MatchId;
    public int Seed;
    public int HomeTeamId;
    public int AwayTeamId;
    public int TeamToForceWin = -1;
    public BasketballTeamProfile HomeTeam;
    public BasketballTeamProfile AwayTeam;
    public BasketballShotHeatmap ShotHeatmap;
}

public sealed class BasketballSimulationResult
{
    public BasketballSimulationResult(int seed, IReadOnlyList<BasketballMatchEvent> events, BasketballMatchState finalState)
    {
        Seed = seed;
        Events = events;
        FinalState = finalState;
    }

    public int Seed { get; }
    public IReadOnlyList<BasketballMatchEvent> Events { get; }
    public BasketballMatchState FinalState { get; }
}
