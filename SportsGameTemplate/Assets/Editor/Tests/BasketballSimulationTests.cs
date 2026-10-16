using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class BasketballSimulationTests
{
    private BasketballSimulationRequest _request;
    private BasketballSimulationResult _result;

    [SetUp]
    public void SetUp()
    {
        _request = CreateRequest(24681357);
        _result = new EventDrivenBasketballEngine().Simulate(_request);
    }

    [Test]
    public void EventsHaveSequentialIndicesAndChronologicalClocks()
    {
        for (int i = 0; i < _result.Events.Count; i++)
        {
            Assert.AreEqual(i, _result.Events[i].Index);
            if (i > 0)
                Assert.GreaterOrEqual(_result.Events[i].Clock.CompareTo(_result.Events[i - 1].Clock), 0, $"Event {i} moved backward in game time.");
        }
        Assert.AreEqual(BasketballEventType.GameEnded, _result.Events.Last().EventType);
    }

    [Test]
    public void SameSeedProducesIdenticalTimeline()
    {
        BasketballSimulationResult repeated = new EventDrivenBasketballEngine().Simulate(CreateRequest(24681357));
        Assert.AreEqual(_result.Events.Count, repeated.Events.Count);
        for (int i = 0; i < _result.Events.Count; i++)
        {
            BasketballMatchEvent expected = _result.Events[i];
            BasketballMatchEvent actual = repeated.Events[i];
            Assert.AreEqual(expected.EventType, actual.EventType);
            Assert.AreEqual(expected.Clock.Period, actual.Clock.Period);
            Assert.AreEqual(expected.Clock.SecondsRemaining, actual.Clock.SecondsRemaining);
            Assert.AreEqual(expected.PlayerId, actual.PlayerId);
            Assert.AreEqual(expected.SecondaryPlayerId, actual.SecondaryPlayerId);
            Assert.AreEqual(expected.HomeScore, actual.HomeScore);
            Assert.AreEqual(expected.AwayScore, actual.AwayScore);
            Assert.AreEqual(expected.ShotType, actual.ShotType);
            Assert.AreEqual(expected.IsMade, actual.IsMade);
            Assert.AreEqual(expected.CourtPosition, actual.CourtPosition);
        }
    }

    [Test]
    public void ScoreAndBoxScoresReconcileFromEvents()
    {
        BasketballValidationResult validation = BasketballSimulationValidator.Validate(_request, _result.Events);
        Assert.IsTrue(validation.IsValid, string.Join("\n", validation.Errors));
        Assert.AreEqual(_result.FinalState.HomeScore, _result.FinalState.HomeBoxScore.Points);
        Assert.AreEqual(_result.FinalState.AwayScore, _result.FinalState.AwayBoxScore.Points);
        Assert.AreNotEqual(_result.FinalState.HomeScore, _result.FinalState.AwayScore);
    }

    [Test]
    public void LineupsRemainLegalAndSubstitutionsAreGenerated()
    {
        BasketballMatchTimeline timeline = new BasketballMatchTimeline(_request, _result.Events, 17);
        List<BasketballMatchEvent> possessions = _result.Events.Where(matchEvent => matchEvent.EventType == BasketballEventType.PossessionStarted).ToList();
        Assert.IsNotEmpty(possessions);
        foreach (BasketballMatchEvent possession in possessions)
        {
            BasketballMatchState state = timeline.GetStateAt(possession.Index);
            Assert.AreEqual(5, state.HomeLineup.Distinct().Count());
            Assert.AreEqual(5, state.AwayLineup.Distinct().Count());
        }
        Assert.Greater(_result.Events.Count(matchEvent => matchEvent.EventType == BasketballEventType.Substitution), 10);
    }

    [Test]
    public void TeamMinutesEqualFivePlayersForEveryGameSecond()
    {
        int gameSeconds = _result.Events
            .Where(matchEvent => matchEvent.EventType == BasketballEventType.PeriodStarted)
            .Sum(matchEvent => matchEvent.Clock.Period <= 4 ? 720 : 300);
        int expectedPlayerSeconds = gameSeconds * 5;
        int homeSeconds = _result.FinalState.HomeBoxScore.Players.Values.Sum(box => box.SecondsPlayed);
        int awaySeconds = _result.FinalState.AwayBoxScore.Players.Values.Sum(box => box.SecondsPlayed);
        Assert.AreEqual(expectedPlayerSeconds, homeSeconds);
        Assert.AreEqual(expectedPlayerSeconds, awaySeconds);

        foreach (BasketballPlayerBoxScore boxScore in _result.FinalState.PlayerBoxScores.Values)
            Assert.LessOrEqual(System.Math.Abs(boxScore.Minutes - 24), 12, $"Rotation missed {boxScore.PlayerId}'s target excessively.");
    }

    [Test]
    public void CheckpointSeekMatchesDirectReduction()
    {
        BasketballMatchTimeline timeline = new BasketballMatchTimeline(_request, _result.Events, 23);
        int index = _result.Events.Count * 2 / 3;
        BasketballMatchState sought = timeline.GetStateAt(index);
        BasketballMatchState reduced = BasketballEventReducer.Reduce(_request, _result.Events.Take(index + 1));
        Assert.AreEqual(reduced.HomeScore, sought.HomeScore);
        Assert.AreEqual(reduced.AwayScore, sought.AwayScore);
        Assert.AreEqual(reduced.PossessionTeamId, sought.PossessionTeamId);
        CollectionAssert.AreEquivalent(reduced.HomeLineup, sought.HomeLineup);
        CollectionAssert.AreEquivalent(reduced.AwayLineup, sought.AwayLineup);
        foreach (string playerId in reduced.PlayerBoxScores.Keys)
        {
            Assert.AreEqual(reduced.PlayerBoxScores[playerId].Points, sought.PlayerBoxScores[playerId].Points);
            Assert.AreEqual(reduced.PlayerBoxScores[playerId].SecondsPlayed, sought.PlayerBoxScores[playerId].SecondsPlayed);
            Assert.AreEqual(reduced.PlayerBoxScores[playerId].PersonalFouls, sought.PlayerBoxScores[playerId].PersonalFouls);
        }
        Assert.AreEqual(_result.Events[index], sought.CurrentEvent);
        Assert.AreEqual(_result.Events[index - 1], sought.PreviousEvent);
        Assert.AreEqual(_result.Events[index + 1], sought.NextEvent);
    }

    [Test]
    public void TimelineContainsAllRequiredBasketballEventFamilies()
    {
        Assert.IsTrue(_result.Events.Any(matchEvent => matchEvent.EventType == BasketballEventType.ShotAttempt));
        Assert.IsTrue(_result.Events.Any(matchEvent => matchEvent.EventType == BasketballEventType.FreeThrow));
        Assert.IsTrue(_result.Events.Any(matchEvent => matchEvent.EventType == BasketballEventType.Rebound));
        Assert.IsTrue(_result.Events.Any(matchEvent => matchEvent.EventType == BasketballEventType.Turnover));
        Assert.IsTrue(_result.Events.Any(matchEvent => matchEvent.EventType == BasketballEventType.Foul));
        Assert.IsTrue(_result.Events.Any(matchEvent => matchEvent.EventType == BasketballEventType.Timeout));
        Assert.IsTrue(_result.Events.Any(matchEvent => matchEvent.EventType == BasketballEventType.Substitution));
        Assert.IsTrue(_result.Events.Where(matchEvent => matchEvent.EventType == BasketballEventType.ShotAttempt)
            .All(shot => shot.MakeProbability > 0f && shot.CourtPosition.y >= 0f));
    }

    private static BasketballSimulationRequest CreateRequest(int seed)
    {
        return new BasketballSimulationRequest
        {
            MatchId = 88,
            Seed = seed,
            HomeTeamId = 1,
            AwayTeamId = 2,
            HomeTeam = CreateTeam(1, "H"),
            AwayTeam = CreateTeam(2, "A")
        };
    }

    private static BasketballTeamProfile CreateTeam(int teamId, string prefix)
    {
        string[] positions = { "Point Guard", "Shooting Guard", "Small Forward", "Power Forward", "Center" };
        BasketballTeamProfile team = new BasketballTeamProfile
        {
            TeamId = teamId,
            Tactics = BasketballTacticProfile.Balanced
        };
        for (int i = 0; i < 10; i++)
        {
            int rating = 58 + (i * 7) % 30;
            team.Players.Add(new BasketballPlayerProfile
            {
                PlayerId = prefix + i,
                TeamId = teamId,
                Position = positions[i % positions.Length],
                TargetMinutes = 24,
                Overall = rating,
                Speed = rating,
                ShortRange = rating + 2,
                MidRange = rating,
                ThreePoint = i % 5 == 4 ? 27 : rating,
                Dunking = i % 5 >= 3 ? rating + 7 : rating - 8,
                Layups = rating + 2,
                Stealing = rating - 3,
                Passing = rating,
                Dribbling = rating,
                Playmaking = rating,
                Rebounding = i % 5 >= 3 ? rating + 8 : rating - 5,
                Blocking = i % 5 == 4 ? rating + 10 : rating - 6,
                Stamina = 72 + i % 5
            });
        }
        return team;
    }
}
