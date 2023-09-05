using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class Match
{
    [SerializeField] int _matchID;
    [SerializeField] int _week;
    [SerializeField] int _homeTeamID;
    [SerializeField] int _awayTeamID;

    public Match (int id, int week, int homeID, int awayID)
    {
        _matchID = id;
        _week = week;
        _homeTeamID = homeID;
        _awayTeamID = awayID;
    }

    public int GetWeek()
    {
        return _week;
    }

    public int GetHomeTeamID()
    {
        return _homeTeamID;
    }

    public int GetAwayTeamID()
    {
        return _awayTeamID;
    }
}
