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

    [SerializeField] TextMeshProUGUI _teamLogo;

    public void SetTeamDetails(int place, Team team)
    {
        _placeText.text = place.ToString();
        _teamLogo.text = team.GetTeamAbbreviation().ToUpper();
        _teamNameText.text = team.GetTeamName();
        _winsText.text = 0.ToString();
        _lossesText.text = 0.ToString();
        _percentageText.text = "0.000";
        _gamesBackText.text = "-";

        SetButton(team);
    }

    private void SetButton(Team team)
    {
        Button button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, Navigation.Instance.GetCanvas(CanvasKey.Team), team));
        }
    }
}
