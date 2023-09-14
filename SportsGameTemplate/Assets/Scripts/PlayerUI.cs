using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour, ISettable
{
    [SerializeField] Image _playerPortrait;
    [SerializeField] TextMeshProUGUI _playerName;
    [SerializeField] TextMeshProUGUI _age;
    [SerializeField] TextMeshProUGUI _rating;
    [SerializeField] TextMeshProUGUI _height;
    [SerializeField] TextMeshProUGUI _team;
    [SerializeField] TextMeshProUGUI _contract;
    [SerializeField] TextMeshProUGUI _position;

    public void SetDetails<T>(T itemDetails) where T : class
    {
        Player player = itemDetails as Player;
        _playerPortrait.sprite = player.GetPlayerPortrait();
        _playerName.text = player.GetFullName();
        _age.text = player.GetAge().ToString();
        _rating.text = player.CalculateRatingForPosition().ToString();
        _height.text = $"6\'{UnityEngine.Random.Range(1, 11)}\"";
        _team.text = LeagueSystem.Instance.GetTeam(player.GetTeamID()).GetTeamName();
        _contract.text = $"{player.GetContract().GetYearlySalary().ConvertToMonetaryString()}\n{player.GetContract().GetYearsOnContract()} YRS";
        _position.text = player.GetPosition();
    }
}
