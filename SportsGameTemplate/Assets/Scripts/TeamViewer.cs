using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamViewer : MonoBehaviour, ISettable
{
    [SerializeField][ReadOnly] int _currentShowedTeam;
    [SerializeField] RectTransform _playerRoot;
    [SerializeField] RectTransform _pickRoot;
    [SerializeField] TextMeshProUGUI _teamNameText;

    public void SetDetails<T>(T item) where T : class
    {
        Team team = item as Team;
        _currentShowedTeam = team.GetTeamID();
        _teamNameText.text = team.GetTeamName();
        List<Player> players = team.GetPlayersFromTeam().OrderBy(x => x.GetPosition()).ToList();
        List<DraftPick> draftPicks = team.GetDraftPicks().OrderBy(x => x.GetTotalPickNumber()).ToList();

        List<PlayerItem> playerItems = _playerRoot.GetComponentsInChildren<PlayerItem>().ToList();
        List<DraftPickItem> pickItems = _pickRoot.GetComponentsInChildren<DraftPickItem>().ToList();

        SetPlayers(players, playerItems);
        SetDraftPicks(draftPicks, pickItems);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_playerRoot);
    }

    private void SetDraftPicks(List<DraftPick> draftPicks, List<DraftPickItem> pickItems)
    {
        for (int i = 0; i < pickItems.Count; i++)
        {
            if (i < draftPicks.Count)
            {
                pickItems[i].gameObject.SetActive(true);
                pickItems[i].SetPickDetails(draftPicks[i]);
            }
            else
            {
                pickItems[i].gameObject.SetActive(false);
            }
        }
    }

    private void SetPlayers(List<Player> players, List<PlayerItem> playerItems)
    {
        for (int i = 0; i < playerItems.Count; i++)
        {
            if (i < players.Count)
            {
                playerItems[i].gameObject.SetActive(true);
                playerItems[i].SetPlayerDetails(players[i]);
            }
            else
            {
                playerItems[i].gameObject.SetActive(false);
            }
        }
    }
}
