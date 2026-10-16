using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public interface IBasketballMatchTimeline
{
    int Seed { get; }
    int EventCount { get; }
    IReadOnlyList<BasketballMatchEvent> Events { get; }
    BasketballMatchEvent GetEvent(int eventIndex);
    BasketballMatchState GetStateAt(int eventIndex);
    float GetPlaybackDuration(int eventIndex, float globalSpeedMultiplier = 1f);
}

public sealed class BasketballMatchTimeline : IBasketballMatchTimeline
{
    private readonly BasketballSimulationRequest _request;
    private readonly ReadOnlyCollection<BasketballMatchEvent> _events;
    private readonly SortedDictionary<int, BasketballMatchState> _checkpoints = new SortedDictionary<int, BasketballMatchState>();

    public BasketballMatchTimeline(BasketballSimulationRequest request, IEnumerable<BasketballMatchEvent> events, int checkpointInterval = 50)
    {
        _request = request ?? throw new ArgumentNullException(nameof(request));
        if (events == null)
            throw new ArgumentNullException(nameof(events));
        if (checkpointInterval < 1)
            throw new ArgumentOutOfRangeException(nameof(checkpointInterval));

        _events = new ReadOnlyCollection<BasketballMatchEvent>(events.ToList());
        Seed = request.Seed;
        CheckpointInterval = checkpointInterval;
        BuildCheckpoints();
    }

    public int Seed { get; }
    public int EventCount => _events.Count;
    public int CheckpointInterval { get; }
    public IReadOnlyList<BasketballMatchEvent> Events => _events;

    public BasketballMatchEvent GetEvent(int eventIndex)
    {
        ValidateIndex(eventIndex);
        return _events[eventIndex];
    }

    public BasketballMatchState GetStateAt(int eventIndex)
    {
        ValidateIndex(eventIndex);
        int checkpointIndex = FindCheckpoint(eventIndex);
        BasketballMatchState state = _checkpoints[checkpointIndex].Clone();
        for (int i = checkpointIndex + 1; i <= eventIndex; i++)
            BasketballEventReducer.Apply(state, _events[i]);

        state.CurrentEvent = _events[eventIndex];
        state.PreviousEvent = eventIndex > 0 ? _events[eventIndex - 1] : null;
        state.NextEvent = eventIndex + 1 < _events.Count ? _events[eventIndex + 1] : null;
        return state;
    }

    public float GetPlaybackDuration(int eventIndex, float globalSpeedMultiplier = 1f)
    {
        ValidateIndex(eventIndex);
        if (globalSpeedMultiplier <= 0f)
            throw new ArgumentOutOfRangeException(nameof(globalSpeedMultiplier));
        BasketballMatchEvent matchEvent = _events[eventIndex];
        return (matchEvent.TimeSincePreviousEvent + matchEvent.RecommendedAnimationDuration) / globalSpeedMultiplier;
    }

    private void BuildCheckpoints()
    {
        BasketballMatchState state = BasketballEventReducer.CreateInitialState(_request);
        _checkpoints[-1] = state.Clone();
        for (int i = 0; i < _events.Count; i++)
        {
            BasketballEventReducer.Apply(state, _events[i]);
            if ((i + 1) % CheckpointInterval == 0 || i == _events.Count - 1)
                _checkpoints[i] = state.Clone();
        }
    }

    private int FindCheckpoint(int eventIndex)
    {
        int checkpoint = -1;
        foreach (int index in _checkpoints.Keys)
        {
            if (index > eventIndex)
                break;
            checkpoint = index;
        }
        return checkpoint;
    }

    private void ValidateIndex(int eventIndex)
    {
        if (eventIndex < 0 || eventIndex >= _events.Count)
            throw new ArgumentOutOfRangeException(nameof(eventIndex), eventIndex, $"Timeline contains {_events.Count} events.");
    }
}
