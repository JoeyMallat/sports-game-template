using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

public class PrematchView : MonoBehaviour, ISettable
{
    [SerializeField] GameObject _lineupItemsRoot;
    [SerializeField] GameObject _benchItemsRoot;

    private void Awake()
    {
        Team.OnLineupChanged += SetDetails;
    }

    public void SetDetails<T>(T item) where T : class
    {
        Debug.Log("Details set");
        Team team = item as Team;
        if (team.GetTeamID() != GameManager.Instance.GetTeamID()) return;

        List<LineupItem> _lineupItems = _lineupItemsRoot.GetComponentsInChildren<LineupItem>(true).ToList();
        List<BenchItem> _benchItems = _benchItemsRoot.GetComponentsInChildren<BenchItem>(true).ToList();

        List<Player> players = team.GetPlayersFromIDs(team.GetLineup());
        List<Player> restPlayers = GetBench(team, players);

        for (int i = 0; i < _lineupItems.Count; i++)
        {
            int index = i;
            _lineupItems[i].SetPlayerDetails(players[index]);
        }

        for (int i = 0; i < _benchItems.Count; i++)
        {
            if (i < restPlayers.Count)
            {
                int index = i;
                _benchItems[i].gameObject.SetActive(true);
                _benchItems[i].SetPlayerDetails(restPlayers[index]);
            }
            else
            {
                _benchItems[i].gameObject.SetActive(false);
            }
        }
    }

    private List<Player> GetBench(Team team, List<Player> players)
    {
        List<Player> newPlayerList = new List<Player>();
        foreach (Player player in team.GetPlayersFromTeam())
        {
            if (!players.Contains(player))
            {
                newPlayerList.Add(player);
            }
        }

        return newPlayerList;
    }

    public void GoToPrematchView()
    {
        Navigation.Instance.GoToScreen(true, CanvasKey.Prematch, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()));
    }
}
