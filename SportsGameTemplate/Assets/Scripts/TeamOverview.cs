using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TeamOverview : MonoBehaviour, ISettable
{
    [SerializeField] RectTransform _teamItemsRoot;

    public void SetDetails<T>(T item) where T : class
    {
        List<Team> teams = item as List<Team>;

        List<PickTeamItem> teamItems = _teamItemsRoot.GetComponentsInChildren<PickTeamItem>().ToList();

        for (int i = 0; i < teamItems.Count; i++)
        {
            if (i < teams.Count)
            {
                int index = i;
                teamItems[i].gameObject.SetActive(true);
                teamItems[i].SetTeamDetails(teams[index], () => { TeamOverviewActions(index); });
            }
            else
            {
                teamItems[i].gameObject.SetActive(false);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_teamItemsRoot);
    }

    private void TeamOverviewActions(int index)
    {
        if (!GameManager.Instance.GetTeamPickedStatus())
        {
            FindFirstObjectByType<TeamSelection>().SelectTeam(index);
            Navigation.Instance.CloseCanvas(GetComponent<Canvas>());
        }
        else
        {
            Navigation.Instance.GoToScreen(true, CanvasKey.Team, LeagueSystem.Instance.GetTeam(index));
        }
    }
}
