using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamTacticsView : MonoBehaviour, ISettable
{
    [SerializeField] Slider _minutesRemainingSlider;
    [SerializeField] TextMeshProUGUI _minutesRemainingText;
    [SerializeField] Transform _playerMinutesRoot;

    Team _team;

    public static event Action<int> OnMinutesRemainingUpdated;

    private void Awake()
    {
        MinutesBar.OnMinutesChanged += UpdateMinutesRemaining;
    }

    public void SetDetails<T>(T item) where T : class
    {
        _team = item as Team;

        List<PlayerMinutes> playerMinutes = _playerMinutesRoot.GetComponentsInChildren<PlayerMinutes>().ToList();
        SetPlayers(_team.GetPlayersFromTeam(), playerMinutes);
    }

    private void UpdateMinutesRemaining()
    {
        int totalMinutesSpent = 0;

        _team.GetPlayersFromTeam().ForEach(x => totalMinutesSpent += x.GetMinutes());

        _minutesRemainingSlider.value = 240 - totalMinutesSpent;
        _minutesRemainingSlider.onValueChanged?.Invoke(_minutesRemainingSlider.value);

        _minutesRemainingText.text = $"{_minutesRemainingSlider.value} minutes remaining";

        OnMinutesRemainingUpdated?.Invoke(240 - totalMinutesSpent);
    }

    private void SetPlayers(List<Player> players, List<PlayerMinutes> playerMinutes)
    {
        for (int i = 0; i < playerMinutes.Count; i++)
        {
            if (i < players.Count)
            {
                playerMinutes[i].gameObject.SetActive(true);
                playerMinutes[i].SetPlayerDetails(players[i]);
            }
            else
            {
                playerMinutes[i].gameObject.SetActive(false);
            }
        }
    }
}
