using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayoffViewer : MonoBehaviour, ISettable
{
    [SerializeField] GameObject _firstRoundRoot;

    private void Awake()
    {
        PlayoffRound.OnPlayoffRoundUpdated += SetDetails;
    }

    public void SetDetails<T>(T item) where T : class
    {
        PlayoffRound playoffRound = item as PlayoffRound;
        List<PlayoffMatchup> matchups = playoffRound.GetMatchups();

        List<MatchupItem> matchupItems = _firstRoundRoot.GetComponentsInChildren<MatchupItem>().ToList();

        for (int i = 0; i < matchups.Count; i++)
        {
            int index = i;
            matchupItems[i].SetMatchup(matchups[index]);
        }
    }
}
