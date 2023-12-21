using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField][HideInInspector] int _seriesWinner = -1;

    public static event Action<PlayoffMatchup, int> OnMatchupCompleted;

    public int GetSeriesWinner()
    {
        return _seriesWinner;
    }

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

    public List<Match> GetMatches()
    {
        return _matches;
    }

    public Match GetNextMatch()
    {
        foreach (Match match in _matches)
        {
            if (!match.GetMatchStatus())
            {
                return match;
            }
        }
        return null;
    }

    public Match GetLastPlayedMatch()
    {
        for (int i = _matches.Count - 1; i >= 0; i--)
        {
            if (_matches[i].GetMatchStatus())
            {
                Debug.Log("Found a match!");
                return _matches[i];
            }
        }

        Debug.Log("Did not find a match");
        return null;
    }

    public PlayoffMatchup(int homeID, int homeSeed, int awayID, int awaySeed)
    {
        _homeTeamID = homeID;
        _homeTeamSeed = homeSeed;
        _awayTeamID = awayID;
        _awayTeamSeed = awaySeed;

        GenerateMatches();
    }

    public List<Match> GenerateMatches()
    {
        int minimumAmountOfMatches = (ConfigManager.Instance.GetCurrentConfig().BestOfAmountInPlayoffs + 1) / 2;
        _matches = new List<Match>();

        for (int i = 0; i < minimumAmountOfMatches; i++)
        {
            if (i % 2 == 0)
            {
                Match match = new Match(GameManager.Instance.GetNextMatchID(), i, _homeTeamID, _awayTeamID);
                _matches.Add(match);

            }
            else
            {
                Match match = new Match(GameManager.Instance.GetNextMatchID(), i, _awayTeamID, _homeTeamID);
                _matches.Add(match);

            }
        }

        return _matches;
    }

    public void SimulateNextGameInSeries()
    {
        if (_matchupCompleted) return;

        foreach (var match in _matches)
        {
            if (!match.GetMatchStatus())
            {
                ConfigManager.Instance.GetCurrentConfig().MatchSimulator.SimulateMatch(match, GameManager.Instance.GetCurrentForceWinState() ? GameManager.Instance.GetTeamID() : -1);
                _homeTeamWins += match.GetWinStatForTeam(_homeTeamID).Item1;
                _awayTeamWins += match.GetWinStatForTeam(_awayTeamID).Item1;
                CheckForSeriesWinner();
                return;
            }
        }
    }

    public void SimulateSeries()
    {
        if (_matchupCompleted) return;

        foreach (var match in _matches)
        {
            if (!match.GetMatchStatus())
            {
                ConfigManager.Instance.GetCurrentConfig().MatchSimulator.SimulateMatch(match, GameManager.Instance.GetCurrentForceWinState() ? GameManager.Instance.GetTeamID() : -1);
                _homeTeamWins += match.GetWinStatForTeam(_homeTeamID).Item1;
                _awayTeamWins += match.GetWinStatForTeam(_awayTeamID).Item1;
            }
        }

        int winner = CheckForSeriesWinner();

        if (winner == -1)
        {
            SimulateSeries();
        }
    }

    public bool GetMatchupStatus()
    {
        return _matchupCompleted;
    }

    public int CheckForSeriesWinner()
    {
        int homeWins = _homeTeamWins;
        int awayWins = _awayTeamWins;

        if (homeWins >= (ConfigManager.Instance.GetCurrentConfig().BestOfAmountInPlayoffs + 1) / 2)
        {
            _matchupCompleted = true;
            _seriesWinner = _homeTeamID;
            OnMatchupCompleted?.Invoke(this, _homeTeamID);
            return _homeTeamID;
        }
        if (awayWins >= (ConfigManager.Instance.GetCurrentConfig().BestOfAmountInPlayoffs + 1) / 2)
        {
            _matchupCompleted = true;
            _seriesWinner = _awayTeamID;
            OnMatchupCompleted?.Invoke(this, _awayTeamID);
            return _awayTeamID;
        }

        bool enoughMatchesForHomeWin = _matches.Where(x => x.GetMatchStatus() == false).ToList().Count + homeWins >= (ConfigManager.Instance.GetCurrentConfig().BestOfAmountInPlayoffs + 1) / 2;
        bool enoughMatchesForAwayWin = _matches.Where(x => x.GetMatchStatus() == false).ToList().Count + awayWins >= (ConfigManager.Instance.GetCurrentConfig().BestOfAmountInPlayoffs + 1) / 2;

        if (!enoughMatchesForHomeWin || !enoughMatchesForHomeWin)
        {
            _matches.Add(new Match(GameManager.Instance.GetNextMatchID(), 0, _homeTeamID, _awayTeamID));
        }

        return -1;
    }
}
