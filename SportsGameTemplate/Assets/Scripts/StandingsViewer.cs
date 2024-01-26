using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StandingsViewer : MonoBehaviour, ISettable
{
    [SerializeField] RectTransform _teamItemsRoot;

    public void SetDetails<T>(T item) where T : class
    {
        List<Team> teamList = item as List<Team>;
        List<TeamItem> teamItems = _teamItemsRoot.GetComponentsInChildren<TeamItem>().ToList();
        //teamList = teamList.OrderByDescending(x => x.GetCurrentSeasonStats().GetWinPercentage()).ToList();
        int mostWins = teamList[0].GetCurrentSeasonStats().GetWins();

        for (int i = 0; i < teamItems.Count; i++)
        {
            if (i < teamList.Count)
            {
                teamItems[i].gameObject.SetActive(true);
                teamItems[i].SetTeamDetails(i + 1, teamList[i], mostWins);
            }
            else
            {
                teamItems[i].gameObject.SetActive(false);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_teamItemsRoot);
    }
}
