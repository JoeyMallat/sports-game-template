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
    [SerializeField] Button _selectAutomaticallyButton;
    [SerializeField] Button _clearButton;
    [SerializeField] Button _goToGameButton;

    private void Awake()
    {
        Team.OnLineupChanged += SetDetails;
    }

    public void SetDetails<T>(T item) where T : class
    {
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

        _selectAutomaticallyButton.onClick.RemoveAllListeners();
        _selectAutomaticallyButton.onClick.AddListener(() => team.GenerateLineup());

        _clearButton.onClick.RemoveAllListeners();
        _clearButton.onClick.AddListener(() => team.EmptyLineup());

        _goToGameButton.ToggleButtonStatus(players.Count == 5);
        _goToGameButton.onClick.RemoveAllListeners();
        _goToGameButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.MatchOptions));
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
        TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(true, CanvasKey.Prematch, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID())));
    }
}
