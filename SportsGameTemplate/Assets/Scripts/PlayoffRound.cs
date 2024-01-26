using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayoffRound : MonoBehaviour
{
    [SerializeField] int _playoffRound;
    [SerializeField] List<PlayoffMatchup> _playoffMatchups;
    public static event Action<PlayoffRound> OnPlayoffRoundUpdated;
    public static event Action<List<Team>> OnPlayoffRoundFinished;

    private void Awake()
    {
        PlayoffMatchup.OnMatchupCompleted += CheckForRoundCompletion;
    }

    private void CheckForRoundCompletion(PlayoffMatchup matchup, int winningTeamID)
    {
        if (_playoffMatchups.Contains(matchup) && matchup.GetMatchupStatus())
        {
            List<Team> winners = new List<Team>();

            foreach (PlayoffMatchup match in _playoffMatchups)
            {
                if (match.GetMatchupStatus() == false) return;

                winners.Add(LeagueSystem.Instance.GetTeam(match.GetSeriesWinner()));
            }

            OnPlayoffRoundFinished?.Invoke(winners);
        }
    }

    public void AddMatchup(int index, int homeID, int homeSeed, int awayID, int awaySeed)
    {
        _playoffMatchups[index] = new PlayoffMatchup(homeID, homeSeed, awayID, awaySeed);
        OnPlayoffRoundUpdated?.Invoke(this);
    }

    public void SimMatches()
    {
        foreach (var playoff in _playoffMatchups)
        {
            playoff.SimulateNextGameInSeries();
        }

        OnPlayoffRoundUpdated?.Invoke(this);
    }

    public void SimSeries()
    {
        foreach (var playoff in _playoffMatchups)
        {
            playoff.SimulateSeries();
        }

        OnPlayoffRoundUpdated?.Invoke(this);
    }

    public List<PlayoffMatchup> GetMatchups()
    {
        return _playoffMatchups;
    }

    public int GetPlayoffRoundNumber()
    {
        return _playoffRound;
    }

    public void SetMatchup(int matchupIndex, PlayoffMatchup playoffMatchup)
    {
        _playoffMatchups[matchupIndex] = playoffMatchup;
        OnPlayoffRoundUpdated?.Invoke(this);
    }
}
