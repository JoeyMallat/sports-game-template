using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayoffViewer : MonoBehaviour, ISettable
{
    [SerializeField] GameObject _playoffRoot;
    [SerializeField] GameObject _firstRoundRoot;
    [SerializeField] GameObject _secondRoundRoot;
    [SerializeField] GameObject _thirdRoundRoot;
    [SerializeField] GameObject _fourthRoundRoot;


    private void Awake()
    {
        PlayoffRound.OnPlayoffRoundUpdated += SetDetails;
    }

    public void SetDetails<T>(T item) where T : class
    {
        gameObject.SetActive(false);

        PlayoffRound playoffRound = item as PlayoffRound;

        int round = playoffRound.GetPlayoffRoundNumber();
        List<PlayoffMatchup> matchups = playoffRound.GetMatchups();

        List<MatchupItem> matchupItems = new List<MatchupItem>();

        matchupItems = _playoffRoot.GetComponentsInChildren<MatchupItem>(true).ToList();
        SetMatchups(matchupItems, matchups, true);

        switch (round)
        {
            case 0:
                matchupItems = _firstRoundRoot.GetComponentsInChildren<MatchupItem>().ToList();
                break;
            case 1:
                matchupItems = _secondRoundRoot.GetComponentsInChildren<MatchupItem>().ToList();
                break;
            case 2:
                matchupItems = _thirdRoundRoot.GetComponentsInChildren<MatchupItem>().ToList();
                break;
            case 3:
                matchupItems = _fourthRoundRoot.GetComponentsInChildren<MatchupItem>().ToList();
                break;
            default:
                break;
        }

        SetMatchups(matchupItems, matchups, false);

        gameObject.SetActive(true);
    }

    private void SetMatchups(List<MatchupItem> matchupItems, List<PlayoffMatchup> matchups, bool empty)
    {
        for (int i = 0; i < matchups.Count; i++)
        {
            int index = i;

            if (empty)
            {
                matchupItems[i].EmptyMatchup();
            }
            else
            {
                matchupItems[i].SetMatchup(matchups[index]);
            }
        }
    }
}
