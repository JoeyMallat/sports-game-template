using System.Collections;
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
        teamList = teamList.OrderByDescending(x => x.GetAverageTeamRating()).ToList();

        for (int i = 0; i < teamList.Count; i++)
        {
            teamItems[i].SetTeamDetails(i + 1, teamList[i]);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_teamItemsRoot);
    }
}
