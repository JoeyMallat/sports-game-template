using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class PlayoffRound : MonoBehaviour
{
    [SerializeField] List<PlayoffMatchup> _playoffMatchups;
    public static event Action<PlayoffRound> OnPlayoffRoundUpdated;

    public void AddMatchup(int index, int homeID, int homeSeed, int awayID, int awaySeed)
    {
        _playoffMatchups[index] = new PlayoffMatchup(homeID, homeSeed, awayID, awaySeed);
        OnPlayoffRoundUpdated?.Invoke(this);

        SimMatches();
    }

    private void SimMatches()
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
}
