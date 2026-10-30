using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeRosterComponent : MonoBehaviour, ISettable
{
    [SerializeField] Button _button;
    [SerializeField] TextMeshProUGUI _amountOfPlayersText;
    [SerializeField] TextMeshProUGUI _overallRatingText;
    [SerializeField] Image _frontPlayerHeadshot;
    [SerializeField] Image _backPlayerHeadshot;

    public void SetDetails<T>(T item, bool showMatchButton = false) where T : class
    {
        List<Player> players = item as List<Player>;

        if (item is Team team)
        {
            players = team.GetPlayersFromTeam();
        }

        if (players == null)
        {
            return;
        }

        _amountOfPlayersText.text = players.Count.ToString();
        _overallRatingText.text = players.Count > 0
            ? Mathf.RoundToInt((float)players.Average(player => player.CalculateRatingForPosition())).ToString()
            : "0";

        List<Player> topPlayers = players
            .OrderByDescending(player => player.CalculateRatingForPosition())
            .Take(2)
            .ToList();

        SetHeadshot(_frontPlayerHeadshot, topPlayers, 0);
        SetHeadshot(_backPlayerHeadshot, topPlayers, 1);

        if (_button != null)
        {
            Team selectedTeam = LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID());
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Team, selectedTeam, showMatchButton));
        }
    }

    private void SetHeadshot(Image headshot, List<Player> players, int index)
    {
        bool hasPlayer = index < players.Count;
        headshot.gameObject.SetActive(hasPlayer);

        if (hasPlayer)
        {
            headshot.sprite = players[index].GetPlayerPortrait();
        }
    }
}
