using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _placeText;
    [SerializeField] TextMeshProUGUI _teamNameText;
    [SerializeField] TextMeshProUGUI _winsText;
    [SerializeField] TextMeshProUGUI _lossesText;
    [SerializeField] TextMeshProUGUI _percentageText;
    [SerializeField] TextMeshProUGUI _gamesBackText;

    [SerializeField] Image _teamLogo;

    public void SetTeamDetails(int place, Team team, int mostWins)
    {
        TeamSeason teamSeason = team.GetCurrentSeasonStats();

        if (team.GetTeamID() == GameManager.Instance.GetTeamID())
        {
            _teamNameText.text = $"{team.GetTeamName()} <sprite name=\"star\">";
        }
        else
        {
            _teamNameText.text = team.GetTeamName();
        }

        _placeText.text = place.ToString();
        _teamLogo.sprite = team.GetTeamLogo();
        _winsText.text = teamSeason.GetWins().ToString();
        _lossesText.text = teamSeason.GetLosses().ToString();
        _percentageText.text = teamSeason.GetWinPercentage().ToString("F3");
        _gamesBackText.text = (mostWins - teamSeason.GetWins()).ToString();

        SetButton(team);
    }

    private void SetButton(Team team)
    {
        Button button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Team, team));
        }
    }
}
