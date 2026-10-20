using System;
using System.Collections.Generic;
using System.Linq;

public readonly struct BasketballSubstitution
{
    public BasketballSubstitution(int teamId, string outgoingPlayerId, string incomingPlayerId)
    {
        TeamId = teamId;
        OutgoingPlayerId = outgoingPlayerId;
        IncomingPlayerId = incomingPlayerId;
    }

    public int TeamId { get; }
    public string OutgoingPlayerId { get; }
    public string IncomingPlayerId { get; }
}

public sealed class BasketballRotationPlanner
{
    private static readonly string[] CourtSlots = { "Point Guard", "Shooting Guard", "Small Forward", "Power Forward", "Center" };
    private readonly Dictionary<string, int> _stintSeconds = new Dictionary<string, int>();
    private readonly BasketballSimulationSettings _settings;

    public BasketballRotationPlanner(BasketballSimulationSettings settings)
    {
        _settings = settings ?? new BasketballSimulationSettings();
    }

    public void Advance(IEnumerable<string> homeLineup, IEnumerable<string> awayLineup, int elapsedSeconds)
    {
        foreach (string playerId in homeLineup.Concat(awayLineup))
        {
            if (!_stintSeconds.ContainsKey(playerId))
                _stintSeconds[playerId] = 0;
            _stintSeconds[playerId] += Math.Max(0, elapsedSeconds);
        }
    }

    public IReadOnlyList<string> SelectLineup(BasketballTeamProfile team, BasketballMatchState state, IReadOnlyList<string> currentLineup)
    {
        if (team.Players.Count < 5)
            throw new InvalidOperationException($"Team {team.TeamId} requires at least five available players.");

        List<BasketballPlayerProfile> eligible = team.Players
            .Where(player => GetFouls(state, player.PlayerId) < 6)
            .ToList();
        if (eligible.Count < 5)
        {
            eligible = team.Players
                .OrderBy(player => GetFouls(state, player.PlayerId))
                .ThenByDescending(player => player.Overall)
                .ToList();
        }

        float minuteScale = CalculateMinuteScale(team.Players);
        List<BasketballPlayerProfile> selected = new List<BasketballPlayerProfile>(5);
        foreach (string slot in CourtSlots)
        {
            List<BasketballPlayerProfile> candidates = eligible.Where(player => !selected.Contains(player)).ToList();
            BasketballPlayerProfile choice = candidates
                .OrderByDescending(player => SelectionScore(player, slot, state, currentLineup, minuteScale))
                .First();
            selected.Add(choice);
        }

        return selected.Select(player => player.PlayerId).ToList().AsReadOnly();
    }

    public IReadOnlyList<BasketballSubstitution> BuildSubstitutions(int teamId, IReadOnlyList<string> currentLineup, IReadOnlyList<string> desiredLineup)
    {
        List<string> outgoing = currentLineup.Where(playerId => !desiredLineup.Contains(playerId)).ToList();
        List<string> incoming = desiredLineup.Where(playerId => !currentLineup.Contains(playerId)).ToList();
        List<BasketballSubstitution> substitutions = new List<BasketballSubstitution>();
        int count = Math.Max(outgoing.Count, incoming.Count);
        for (int i = 0; i < count; i++)
        {
            string playerOut = i < outgoing.Count ? outgoing[i] : string.Empty;
            string playerIn = i < incoming.Count ? incoming[i] : string.Empty;
            substitutions.Add(new BasketballSubstitution(teamId, playerOut, playerIn));
            if (!string.IsNullOrEmpty(playerOut))
                _stintSeconds[playerOut] = 0;
            if (!string.IsNullOrEmpty(playerIn))
                _stintSeconds[playerIn] = 0;
        }
        return substitutions.AsReadOnly();
    }

    private float SelectionScore(BasketballPlayerProfile player, string slot, BasketballMatchState state, IReadOnlyList<string> currentLineup, float minuteScale)
    {
        int targetSeconds = Math.Max(1, (int)(Math.Max(0, player.TargetMinutes) * minuteScale * 60f));
        int playedSeconds = state.PlayerStatsInternal.TryGetValue(player.PlayerId, out BasketballPlayerBoxScore boxScore)
            ? boxScore.SecondsPlayed
            : 0;
        float minutesNeed = (targetSeconds - playedSeconds) / (float)targetSeconds;
        float stint = _stintSeconds.TryGetValue(player.PlayerId, out int seconds) ? seconds : 0;
        float staminaCapacity = _settings.StaminaCapacityBaseSeconds +
            Math.Max(_settings.MinimumStaminaRating, player.Stamina) * _settings.StaminaCapacitySecondsPerRating;
        staminaCapacity = Math.Max(1f, staminaCapacity);
        float fatiguePenalty = Math.Max(0f, stint - staminaCapacity * _settings.FatigueThreshold) / staminaCapacity;
        int fouls = GetFouls(state, player.PlayerId);
        float foulPenalty = fouls >= 5
            ? _settings.FiveFoulPenalty
            : fouls >= 4
                ? _settings.FourFoulPenalty
                : fouls >= 3
                    ? _settings.ThreeFoulPenalty
                    : 0f;
        float continuityBonus = currentLineup.Contains(player.PlayerId) ? _settings.ContinuityBonus : 0f;
        float positionalFit = PositionalFit(player.Position, slot);

        return positionalFit * _settings.PositionalFitSelectionWeight +
            minutesNeed * _settings.MinutesNeedSelectionWeight +
            player.Overall / Math.Max(1f, _settings.OverallSelectionDivisor) +
            continuityBonus -
            fatiguePenalty * _settings.FatiguePenaltyWeight -
            foulPenalty;
    }

    private static float CalculateMinuteScale(IReadOnlyCollection<BasketballPlayerProfile> players)
    {
        int assigned = players.Sum(player => Math.Max(0, player.TargetMinutes));
        return assigned > 0 ? 240f / assigned : 1f;
    }

    private static int GetFouls(BasketballMatchState state, string playerId)
    {
        return state.PlayerStatsInternal.TryGetValue(playerId, out BasketballPlayerBoxScore boxScore)
            ? boxScore.PersonalFouls
            : 0;
    }

    private float PositionalFit(string playerPosition, string courtSlot)
    {
        if (string.Equals(playerPosition, courtSlot, StringComparison.OrdinalIgnoreCase))
            return _settings.ExactPositionFit;

        switch (courtSlot)
        {
            case "Point Guard":
                return Is(playerPosition, "Shooting Guard") ? _settings.AdjacentPositionFit : _settings.PointGuardFallbackFit;
            case "Shooting Guard":
                return Is(playerPosition, "Point Guard", "Small Forward") ? _settings.AdjacentPositionFit : _settings.ShootingGuardFallbackFit;
            case "Small Forward":
                return Is(playerPosition, "Shooting Guard", "Power Forward") ? _settings.AdjacentPositionFit : _settings.SmallForwardFallbackFit;
            case "Power Forward":
                return Is(playerPosition, "Small Forward", "Center") ? _settings.AdjacentPositionFit : _settings.PowerForwardFallbackFit;
            case "Center":
                return Is(playerPosition, "Power Forward") ? _settings.AdjacentPositionFit : _settings.CenterFallbackFit;
            default:
                return _settings.UnknownPositionFit;
        }
    }

    private static bool Is(string position, params string[] choices)
    {
        return choices.Any(choice => string.Equals(position, choice, StringComparison.OrdinalIgnoreCase));
    }
}
