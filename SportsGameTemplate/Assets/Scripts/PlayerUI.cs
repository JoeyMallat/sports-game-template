using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour, ISettable
{
    [SerializeField] Image _playerPortrait;
    [SerializeField] Image _teamLogo;
    [SerializeField] TextMeshProUGUI _playerName;
    [SerializeField] TextMeshProUGUI _age;
    [SerializeField] TextMeshProUGUI _rating;
    [SerializeField] TextMeshProUGUI _height;
    [SerializeField] TextMeshProUGUI _team;
    [SerializeField] TextMeshProUGUI _contract;
    [SerializeField] TextMeshProUGUI _position;

    [Header("Stats")]
    [SerializeField] TextMeshProUGUI _careerMinutes;
    [SerializeField] TextMeshProUGUI _careerPoints;
    [SerializeField] TextMeshProUGUI _careerAssists;
    [SerializeField] TextMeshProUGUI _careerRebounds;
    [SerializeField] TextMeshProUGUI _seasonMinutes;
    [SerializeField] TextMeshProUGUI _seasonPoints;
    [SerializeField] TextMeshProUGUI _seasonAssists;
    [SerializeField] TextMeshProUGUI _seasonRebounds;

    [SerializeField] Button _extendContractButton;
    [SerializeField] Button _addToTradingBlock;
    [SerializeField] Button _addToTradeButton;

    public void SetDetails<T>(T itemDetails) where T : class
    {
        Player player = itemDetails as Player;
        _playerPortrait.sprite = player.GetPlayerPortrait();
        _playerName.text = player.GetFullName();
        _age.text = player.GetAge().ToString();
        _rating.text = player.CalculateRatingForPosition().ToString();
        _height.text = $"6\'{UnityEngine.Random.Range(1, 11)}\"";
        _team.text = LeagueSystem.Instance.GetTeam(player.GetTeamID()).GetTeamName();
        _teamLogo.sprite = LeagueSystem.Instance.GetTeam(player.GetTeamID()).GetTeamLogo();
        _contract.text = $"{player.GetContract().GetYearlySalary().ConvertToMonetaryString()}\n{player.GetContract().GetYearsOnContract()} YRS";
        _position.text = player.GetPosition();

        SetSkills(player);
        SetStats(player);
        SetButtons(player);
    }

    private void SetButtons(Player player)
    {
        _addToTradingBlock.onClick.RemoveAllListeners();
        _addToTradingBlock.onClick.AddListener(() => player.ToggleTradingBlockStatus());
        // TODO: Change Trading block status indicator
        _addToTradingBlock.onClick.AddListener(() => { _playerName.text = player.GetTradingBlockStatus() == true ? $"{player.GetFullName()}*" : player.GetFullName(); });

        _addToTradeButton.onClick.RemoveAllListeners();
        _addToTradeButton.onClick.AddListener(() => player.AddToTrade(player.GetTeamID()));
    }

    private void SetStats(Player player)
    {
        _seasonMinutes.text = player.GetLatestSeason().GetAverageOfStat("minutes").ToString("F1");
        _seasonPoints.text = player.GetLatestSeason().GetAveragePoints().ToString("F1");
        _seasonAssists.text = player.GetLatestSeason().GetAverageOfStat("assists").ToString("F1");
        _seasonRebounds.text = player.GetLatestSeason().GetAverageOfStat("rebounds").ToString("F1");
    }

    private void SetSkills(Player player)
    {
        SkillBar[] skillBars = GetComponentsInChildren<SkillBar>();
        List<PlayerSkill> playerSkills = player.GetSkills();
        int skillCount = playerSkills.Count;

        // Set the overall as the top bar
        skillBars[0].gameObject.SetActive(true);
        skillBars[0].SetSkillBar("Overall", player.CalculateRatingForPosition());

        for (int i = 1; i < skillBars.Length; i++)
        {
            int index = i;
            if (i < skillCount)
            {
                skillBars[i].gameObject.SetActive(true);
                skillBars[i].SetSkillBar(playerSkills[index]);
            } else
            {
                skillBars[i].gameObject.SetActive(false);
            }
        }
    }
}
