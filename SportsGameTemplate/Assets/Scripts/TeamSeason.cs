using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TeamSeason
{
    [SerializeField] int _wins;
    [SerializeField] int _losses;

    public TeamSeason()
    {
        _wins = 0;
        _losses = 0;
    }

    public void AddResult(int points, int pointsAgainst)
    {
        if (points > pointsAgainst)
        {
            _wins++;
        } else
        {
            _losses++;
        }
    }

    public int GetWins()
    {
        return _wins;
    }

    public int GetLosses()
    {
        return _losses;
    }

    public (int, int) GetRecord()
    {
        return (_wins, _losses);
    }

    public float GetWinPercentage()
    {
        if (_wins == 0 && _losses == 0)
        {
            return 0;
        }
        return (float)_wins / (float)(_wins + _losses);
    }
}
