using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MatchupItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _homeTeamName;
    [SerializeField] TextMeshProUGUI _homeTeamWins;

    [SerializeField] TextMeshProUGUI _awayTeamName;
    [SerializeField] TextMeshProUGUI _awayTeamWins;

    public void SetMatchup(PlayoffMatchup matchup)
    {
        _homeTeamName.text = $"{LeagueSystem.Instance.GetTeam(matchup.GetHomeTeamID()).GetTeamName()} <size=75%><color=#FF9900>({matchup.GetHomeTeamSeed()})";
        _awayTeamName.text = $"{LeagueSystem.Instance.GetTeam(matchup.GetAwayTeamID()).GetTeamName()} <size=75%><color=#FF9900>({matchup.GetAwayTeamSeed()})";

        _homeTeamWins.text = matchup.GetSeriesScore().Item1.ToString();
        _awayTeamWins.text = matchup.GetSeriesScore().Item2.ToString();
    }
}
