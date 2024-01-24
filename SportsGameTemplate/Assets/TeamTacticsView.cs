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

    public static event Action<int, string> OnMinutesForPositionRemainingUpdated;

    private void Awake()
    {
        MinutesBar.OnMinutesChanged += UpdateMinutesRemaining;
    }

    public void SetDetails<T>(T item) where T : class
    {
        _team = item as Team;

        List<PlayerMinutes> playerMinutes = _playerMinutesRoot.GetComponentsInChildren<PlayerMinutes>(true).ToList();
        SetPlayers(_team.GetPlayersFromTeam().OrderBy(x => x.GetPosition()).ToList(), playerMinutes);
    }

    private void UpdateMinutesRemaining(string position)
    {
        int totalMinutesSpent = 0;
        int totalPositionMinutes = 0;

        _team.GetPlayersFromTeam().ForEach(x => totalMinutesSpent += x.GetMinutes());
        _team.GetPlayersFromTeam().Where(x => x.GetPosition() == position).ToList().ForEach(x => totalPositionMinutes += x.GetMinutes());

        _minutesRemainingSlider.value = 240 - totalMinutesSpent;
        _minutesRemainingSlider.onValueChanged?.Invoke(_minutesRemainingSlider.value);

        _minutesRemainingText.text = $"{_minutesRemainingSlider.value} minutes remaining";

        OnMinutesForPositionRemainingUpdated?.Invoke(48 - totalPositionMinutes, position);
    }

    private void SetPlayers(List<Player> players, List<PlayerMinutes> playerMinutes)
    {
        for (int i = 0; i < playerMinutes.Count; i++)
        {
            if (i < players.Count)
            {
                int index = i;
                playerMinutes[i].SetPlayerDetails(players[index]);
                playerMinutes[i].gameObject.SetActive(true);
            }
            else
            {
                playerMinutes[i].gameObject.SetActive(false);
            }
        }
    }

    public void AutoAssignMinutes()
    {
        AssignMinutesPerPosition(_team.GetPlayersFromTeam().Where(x => x.GetPosition() == "Point Guard").ToList());
        AssignMinutesPerPosition(_team.GetPlayersFromTeam().Where(x => x.GetPosition() == "Shooting Guard").ToList());
        AssignMinutesPerPosition(_team.GetPlayersFromTeam().Where(x => x.GetPosition() == "Small Forward").ToList());
        AssignMinutesPerPosition(_team.GetPlayersFromTeam().Where(x => x.GetPosition() == "Power Forward").ToList());
        AssignMinutesPerPosition(_team.GetPlayersFromTeam().Where(x => x.GetPosition() == "Center").ToList());

        SetDetails(_team);
    }

    private void AssignMinutesPerPosition(List<Player> players)
    {
        int totalRating = 0;
        players.ForEach(x => totalRating += x.CalculateRatingForPosition());
        int totalMinutesSet = 0;

        foreach (Player player in players)
        {
            float share = player.CalculateRatingForPosition() / (float)totalRating;
            totalMinutesSet += (int)(share * 48f);
            player.SetMinutes((int)(share * 48f));
            Debug.Log($"{player.GetFullName()} on position {player.GetPosition()} has a rating of {player.CalculateRatingForPosition()}, so has a share of {share}");
        }

        if (totalMinutesSet < 48)
        {
            Player player = players.OrderByDescending(x => x.CalculateRatingForPosition()).ToList()[0];
            player.SetMinutes(player.GetMinutes() + 48 - totalMinutesSet);
        }
    }
}
