using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System;

public class FreeAgencyView : MonoBehaviour
{
    [SerializeField] GameObject _freeAgentRoot;
    [SerializeField] FreeAgentItem _freeAgentItem;

    private void Awake()
    {
        GameManager.OnPostSeasonStarted += LoadFreeAgents;
    }

    private void LoadFreeAgents(SeasonStage seasonStage, int week)
    {
        List<Player> freeAgents = LeagueSystem.Instance.GetAllPlayers().Where(x => x.GetContract().GetYearsOnContract() == 1).ToList();
        List<FreeAgentItem> spawnedItems = _freeAgentRoot.GetComponentsInChildren<FreeAgentItem>(true).ToList();

        ShowFreeAgents(freeAgents, spawnedItems);
    }

    private void ShowFreeAgents(List<Player> players, List<FreeAgentItem> freeAgentItems)
    {
        int itemsToSpawn = players.Count - freeAgentItems.Count;

        for (int i = 0; i < itemsToSpawn; i++)
        {
            freeAgentItems.Add(Instantiate(_freeAgentItem, _freeAgentRoot.transform));
        }

        for (int i = 0; i < freeAgentItems.Count; i++)
        {
            int index = i;
            freeAgentItems[i].gameObject.SetActive(true);
            freeAgentItems[i].SetPlayerAssets(players[index]);

            if (i > players.Count)
            {
                freeAgentItems[i].gameObject.SetActive(false);
            }
        }
    }
}
