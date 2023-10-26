using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MM_PlayView : MonoBehaviour, ISettable
{
    [SerializeField] Image _homeTeamImage;
    [SerializeField] TextMeshProUGUI _homeTeamText;

    [SerializeField] Image _awayTeamImage;
    [SerializeField] TextMeshProUGUI _awayTeamText;

    public void SetDetails<T>(T item) where T : class
    {
        Team team = item as Team;

        Match nextMatch = LeagueSystem.Instance.GetNextMatchData();

        _homeTeamText.text = LeagueSystem.Instance.GetTeam(nextMatch.GetHomeTeamID()).GetTeamName();
        _awayTeamText.text = LeagueSystem.Instance.GetTeam(nextMatch.GetAwayTeamID()).GetTeamName();
    }
}
