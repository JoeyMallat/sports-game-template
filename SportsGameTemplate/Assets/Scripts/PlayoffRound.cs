using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

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
}
