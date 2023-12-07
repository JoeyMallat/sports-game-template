using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class PrematchView : MonoBehaviour, ISettable
{
    [SerializeField] GameObject _lineupItemsRoot;

    public void SetDetails<T>(T item) where T : class
    {
        Debug.Log("Loading prematch view");
        Team team = item as Team;

        List<LineupItem> _lineupItems = _lineupItemsRoot.GetComponentsInChildren<LineupItem>(true).ToList();

        List<Player> players = team.GetPlayersFromIDs(team.GetLineup());

        for (int i = 0; i < _lineupItems.Count; i++)
        {
            int index = i;
            _lineupItems[i].SetPlayerDetails(players[index]);
        }
    }

    public void GoToPrematchView()
    {
        Navigation.Instance.GoToScreen(true, CanvasKey.Prematch, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()));
    }
}
