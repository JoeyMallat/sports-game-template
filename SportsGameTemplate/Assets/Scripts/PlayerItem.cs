using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItem : MonoBehaviour
{
    [SerializeField] Image _playerPortrait;
    [SerializeField] TextMeshProUGUI _playerFirstName;
    [SerializeField] TextMeshProUGUI _playerLastName;
    [SerializeField] TextMeshProUGUI _jerseyNumber; //Format: "#23"
    [SerializeField] TextMeshProUGUI _playerPosition; //Format: "PG", "SG", "SF", "PF", "C"
    [SerializeField] TextMeshProUGUI _playerRating;
    [SerializeField] TextMeshProUGUI _pointsPerGame;
    [SerializeField] TextMeshProUGUI _assistsPerGame;
    [SerializeField] TextMeshProUGUI _reboundsPerGame; //Format (all stats): "10.2"
    [SerializeField] TextMeshProUGUI _salary; //Format: "$1,0m"
    [SerializeField] Image _teamLogo;

    public void SetPlayerDetails(Player player, bool withLink = true, bool withPosition = true)
    {
        if (_playerPortrait != null)
            _playerPortrait.sprite = player.GetPlayerPortrait();

        if (_playerFirstName != null)
            _playerFirstName.text = _playerLastName == null ? player.GetFullName() : player.GetFirstName();

        if (_playerLastName != null)
            _playerLastName.text = player.GetLastName();

        if (_jerseyNumber != null)
            _jerseyNumber.text = $"#{player.GetJerseyNumber()}";

        if (_playerPosition != null)
            _playerPosition.text = withPosition ? GetPositionAbbreviation(player.GetPosition()) : string.Empty;

        if (_playerRating != null)
            _playerRating.text = player.CalculateRatingForPosition().ToString();

        PlayerSeason season = player.GetLatestSeason();

        if (_pointsPerGame != null)
            _pointsPerGame.text = season.GetAveragePoints().ToString("F1");

        if (_assistsPerGame != null)
            _assistsPerGame.text = season.GetAverageOfStat("assists").ToString("F1");

        if (_reboundsPerGame != null)
            _reboundsPerGame.text = season.GetAverageOfStat("rebounds").ToString("F1");

        if (_salary != null)
            _salary.text = $"${player.GetContract().GetYearlySalary().ConvertToMonetaryString()}";

        SetTeamLogo(player.GetTeamID());

        if (withLink)
            SetButton(player);
    }

    private string GetPositionAbbreviation(string position)
    {
        switch (position)
        {
            case "Point Guard":
                return "PG";
            case "Shooting Guard":
                return "SG";
            case "Small Forward":
                return "SF";
            case "Power Forward":
                return "PF";
            case "Center":
                return "C";
            default:
                return position;
        }
    }

    private void SetTeamLogo(int teamID)
    {
        if (_teamLogo == null)
            return;

        bool hasTeam = teamID >= 0;
        _teamLogo.gameObject.SetActive(hasTeam);

        if (hasTeam)
            _teamLogo.sprite = LeagueSystem.Instance.GetTeam(teamID).GetTeamLogo();
    }

    private void SetButton(Player player)
    {
        Button button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Player, player));
        }
    }
}
