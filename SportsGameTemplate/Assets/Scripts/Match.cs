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

    [SerializeField] int _homeTeamPoints;
    [SerializeField] int _awayTeamPoints;

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

    public void SetResult(int homeTeamPoints, int awayTeamPoints)
    {
        _homeTeamPoints = homeTeamPoints;
        _awayTeamPoints = awayTeamPoints;
    }

    public bool IsHomeTeam(int teamID)
    {
        if (_homeTeamID == teamID)
            return true;

        return false;
    }

    public (int, int) GetWinStatForTeam(int teamID)
    {
        if (IsHomeTeam(teamID))
        {
            if (_homeTeamPoints >= _awayTeamPoints)
            {
                return (1, 0);
            }
            else
            {
                return (0, 1);
            }
        }
        else
        {
            if (_homeTeamPoints >= _awayTeamPoints)
            {
                return (0, 1);
            } 
            else
            {
                return (1, 0);
            }
        }
    }
}
