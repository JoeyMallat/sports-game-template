using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Match
{
    [SerializeField] int _matchID;
    [SerializeField] int _week;
    [SerializeField] int _homeTeamID;
    [SerializeField] int _awayTeamID;

    [SerializeField] int _homeTeamPoints;
    [SerializeField] int _awayTeamPoints;

    [SerializeField] bool _matchPlayed;

    [SerializeField] List<PossessionResult> _possessionResults;

    public static event Action<Match> OnMatchPlayed;

    public Match(int id, int week, int homeID, int awayID)
    {
        _matchID = id;
        _week = week;
        _homeTeamID = homeID;
        _awayTeamID = awayID;
        _possessionResults = new List<PossessionResult>();
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

    public int GetMatchID()
    {
        return _matchID;
    }

    public bool GetMatchStatus()
    {
        return _matchPlayed;
    }

    public bool IsMyTeamMatch(int teamID)
    {
        if (GetHomeTeamID() == teamID || GetAwayTeamID() == teamID)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public (int, int) GetScore()
    {
        return (_homeTeamPoints, _awayTeamPoints);
    }

    public void AddPossessionResult(PossessionResult possessionResult, Team team)
    {
        _possessionResults.Add(possessionResult);

        switch (possessionResult.GetPossessionResult())
        {
            case ResultAction.TwoPointerMade:
                if (IsHomeTeam(team.GetTeamID()))
                {
                    _homeTeamPoints += 2;
                }
                else
                {
                    _awayTeamPoints += 2;
                }
                break;
            case ResultAction.ThreePointerMade:
                if (IsHomeTeam(team.GetTeamID()))
                {
                    _homeTeamPoints += 3;
                }
                else
                {
                    _awayTeamPoints += 3;
                }
                break;
            case ResultAction.FreeThrowMade:
                if (IsHomeTeam(team.GetTeamID()))
                {
                    _homeTeamPoints += 1;
                }
                else
                {
                    _awayTeamPoints += 1;
                }
                break;
            default:
                break;
        }
    }

    public void EndMatch()
    {
        _matchPlayed = true;
        SetResult(_homeTeamPoints, _awayTeamPoints);

        OnMatchPlayed?.Invoke(this);

        _possessionResults = new();

        /*
        int totalTwoPointers = 0;
        int madeTwoPointers = 0;
        foreach (var result in _possessionResults)
        {
            if (result.GetPossessionResult() == ResultAction.TwoPointerMade)
            {
                totalTwoPointers++;
                madeTwoPointers++;
            } else if (result.GetPossessionResult() == ResultAction.TwoPointerMissed)
            {
                totalTwoPointers++;
            }
        }

        Debug.Log($"Two pointers shooting percentage: {(float)madeTwoPointers / (float)totalTwoPointers}");


        int totalThreePointers = 0;
        int madeThreePointers = 0;
        foreach (var result in _possessionResults)
        {
            if (result.GetPossessionResult() == ResultAction.ThreePointerMade)
            {
                totalThreePointers++;
                madeThreePointers++;
            }
            else if (result.GetPossessionResult() == ResultAction.ThreePointerMissed)
            {
                totalThreePointers++;
            }
        }

        Debug.Log($"Three pointers shooting percentage: {(float)madeThreePointers / (float)totalThreePointers}"); */
    }

    public void SetResult(int homeTeamPoints, int awayTeamPoints)
    {
        _homeTeamPoints = homeTeamPoints;
        _awayTeamPoints = awayTeamPoints;

        if (GameManager.Instance.GetSeasonStage() == SeasonStage.RegularSeason)
        {
            LeagueSystem.Instance.GetTeam(_homeTeamID).GetCurrentSeasonStats().AddResult(homeTeamPoints, awayTeamPoints);
            LeagueSystem.Instance.GetTeam(_awayTeamID).GetCurrentSeasonStats().AddResult(awayTeamPoints, homeTeamPoints);
        }
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
