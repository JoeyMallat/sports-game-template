using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegularSeasonPlayView : MonoBehaviour
{
    [SerializeField] Image _homeTeamImage;
    [SerializeField] TextMeshProUGUI _homeTeamText;

    [SerializeField] Image _awayTeamImage;
    [SerializeField] TextMeshProUGUI _awayTeamText;

    public void SetDetails(Team team)
    {
        Match nextMatch = LeagueSystem.Instance.GetNextMatchData();
        Team homeTeam = LeagueSystem.Instance.GetTeam(nextMatch.GetHomeTeamID());
        Team awayTeam = LeagueSystem.Instance.GetTeam(nextMatch.GetAwayTeamID());

        _homeTeamText.text = homeTeam.GetTeamName();
        _homeTeamImage.sprite = homeTeam.GetTeamLogo();

        _awayTeamText.text = awayTeam.GetTeamName();
        _awayTeamImage.sprite = awayTeam.GetTeamLogo();
    }
}
