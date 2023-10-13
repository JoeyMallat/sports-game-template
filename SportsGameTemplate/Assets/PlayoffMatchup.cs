using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayoffMatchup
{
    [SerializeField] int _homeTeamID;
    [SerializeField] int _homeTeamSeed;
    [SerializeField] int _homeTeamWins;
    [SerializeField] int _awayTeamID;
    [SerializeField] int _awayTeamSeed;
    [SerializeField] int _awayTeamWins;
    [SerializeField] List<Match> _matches;
    [SerializeField] bool _matchupCompleted = false;

    public static event Action<int> OnMatchupCompleted;

    public int GetHomeTeamID()
    {
        return _homeTeamID;
    }

    public int GetHomeTeamSeed()
    {
        return _homeTeamSeed;
    }

    public int GetAwayTeamSeed()
    {
        return _awayTeamSeed;
    }

    public int GetAwayTeamID()
    {
        return _awayTeamID;
    }

    public (int, int) GetSeriesScore()
    {
        return (_homeTeamWins, _awayTeamWins);
    }

    public PlayoffMatchup(int homeID, int homeSeed, int awayID, int awaySeed)
    {
        _homeTeamID = homeID;
        _homeTeamSeed = homeSeed;
        _awayTeamID = awayID;
        _awayTeamSeed = awaySeed;

        GenerateMatches(0, 0);
    }

    public List<Match> GenerateMatches(int lastID, int week)
    {
        int minimumAmountOfMatches = (ConfigManager.Instance.GetCurrentConfig().BestOfAmountInPlayoffs + 1) / 2;
        _matches = new List<Match>();

        for (int i = 0; i < minimumAmountOfMatches; i++)
        {
            if (i % 2 == 0)
            {
                Match match = new Match(lastID + i, week + i, _homeTeamID, _awayTeamID);
                _matches.Add(match);

            }
            else
            {
                Match match = new Match(lastID + i, week + i, _awayTeamID, _homeTeamID);
                _matches.Add(match);

            }
        }

        return _matches;
    }

    public void SimulateSeries()
    {
        foreach (var match in _matches)
        {
            if (!match.GetMatchStatus())
            {
                ConfigManager.Instance.GetCurrentConfig().MatchSimulator.SimulateMatch(match);
                _homeTeamWins += match.GetWinStatForTeam(_homeTeamID).Item1;
                _awayTeamWins += match.GetWinStatForTeam(_awayTeamID).Item1;
            }
        }

        if (CheckForSeriesWinner() == -1)
        {
            Debug.Log($"No winner after {_matches.Count} games");
            SimulateSeries();
        } else
        {
            Debug.Log($"{LeagueSystem.Instance.GetTeam(CheckForSeriesWinner()).GetTeamName()} has won the series after {_matches.Count}!");
        }
    }

    private int CheckForSeriesWinner()
    {
        int homeWins = _homeTeamWins;
        int awayWins = _awayTeamWins;

        if (homeWins >= (ConfigManager.Instance.GetCurrentConfig().BestOfAmountInPlayoffs + 1) / 2)
        {
            OnMatchupCompleted?.Invoke(_homeTeamID);
            return _homeTeamID;
        }
        if (awayWins >= (ConfigManager.Instance.GetCurrentConfig().BestOfAmountInPlayoffs + 1) / 2)
        {
            OnMatchupCompleted?.Invoke(_awayTeamID);
            return _awayTeamID;
        }

        _matches.Add(new Match(0, 0, _homeTeamID, _awayTeamID));

        return -1;
    }
}
