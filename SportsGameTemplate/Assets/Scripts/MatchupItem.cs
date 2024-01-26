using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchupItem : MonoBehaviour
{
    [SerializeField] Image _homeTeamLogo;
    [SerializeField] TextMeshProUGUI _homeTeamName;
    [SerializeField] TextMeshProUGUI _homeTeamWins;

    [SerializeField] Image _awayTeamLogo;
    [SerializeField] TextMeshProUGUI _awayTeamName;
    [SerializeField] TextMeshProUGUI _awayTeamWins;

    public void SetMatchup(PlayoffMatchup matchup)
    {
        _homeTeamLogo.enabled = true;
        _awayTeamLogo.enabled = true;

        _homeTeamLogo.sprite = LeagueSystem.Instance.GetTeam(matchup.GetHomeTeamID()).GetTeamLogo();
        _awayTeamLogo.sprite = LeagueSystem.Instance.GetTeam(matchup.GetAwayTeamID()).GetTeamLogo();

        _homeTeamName.text = $"{LeagueSystem.Instance.GetTeam(matchup.GetHomeTeamID()).GetTeamName()} <size=75%><color=#FF9900>({LeagueSystem.Instance.GetTeam(matchup.GetHomeTeamID()).GetSeed()})";
        _awayTeamName.text = $"{LeagueSystem.Instance.GetTeam(matchup.GetAwayTeamID()).GetTeamName()} <size=75%><color=#FF9900>({LeagueSystem.Instance.GetTeam(matchup.GetAwayTeamID()).GetSeed()})";

        _homeTeamWins.text = matchup.GetSeriesScore().Item1.ToString();
        _awayTeamWins.text = matchup.GetSeriesScore().Item2.ToString();
    }

    public void EmptyMatchup()
    {
        _homeTeamLogo.enabled = false;
        _awayTeamLogo.enabled = false;

        _homeTeamName.text = "TBD";
        _awayTeamName.text = "TBD";

        _homeTeamWins.text = "";
        _awayTeamWins.text = "";
    }
}
