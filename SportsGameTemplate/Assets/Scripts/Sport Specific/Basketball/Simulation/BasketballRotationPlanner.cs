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
        float staminaCapacity = 240f + Math.Max(25, player.Stamina) * 5f;
        float fatiguePenalty = Math.Max(0f, stint - staminaCapacity * 0.45f) / staminaCapacity;
        int fouls = GetFouls(state, player.PlayerId);
        float foulPenalty = fouls >= 5 ? 2.5f : fouls >= 4 ? 1.2f : fouls >= 3 ? 0.45f : 0f;
        float continuityBonus = currentLineup.Contains(player.PlayerId) ? 0.08f : 0f;
        float positionalFit = PositionalFit(player.Position, slot);

        return positionalFit * 2.2f + minutesNeed * 2.4f + player.Overall / 200f + continuityBonus - fatiguePenalty * 2f - foulPenalty;
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

    private static float PositionalFit(string playerPosition, string courtSlot)
    {
        if (string.Equals(playerPosition, courtSlot, StringComparison.OrdinalIgnoreCase))
            return 1f;

        switch (courtSlot)
        {
            case "Point Guard":
                return Is(playerPosition, "Shooting Guard") ? 0.72f : 0.15f;
            case "Shooting Guard":
                return Is(playerPosition, "Point Guard", "Small Forward") ? 0.72f : 0.2f;
            case "Small Forward":
                return Is(playerPosition, "Shooting Guard", "Power Forward") ? 0.72f : 0.25f;
            case "Power Forward":
                return Is(playerPosition, "Small Forward", "Center") ? 0.72f : 0.2f;
            case "Center":
                return Is(playerPosition, "Power Forward") ? 0.72f : 0.12f;
            default:
                return 0.1f;
        }
    }

    private static bool Is(string position, params string[] choices)
    {
        return choices.Any(choice => string.Equals(position, choice, StringComparison.OrdinalIgnoreCase));
    }
}
