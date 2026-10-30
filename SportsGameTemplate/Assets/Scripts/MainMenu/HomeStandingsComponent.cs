using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HomeStandingsComponent : MonoBehaviour, ISettable
{
    [SerializeField] Transform _standingsRoot;
    [SerializeField] TeamItem _teamItemPrefab;

    Coroutine _layoutRefreshCoroutine;

    public void SetDetails<T>(T item, bool showMatchButton = false) where T : class
    {
        List<Team> standings = item as List<Team>;

        if (standings == null || standings.Count == 0)
        {
            return;
        }

        List<TeamItem> teamItems = _standingsRoot.GetComponentsInChildren<TeamItem>(true).ToList();
        int visibleTeamCount = Mathf.Min(teamItems.Count, standings.Count);
        int myTeamIndex = standings.FindIndex(team => team.GetTeamID() == GameManager.Instance.GetTeamID());
        int centeredOffset = (visibleTeamCount - 1) / 2;
        int startIndex = Mathf.Clamp(myTeamIndex - centeredOffset, 0, standings.Count - visibleTeamCount);
        int mostWins = standings.Max(team => team.GetCurrentSeasonStats().GetWins());

        for (int i = 0; i < teamItems.Count; i++)
        {
            bool hasTeam = i < visibleTeamCount;
            teamItems[i].gameObject.SetActive(hasTeam);

            if (!hasTeam)
            {
                continue;
            }

            int standingsIndex = startIndex + i;
            teamItems[i].SetTeamDetails(standingsIndex + 1, standings[standingsIndex], mostWins);
        }

        if (_layoutRefreshCoroutine != null)
            StopCoroutine(_layoutRefreshCoroutine);
        _layoutRefreshCoroutine = StartCoroutine(RefreshLayoutAfterPopulation());
    }

    IEnumerator RefreshLayoutAfterPopulation()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();

        RectTransform current = _standingsRoot as RectTransform;
        while (current != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(current);
            current = current.parent as RectTransform;
        }

        Canvas.ForceUpdateCanvases();
        _layoutRefreshCoroutine = null;
    }
}
