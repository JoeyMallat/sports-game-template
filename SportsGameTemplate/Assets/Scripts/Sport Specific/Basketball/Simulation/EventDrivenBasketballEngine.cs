using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class EventDrivenBasketballEngine
{
    private const int RegulationPeriods = 4;
    private const int RegulationPeriodSeconds = 720;
    private const int OvertimeSeconds = 300;
    private const int ShotClockSeconds = 24;
    private readonly List<BasketballMatchEvent> _events = new List<BasketballMatchEvent>();
    private readonly BasketballRotationPlanner _rotation = new BasketballRotationPlanner();
    private BasketballSimulationRequest _request;
    private BasketballRandom _random;
    private BasketballMatchState _state;
    private int _homeTimeouts;
    private int _awayTimeouts;

    public BasketballSimulationResult Simulate(BasketballSimulationRequest request)
    {
        ValidateRequest(request);
        _request = request;
        _random = new BasketballRandom(request.Seed);
        _state = BasketballEventReducer.CreateInitialState(request);
        _events.Clear();
        _homeTimeouts = 7;
        _awayTimeouts = 7;

        int possessionTeamId = _random.Chance(0.5f) ? request.HomeTeamId : request.AwayTeamId;
        int period = 1;
        do
        {
            int periodSeconds = period <= RegulationPeriods ? RegulationPeriodSeconds : OvertimeSeconds;
            BasketballGameClock clock = new BasketballGameClock(period, periodSeconds, RegulationPeriods);
            Emit(new BasketballMatchEvent(BasketballEventType.PeriodStarted, clock)
            {
                PossessionTeamId = possessionTeamId,
                Description = period <= RegulationPeriods ? $"Quarter {period} started" : $"Overtime {period - RegulationPeriods} started",
                Importance = period > RegulationPeriods ? BasketballEventImportance.Important : BasketballEventImportance.Notable,
                RecommendedAnimationDuration = 0.8f
            });
            ApplyRotation(clock, true);

            int secondsRemaining = periodSeconds;
            int secondsSinceRotation = 0;
            while (secondsRemaining > 0)
            {
                BasketballTeamProfile offense = GetTeam(possessionTeamId);
                int defenseTeamId = OtherTeamId(possessionTeamId);
                float pace = offense.Tactics?.Pace ?? 1f;
                int maximumDuration = Math.Min(ShotClockSeconds, secondsRemaining);
                int duration = Math.Min(maximumDuration, Math.Max(3, _random.Range(8, 25) - Mathf.RoundToInt((pace - 1f) * 12f)));
                BasketballGameClock possessionStartClock = new BasketballGameClock(period, secondsRemaining, RegulationPeriods);
                Emit(new BasketballMatchEvent(BasketballEventType.PossessionStarted, possessionStartClock)
                {
                    TeamId = possessionTeamId,
                    OpponentTeamId = defenseTeamId,
                    PossessionTeamId = possessionTeamId,
                    Description = "Possession started",
                    RecommendedAnimationDuration = 0.1f
                });

                secondsRemaining -= duration;
                BasketballGameClock resultClock = new BasketballGameClock(period, secondsRemaining, RegulationPeriods);
                int nextPossession = SimulatePossession(offense, GetTeam(defenseTeamId), resultClock, duration);
                _rotation.Advance(_state.HomeLineup, _state.AwayLineup, duration);
                possessionTeamId = nextPossession;
                secondsSinceRotation += duration;

                MaybeCallTimeout(resultClock, possessionTeamId);
                if (secondsSinceRotation >= 90 || HasFoulOutOnCourt())
                {
                    ApplyRotation(resultClock, false);
                    secondsSinceRotation = 0;
                }
            }

            BasketballGameClock endClock = new BasketballGameClock(period, 0, RegulationPeriods);
            Emit(new BasketballMatchEvent(BasketballEventType.PeriodEnded, endClock)
            {
                PossessionTeamId = possessionTeamId,
                Description = period <= RegulationPeriods ? $"Quarter {period} ended" : $"Overtime {period - RegulationPeriods} ended",
                Importance = period >= RegulationPeriods ? BasketballEventImportance.Important : BasketballEventImportance.Notable,
                RecommendedAnimationDuration = 0.8f
            });
            period++;
        }
        while (period <= RegulationPeriods || _state.HomeScore == _state.AwayScore);

        BasketballGameClock finalClock = new BasketballGameClock(period - 1, 0, RegulationPeriods);
        Emit(new BasketballMatchEvent(BasketballEventType.GameEnded, finalClock)
        {
            PossessionTeamId = -1,
            Description = "Game ended",
            Importance = BasketballEventImportance.Critical,
            RecommendedAnimationDuration = 1.5f
        });

        return new BasketballSimulationResult(request.Seed, _events.AsReadOnly(), _state.Clone());
    }

    private int SimulatePossession(BasketballTeamProfile offense, BasketballTeamProfile defense, BasketballGameClock clock, int duration)
    {
        IReadOnlyList<string> offenseLineup = GetLineup(offense.TeamId);
        IReadOnlyList<string> defenseLineup = GetLineup(defense.TeamId);
        BasketballPlayerProfile ballHandler = SelectShooter(offense, offenseLineup);
        float pressure = defense.Tactics?.DefensivePressure ?? 1f;
        float turnoverChance = Mathf.Clamp(0.16f + pressure * 0.025f - (ballHandler.Dribbling + ballHandler.Playmaking) / 1100f, 0.07f, 0.22f);

        if (_random.Chance(turnoverChance))
        {
            bool offensiveFoul = _random.Chance(0.16f);
            if (offensiveFoul)
            {
                EmitFoul(ballHandler, offense.TeamId, defense.TeamId, BasketballFoulType.Offensive, clock, duration, -1);
            }
            Emit(new BasketballMatchEvent(BasketballEventType.Turnover, clock)
            {
                TeamId = offense.TeamId,
                OpponentTeamId = defense.TeamId,
                PossessionTeamId = defense.TeamId,
                PlayerId = ballHandler.PlayerId,
                TimeSincePreviousEvent = offensiveFoul ? 0f : duration,
                Description = offensiveFoul ? "Offensive foul turnover" : "Turnover",
                Importance = BasketballEventImportance.Notable,
                RecommendedAnimationDuration = 0.65f
            });
            if (!offensiveFoul && _random.Chance(0.62f))
            {
                BasketballPlayerProfile stealer = SelectDefender(defense, defenseLineup, player => player.Stealing);
                Emit(new BasketballMatchEvent(BasketballEventType.Steal, clock)
                {
                    TeamId = defense.TeamId,
                    OpponentTeamId = offense.TeamId,
                    PossessionTeamId = defense.TeamId,
                    PlayerId = stealer.PlayerId,
                    SecondaryPlayerId = ballHandler.PlayerId,
                    LinkedEventIndex = _events.Count - 1,
                    Description = "Steal",
                    Importance = BasketballEventImportance.Notable,
                    RecommendedAnimationDuration = 0.7f
                });
            }
            return defense.TeamId;
        }

        BasketballPlayerProfile defender = SelectDefender(defense, defenseLineup, player => player.Blocking + player.Speed / 2);
        if (_random.Chance(0.035f * pressure))
        {
            EmitFoul(defender, defense.TeamId, offense.TeamId, BasketballFoulType.Personal, clock, duration, -1);
            if (_state.TeamFoulsInternal[defense.TeamId] >= 5)
                return SimulateFreeThrows(ballHandler, offense, defense, clock, 2, _events.Count - 1);
            return offense.TeamId;
        }

        BasketballShotType shotType = SelectShotType(ballHandler, offense.Tactics);
        Vector2 location = SelectShotLocation(ballHandler, offense.Tactics, shotType);
        float makeProbability = CalculateMakeProbability(ballHandler, defender, offense, defense, shotType, clock);
        float blockChance = CalculateBlockChance(ballHandler, defender, shotType);
        bool blocked = _random.Chance(blockChance);
        bool shootingFoul = !blocked && _random.Chance(CalculateShootingFoulChance(shotType, pressure));
        bool made = !blocked && _random.Chance(makeProbability);
        int shotPoints = made ? (shotType == BasketballShotType.ThreePointer ? 3 : 2) : 0;
        string assisterId = made ? SelectAssister(offense, offenseLineup, ballHandler) : string.Empty;
        BasketballMatchEvent shot = new BasketballMatchEvent(BasketballEventType.ShotAttempt, clock)
        {
            TeamId = offense.TeamId,
            OpponentTeamId = defense.TeamId,
            PossessionTeamId = made ? defense.TeamId : offense.TeamId,
            PlayerId = ballHandler.PlayerId,
            SecondaryPlayerId = assisterId,
            ShotType = shotType,
            CourtPosition = location,
            MakeProbability = makeProbability,
            IsMade = made,
            IsBlocked = blocked,
            IsShootingFoul = shootingFoul,
            Points = shotPoints,
            TimeSincePreviousEvent = duration,
            Description = $"{ShotName(shotType)} {(made ? "made" : "missed")}",
            Importance = made ? BasketballEventImportance.Notable : BasketballEventImportance.Routine,
            RecommendedAnimationDuration = shotType == BasketballShotType.Dunk ? 1.15f : 0.9f
        };
        if (blocked || shootingFoul)
            shot.LinkedEventIndex = _events.Count + 1;
        int shotIndex = Emit(shot);

        if (blocked)
        {
            Emit(new BasketballMatchEvent(BasketballEventType.Block, clock)
            {
                TeamId = defense.TeamId,
                OpponentTeamId = offense.TeamId,
                PossessionTeamId = offense.TeamId,
                PlayerId = defender.PlayerId,
                SecondaryPlayerId = ballHandler.PlayerId,
                LinkedEventIndex = shotIndex,
                Description = "Blocked shot",
                Importance = BasketballEventImportance.Important,
                RecommendedAnimationDuration = 0.8f
            });
        }

        if (shootingFoul)
        {
            EmitFoul(defender, defense.TeamId, offense.TeamId, BasketballFoulType.Shooting, clock, 0, shotIndex);
            int attempts = made ? 1 : shotType == BasketballShotType.ThreePointer ? 3 : 2;
            return SimulateFreeThrows(ballHandler, offense, defense, clock, attempts, shotIndex);
        }

        if (made)
            return defense.TeamId;
        return SimulateRebound(offense, defense, clock, shotIndex);
    }

    private int SimulateFreeThrows(BasketballPlayerProfile shooter, BasketballTeamProfile offense, BasketballTeamProfile defense, BasketballGameClock clock, int attempts, int linkedShotIndex)
    {
        bool finalMade = false;
        float probability = Mathf.Clamp(0.54f + (shooter.MidRange + shooter.ShortRange) / 500f, 0.55f, 0.94f);
        for (int attempt = 1; attempt <= attempts; attempt++)
        {
            finalMade = _random.Chance(probability);
            Emit(new BasketballMatchEvent(BasketballEventType.FreeThrow, clock)
            {
                TeamId = offense.TeamId,
                OpponentTeamId = defense.TeamId,
                PossessionTeamId = attempt < attempts ? offense.TeamId : finalMade ? defense.TeamId : offense.TeamId,
                PlayerId = shooter.PlayerId,
                ShotType = BasketballShotType.FreeThrow,
                CourtPosition = new Vector2(0f, 15f),
                MakeProbability = probability,
                IsMade = finalMade,
                FreeThrowNumber = attempt,
                FreeThrowTotal = attempts,
                Points = finalMade ? 1 : 0,
                LinkedEventIndex = linkedShotIndex,
                Description = $"Free throw {attempt} of {attempts} {(finalMade ? "made" : "missed")}",
                Importance = BasketballEventImportance.Notable,
                RecommendedAnimationDuration = 0.65f
            });
        }
        return finalMade ? defense.TeamId : SimulateRebound(offense, defense, clock, linkedShotIndex);
    }

    private int SimulateRebound(BasketballTeamProfile offense, BasketballTeamProfile defense, BasketballGameClock clock, int linkedEventIndex)
    {
        IReadOnlyList<string> offenseLineup = GetLineup(offense.TeamId);
        IReadOnlyList<string> defenseLineup = GetLineup(defense.TeamId);
        float offenseReboundRating = (float)offenseLineup.Average(id => FindPlayer(offense, id).Rebounding);
        float defenseReboundRating = (float)defenseLineup.Average(id => FindPlayer(defense, id).Rebounding);
        float tacticBoost = offense.Tactics?.OffensiveRebounding ?? 1f;
        float offensiveChance = Mathf.Clamp(0.19f + (offenseReboundRating - defenseReboundRating) / 350f + (tacticBoost - 1f) * 0.12f, 0.12f, 0.39f);
        bool offensive = _random.Chance(offensiveChance);
        BasketballTeamProfile reboundTeam = offensive ? offense : defense;
        IReadOnlyList<string> lineup = offensive ? offenseLineup : defenseLineup;
        BasketballPlayerProfile rebounder = SelectDefender(reboundTeam, lineup, player => player.Rebounding * PositionReboundWeight(player.Position));
        int nextTeamId = offensive ? offense.TeamId : defense.TeamId;
        Emit(new BasketballMatchEvent(BasketballEventType.Rebound, clock)
        {
            TeamId = reboundTeam.TeamId,
            OpponentTeamId = OtherTeamId(reboundTeam.TeamId),
            PossessionTeamId = nextTeamId,
            PlayerId = rebounder.PlayerId,
            ReboundType = offensive ? BasketballReboundType.Offensive : BasketballReboundType.Defensive,
            LinkedEventIndex = linkedEventIndex,
            Description = offensive ? "Offensive rebound" : "Defensive rebound",
            Importance = offensive ? BasketballEventImportance.Notable : BasketballEventImportance.Routine,
            RecommendedAnimationDuration = 0.45f
        });
        return nextTeamId;
    }

    private void ApplyRotation(BasketballGameClock clock, bool periodStart)
    {
        ApplyTeamRotation(_request.HomeTeam, _state.HomeLineup, clock, periodStart);
        ApplyTeamRotation(_request.AwayTeam, _state.AwayLineup, clock, periodStart);
    }

    private void ApplyTeamRotation(BasketballTeamProfile team, IReadOnlyList<string> current, BasketballGameClock clock, bool periodStart)
    {
        IReadOnlyList<string> desired = _rotation.SelectLineup(team, _state, current);
        foreach (BasketballSubstitution substitution in _rotation.BuildSubstitutions(team.TeamId, current, desired))
        {
            Emit(new BasketballMatchEvent(BasketballEventType.Substitution, clock)
            {
                TeamId = team.TeamId,
                OpponentTeamId = OtherTeamId(team.TeamId),
                PossessionTeamId = _state.PossessionTeamId,
                OutgoingPlayerId = substitution.OutgoingPlayerId,
                IncomingPlayerId = substitution.IncomingPlayerId,
                Description = string.IsNullOrEmpty(substitution.OutgoingPlayerId) ? "Starting lineup" : "Substitution",
                Importance = periodStart ? BasketballEventImportance.Notable : BasketballEventImportance.Routine,
                RecommendedAnimationDuration = 0.25f
            });
        }
    }

    private void MaybeCallTimeout(BasketballGameClock clock, int possessionTeamId)
    {
        int remaining = possessionTeamId == _request.HomeTeamId ? _homeTimeouts : _awayTimeouts;
        float chance = clock.Period >= 4 && clock.SecondsRemaining <= 180 ? 0.055f : 0.012f;
        bool scheduledTimeout = clock.Period == 2 && clock.SecondsRemaining <= 360 && remaining == 7;
        if (remaining <= 0 || (!scheduledTimeout && !_random.Chance(chance)))
            return;
        if (possessionTeamId == _request.HomeTeamId)
            _homeTimeouts--;
        else
            _awayTimeouts--;
        Emit(new BasketballMatchEvent(BasketballEventType.Timeout, clock)
        {
            TeamId = possessionTeamId,
            OpponentTeamId = OtherTeamId(possessionTeamId),
            PossessionTeamId = possessionTeamId,
            Description = "Timeout",
            Importance = clock.Period >= 4 ? BasketballEventImportance.Important : BasketballEventImportance.Notable,
            RecommendedAnimationDuration = 0.8f
        });
        ApplyRotation(clock, false);
    }

    private void EmitFoul(BasketballPlayerProfile fouler, int teamId, int opponentId, BasketballFoulType foulType, BasketballGameClock clock, int elapsed, int linkedEventIndex)
    {
        Emit(new BasketballMatchEvent(BasketballEventType.Foul, clock)
        {
            TeamId = teamId,
            OpponentTeamId = opponentId,
            PossessionTeamId = foulType == BasketballFoulType.Offensive ? opponentId : opponentId,
            PlayerId = fouler.PlayerId,
            FoulType = foulType,
            IsShootingFoul = foulType == BasketballFoulType.Shooting,
            LinkedEventIndex = linkedEventIndex,
            TimeSincePreviousEvent = elapsed,
            Description = foulType == BasketballFoulType.Shooting ? "Shooting foul" : foulType == BasketballFoulType.Offensive ? "Offensive foul" : "Personal foul",
            Importance = BasketballEventImportance.Notable,
            RecommendedAnimationDuration = 0.55f
        });
    }

    private int Emit(BasketballMatchEvent matchEvent)
    {
        int oldHome = _state.HomeScore;
        int oldAway = _state.AwayScore;
        matchEvent.Index = _events.Count;
        BasketballEventReducer.Apply(_state, matchEvent);
        matchEvent.HomeScore = _state.HomeScore;
        matchEvent.AwayScore = _state.AwayScore;
        ApplyPresentationMetadata(matchEvent, oldHome, oldAway);
        _events.Add(matchEvent);
        return matchEvent.Index;
    }

    private void ApplyPresentationMetadata(BasketballMatchEvent matchEvent, int oldHome, int oldAway)
    {
        int oldDifference = oldHome - oldAway;
        int newDifference = _state.HomeScore - _state.AwayScore;
        bool late = matchEvent.Clock.Period >= 4 && matchEvent.Clock.SecondsRemaining <= 180;
        if (oldDifference != 0 && newDifference != 0 && Math.Sign(oldDifference) != Math.Sign(newDifference))
            matchEvent.DramaticMoment |= BasketballDramaticMoment.LeadChange;
        if (newDifference == 0 && matchEvent.Points > 0)
            matchEvent.DramaticMoment |= BasketballDramaticMoment.TieGame;
        if (late && Math.Abs(newDifference) <= 6)
            matchEvent.DramaticMoment |= BasketballDramaticMoment.CloseFourthQuarter;
        if (late && matchEvent.EventType == BasketballEventType.ShotAttempt && Math.Abs(oldDifference) <= 3)
        {
            matchEvent.DramaticMoment |= BasketballDramaticMoment.ClutchAttempt;
            if (matchEvent.Clock.SecondsRemaining <= 10)
                matchEvent.DramaticMoment |= BasketballDramaticMoment.GameWinningAttempt;
        }
        if (matchEvent.Points > 0 && matchEvent.Clock.SecondsRemaining == 0)
            matchEvent.DramaticMoment |= BasketballDramaticMoment.BuzzerBeater;
        if (matchEvent.Clock.IsOvertime)
            matchEvent.DramaticMoment |= BasketballDramaticMoment.Overtime;
        if (matchEvent.DramaticMoment != BasketballDramaticMoment.None)
        {
            matchEvent.Importance = Math.Abs(newDifference) <= 3 ? BasketballEventImportance.Critical : BasketballEventImportance.Important;
            matchEvent.RecommendedAnimationDuration = Math.Max(matchEvent.RecommendedAnimationDuration, 1.1f);
        }
    }

    private BasketballPlayerProfile SelectShooter(BasketballTeamProfile team, IReadOnlyList<string> lineup)
    {
        List<BasketballPlayerProfile> players = lineup.Select(id => FindPlayer(team, id)).ToList();
        float concentration = team.Tactics?.UsageConcentration ?? 1f;
        return _random.Weighted(players, player =>
        {
            float scoring = player.ThreePoint * 0.22f + player.MidRange * 0.13f + player.ShortRange * 0.18f + player.Layups * 0.2f + player.Dunking * 0.12f + player.Playmaking * 0.15f;
            return Mathf.Pow(Mathf.Max(1f, scoring), concentration);
        });
    }

    private BasketballPlayerProfile SelectDefender(BasketballTeamProfile team, IReadOnlyList<string> lineup, Func<BasketballPlayerProfile, float> rating)
    {
        List<BasketballPlayerProfile> players = lineup.Select(id => FindPlayer(team, id)).ToList();
        return _random.Weighted(players, player => Math.Max(1f, rating(player)));
    }

    private BasketballShotType SelectShotType(BasketballPlayerProfile shooter, BasketballTacticProfile tactics)
    {
        float threeSkill = Mathf.Pow(Mathf.Clamp01(shooter.ThreePoint / 99f), 2.35f);
        float threeWeight = 0.62f * threeSkill * (tactics?.ThreePointEmphasis ?? 1f);
        float rimSkill = (shooter.Layups + shooter.Dunking + shooter.ShortRange) / 297f;
        float rimWeight = 0.48f * rimSkill * (tactics?.RimEmphasis ?? 1f);
        float midWeight = 0.16f + shooter.MidRange / 260f;
        float total = threeWeight + rimWeight + midWeight;
        float roll = _random.Range(0f, total);
        if (roll < threeWeight)
            return BasketballShotType.ThreePointer;
        roll -= threeWeight;
        if (roll < rimWeight)
        {
            float dunkChance = Mathf.Clamp01((shooter.Dunking - 35f) / 85f);
            return _random.Chance(dunkChance) ? BasketballShotType.Dunk : BasketballShotType.Layup;
        }
        return BasketballShotType.TwoPointJumpShot;
    }

    private Vector2 SelectShotLocation(BasketballPlayerProfile shooter, BasketballTacticProfile tactics, BasketballShotType shotType)
    {
        if (_request.ShotHeatmap != null)
            return _request.ShotHeatmap.Sample(shooter.Position, tactics?.OffensiveTactic ?? BasketballOffensiveTactic.Balanced, shotType, _random);
        if (shotType == BasketballShotType.Dunk || shotType == BasketballShotType.Layup)
            return new Vector2(_random.Range(-4f, 4f), _random.Range(1f, 8f));
        if (shotType == BasketballShotType.ThreePointer)
        {
            float angle = _random.Range(-1.18f, 1.18f);
            float radius = _random.Range(23.75f, 26.5f);
            return new Vector2(Mathf.Sin(angle) * radius, Mathf.Cos(angle) * radius);
        }
        return new Vector2(_random.Range(-16f, 16f), _random.Range(8f, 21f));
    }

    private float CalculateMakeProbability(BasketballPlayerProfile shooter, BasketballPlayerProfile defender, BasketballTeamProfile offense, BasketballTeamProfile defense, BasketballShotType shotType, BasketballGameClock clock)
    {
        float skill;
        float baseline;
        switch (shotType)
        {
            case BasketballShotType.ThreePointer: skill = shooter.ThreePoint; baseline = 0.335f; break;
            case BasketballShotType.Dunk: skill = shooter.Dunking; baseline = 0.78f; break;
            case BasketballShotType.Layup: skill = shooter.Layups; baseline = 0.57f; break;
            default: skill = shooter.MidRange; baseline = 0.41f; break;
        }
        float defenseRating = (defender.Speed + defender.Blocking) * 0.5f;
        float probability = baseline + (skill - 60f) * 0.0032f - (defenseRating - 60f) * 0.0014f;
        probability += ((offense.Tactics?.BallMovement ?? 1f) - 1f) * 0.055f;
        probability -= ((defense.Tactics?.DefensivePressure ?? 1f) - 1f) * 0.045f;
        if (_request.TeamToForceWin >= 0)
        {
            float force = clock.Period >= 4 ? 0.11f : 0.045f;
            probability += offense.TeamId == _request.TeamToForceWin ? force : -force;
        }
        return Mathf.Clamp(probability, shotType == BasketballShotType.Dunk ? 0.48f : 0.18f, shotType == BasketballShotType.Dunk ? 0.96f : 0.78f);
    }

    private float CalculateBlockChance(BasketballPlayerProfile shooter, BasketballPlayerProfile defender, BasketballShotType shotType)
    {
        float baseline = shotType == BasketballShotType.ThreePointer ? 0.018f : shotType == BasketballShotType.Dunk ? 0.07f : 0.055f;
        return Mathf.Clamp(baseline + (defender.Blocking - shooter.Speed) / 900f, 0.008f, 0.16f);
    }

    private static float CalculateShootingFoulChance(BasketballShotType shotType, float pressure)
    {
        float baseline = shotType == BasketballShotType.Dunk || shotType == BasketballShotType.Layup ? 0.13f : shotType == BasketballShotType.ThreePointer ? 0.035f : 0.065f;
        return Mathf.Clamp(baseline * pressure, 0.02f, 0.2f);
    }

    private string SelectAssister(BasketballTeamProfile offense, IReadOnlyList<string> lineup, BasketballPlayerProfile shooter)
    {
        float chance = Mathf.Clamp(0.48f * (offense.Tactics?.BallMovement ?? 1f) + shooter.Playmaking / 600f, 0.35f, 0.82f);
        if (!_random.Chance(chance))
            return string.Empty;
        List<BasketballPlayerProfile> candidates = lineup.Where(id => id != shooter.PlayerId).Select(id => FindPlayer(offense, id)).ToList();
        return _random.Weighted(candidates, player => Math.Max(1, player.Passing + player.Playmaking)).PlayerId;
    }

    private bool HasFoulOutOnCourt()
    {
        return _state.HomeLineup.Concat(_state.AwayLineup).Any(id => _state.PlayerStatsInternal[id].PersonalFouls >= 6);
    }

    private IReadOnlyList<string> GetLineup(int teamId) => teamId == _request.HomeTeamId ? _state.HomeLineup : _state.AwayLineup;
    private BasketballTeamProfile GetTeam(int teamId) => teamId == _request.HomeTeamId ? _request.HomeTeam : _request.AwayTeam;
    private int OtherTeamId(int teamId) => teamId == _request.HomeTeamId ? _request.AwayTeamId : _request.HomeTeamId;
    private static BasketballPlayerProfile FindPlayer(BasketballTeamProfile team, string playerId) => team.Players.First(player => player.PlayerId == playerId);
    private static float PositionReboundWeight(string position) => position == "Center" ? 1.35f : position == "Power Forward" ? 1.2f : position == "Point Guard" ? 0.75f : 1f;
    private static string ShotName(BasketballShotType shotType) => shotType == BasketballShotType.ThreePointer ? "Three-pointer" : shotType == BasketballShotType.Dunk ? "Dunk" : shotType == BasketballShotType.Layup ? "Layup" : "Two-point jumper";

    private static void ValidateRequest(BasketballSimulationRequest request)
    {
        if (request == null || request.HomeTeam == null || request.AwayTeam == null)
            throw new ArgumentNullException(nameof(request));
        if (request.HomeTeam.Players.Count < 5 || request.AwayTeam.Players.Count < 5)
            throw new InvalidOperationException("Detailed basketball simulation requires at least five players per team.");
        IEnumerable<string> ids = request.HomeTeam.Players.Concat(request.AwayTeam.Players).Select(player => player.PlayerId);
        if (ids.Any(string.IsNullOrEmpty) || ids.Distinct().Count() != ids.Count())
            throw new InvalidOperationException("Every simulated player requires a unique non-empty ID.");
    }
}
