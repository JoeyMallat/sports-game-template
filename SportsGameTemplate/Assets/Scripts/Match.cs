using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

[System.Serializable]
public class Match
{
    [SerializeField] int _matchID;
    [SerializeField] int _week;
    [SerializeField] int _homeTeamID;
    [SerializeField] int _awayTeamID;

    [SerializeField] int _homeTeamPoints;
    [SerializeField] int _awayTeamPoints;

    public static event Action<Match> OnMatchPlayed;

    public Match (int id, int week, int homeID, int awayID)
    {
        _matchID = id;
        _week = week;
        _homeTeamID = homeID;
        _awayTeamID = awayID;

        OnMatchPlayed?.Invoke(this);
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
        LeagueSystem.Instance.GetTeam(_homeTeamID).GetCurrentSeasonStats().AddResult(homeTeamPoints, awayTeamPoints);

        _awayTeamPoints = awayTeamPoints;
        LeagueSystem.Instance.GetTeam(_awayTeamID).GetCurrentSeasonStats().AddResult(awayTeamPoints, homeTeamPoints);
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
