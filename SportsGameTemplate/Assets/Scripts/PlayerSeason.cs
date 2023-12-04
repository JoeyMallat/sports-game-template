using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class PlayerSeason
{
    [SerializeField] int _season;
    [SerializeField] int _teamID;
    [SerializeField] List<PlayerMatchStats> _matchStats;
    
    public PlayerSeason(int season, int teamID)
    {
        _season = season;
        _teamID = teamID;

        _matchStats = new List<PlayerMatchStats>();

        //DebugRandomSeason();
    }

    public List<PlayerMatchStats> GetMatchStats()
    {
        return _matchStats;
    }

    public void UpdateMatch(int matchID, List<(string, int)> stats)
    {
        var singleMatch = _matchStats.Where(x => x.GetMatchID() == matchID).ToList();

        if (singleMatch.Count == 0)
        {
            PlayerMatchStats newMatch = new PlayerMatchStats(matchID);
            _matchStats.Add(newMatch);
            newMatch.AddStat(stats);
        } else
        {
            singleMatch.FirstOrDefault().AddStat(stats);
        }
    }

    public void AddMatch(PlayerMatchStats match)
    {
        _matchStats.Add(match);
    }

    public float GetAveragePoints()
    {
        if (_matchStats.Count == 0) { return 0; }

        float total = 0;
        foreach (PlayerMatchStats match in _matchStats)
        {
            total += match.GetPoints();
        }

        return total / _matchStats.Count;
    }

    public float GetAverageOfStat(List<string> stats)
    {
        if (_matchStats.Count == 0) { return 0; }

        float total = 0;

        foreach (PlayerMatchStats match in _matchStats)
        {
            total += match.GetTotal(stats);
        }

        float percentage = total / _matchStats.Count;

        return percentage;
    }

    public float GetAverageOfStat(string stat)
    {
        if (_matchStats.Count == 0) { return 0; }

        float total = 0;

        foreach (PlayerMatchStats match in _matchStats)
        {
            total += match.GetTotal(stat);
        }

        float percentage = total / _matchStats.Count;

        return percentage;
    }
}
