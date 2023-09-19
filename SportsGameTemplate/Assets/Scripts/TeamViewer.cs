using Sirenix.OdinInspector;
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
    [SerializeField] TextMeshProUGUI _teamNameText;

    public void SetDetails<T>(T item) where T : class
    {
        Team team = item as Team;
        _currentShowedTeam = team.GetTeamID();
        _teamNameText.text = team.GetTeamName();
        List<Player> players = team.GetPlayersFromTeam().OrderBy(x => x.GetPosition()).ToList();
        List<PlayerItem> playerItems = _playerRoot.GetComponentsInChildren<PlayerItem>().ToList();

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

        LayoutRebuilder.ForceRebuildLayoutImmediate(_playerRoot);
    }
}
