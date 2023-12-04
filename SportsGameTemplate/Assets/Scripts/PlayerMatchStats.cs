using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class PlayerMatchStats
{
    [SerializeField] int _matchID;
    [SerializeField] Dictionary<string, int> _stats;

    public PlayerMatchStats(int matchID)
    {
        _matchID = matchID;
        _stats = new Dictionary<string, int>();

        _stats["minutes"] = 0;
        _stats["assists"] = 0;
        _stats["steals"] = 0;
        _stats["rebounds"] = 0;
        _stats["blocks"] = 0;
        _stats["freeThrowsAttempted"] = 0;
        _stats["freeThrowsMade"] = 0;
        _stats["twoPointersAttempted"] = 0;
        _stats["twoPointersMade"] = 0;
        _stats["twoPointersPoints"] = 0;
        _stats["threePointersAttempted"] = 0;
        _stats["threePointersMade"] = 0;
        _stats["threePointersPoints"] = 0;
    }

    public PlayerMatchStats(int matchID, int minutes, int assists, int steals, int rebounds, int blocks, int ftAttempts, int ftMade, int twoAttempts, int twoMade, int threeAttempts, int threeMade)
    {
        _matchID = matchID;
        _stats = new Dictionary<string, int>();

        _stats["minutes"] = minutes;
        _stats["assists"] = assists;
        _stats["steals"] = steals;
        _stats["rebounds"] = rebounds;
        _stats["blocks"] = blocks;
        _stats["freeThrowsAttempted"] = ftAttempts;
        _stats["freeThrowsMade"] = ftMade;
        _stats["twoPointersAttempted"] = twoAttempts;
        _stats["twoPointersMade"] = twoMade;
        _stats["twoPointersPoints"] = twoMade * 2;
        _stats["threePointersAttempted"] = threeAttempts;
        _stats["threePointersMade"] = threeMade;
        _stats["threePointersPoints"] = threeMade * 3;
    }

    public int GetPoints()
    {
        return _stats["freeThrowsMade"] + _stats["twoPointersMade"] * 2 + _stats["threePointersMade"] * 3;
    }

    public int GetTotal(string stat)
    {
        return _stats[stat];
    }

    public int GetTotal(List<string> stats)
    {
        int total = 0;

        foreach (string key in stats)
        {
            total += _stats[key];
        }

        return total;
    }

    public void AddStat(List<(string, int)> stats)
    {
        foreach (var stat in stats)
        {
            _stats[stat.Item1] += stat.Item2;
        }
    }

    public int GetMatchID()
    {
        return _matchID;
    }
}
