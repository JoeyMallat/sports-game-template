using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TeamViewer : MonoBehaviour
{
    [SerializeField][ReadOnly] int _currentShowedTeam;
    [SerializeField] RectTransform _playerRoot;

    private void Start()
    {
        ShowTeam(8);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            int random = UnityEngine.Random.Range(0, 30);
            ShowTeam(random);
        }
    }

    public void ShowTeam(int teamID)
    {
        _currentShowedTeam = teamID;

        Team team = LeagueSystem.Instance.GetTeam(teamID);
        List<Player> players = team.GetPlayersFromTeam().OrderBy(x => x.GetPosition()).ToList();
        List<PlayerItem> playerItems = _playerRoot.GetComponentsInChildren<PlayerItem>().ToList();

        for (int i = 0; i < playerItems.Count; i++)
        {
            if (i < players.Count)
            {
                playerItems[i].gameObject.SetActive(true);
                playerItems[i].SetPlayerDetails(players[i]);
            } else
            {
                playerItems[i].gameObject.SetActive(false);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_playerRoot);
    }
}
