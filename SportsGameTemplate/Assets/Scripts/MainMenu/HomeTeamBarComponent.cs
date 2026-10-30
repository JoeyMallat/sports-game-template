using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeTeamBarComponent : MonoBehaviour, ISettable
{
    [SerializeField] TextMeshProUGUI _teamNameText;
    [SerializeField] TextMeshProUGUI _positionInLeague;
    [SerializeField] TextMeshProUGUI _moraleText;
    [SerializeField] TextMeshProUGUI _injuriesText;
    [SerializeField] TextMeshProUGUI _ratingText;

    [SerializeField] Image _teamLogo;
    [SerializeField] Image _moraleFill;
    [SerializeField] Image _ratingFill;

    public void SetDetails<T>(T item, bool showMatchButton = false) where T : class
    {
        Team team = item as Team;

        if (team == null)
        {
            return;
        }

        List<Team> standings = LeagueSystem.Instance.GetTeams();
        int leaguePosition = standings.IndexOf(team) + 1;
        int rating = team.GetPlayersFromTeam().Count > 0 ? team.GetAverageTeamRating() : 0;

        _teamNameText.text = team.GetTeamName();
        _positionInLeague.text = $"{GetOrdinal(leaguePosition)} in league";
        _moraleText.text = "100%";
        _injuriesText.text = "0";
        _ratingText.text = rating.ToString();

        _teamLogo.sprite = team.GetTeamLogo();
        _moraleFill.fillAmount = 1f;
        _ratingFill.fillAmount = Mathf.Clamp01(rating / 99f);
    }

    private string GetOrdinal(int number)
    {
        int lastTwoDigits = number % 100;

        if (lastTwoDigits >= 11 && lastTwoDigits <= 13)
        {
            return $"{number}th";
        }

        switch (number % 10)
        {
            case 1:
                return $"{number}st";
            case 2:
                return $"{number}nd";
            case 3:
                return $"{number}rd";
            default:
                return $"{number}th";
        }
    }
}
