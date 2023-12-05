using Firebase.Analytics;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.UI;

public class DraftPlayerUI : MonoBehaviour, ISettable
{
    [SerializeField] Image _playerPortrait;
    [SerializeField] TextMeshProUGUI _playerName;
    [SerializeField] TextMeshProUGUI _age;
    [SerializeField] TextMeshProUGUI _rating;
    [SerializeField] TextMeshProUGUI _height;
    [SerializeField] TextMeshProUGUI _scoutingPercentage;
    [SerializeField] TextMeshProUGUI _potential;
    [SerializeField] TextMeshProUGUI _position;

    [SerializeField] Button _scoutButton;
    [SerializeField] Button _draftButton;

    public static event Action<Player> OnPlayerDrafted;

    public void SetDetails<T>(T itemDetails) where T : class
    {
        Player player = itemDetails as Player;
        _playerPortrait.sprite = player.GetPlayerPortrait();
        _playerName.text = player.GetFullName();
        _age.text = player.GetAge().ToString();

        _rating.text = player.CalculateRatingForPosition().GetRatingRange(player.GetScoutingPercentage(), player.GetFullName().GetHashCode());
        _scoutingPercentage.text = $"{(player.GetScoutingPercentage() * 100).ToString("F0")}%";
        _potential.text = player.GetPotential().GetPotentialRange(player.GetScoutingPercentage(), player.GetFullName().GetHashCode());
        _height.text = $"6\'{UnityEngine.Random.Range(1, 11)}\"";
        _position.text = player.GetPosition();

        SetSkills(player);
        SetButtons(player);
    }

    private void SetButtons(Player player)
    {
        _scoutButton.ToggleButtonStatus(player.GetScoutingPercentage() <= .99f);

        _scoutButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Scout player <color=\"white\">\n{RemoteConfigService.Instance.appConfig.GetInt("scoutplayer_cost", 2)} <sprite name=\"Gem\">";
        _scoutButton.onClick.RemoveAllListeners();
        if (GameManager.Instance.CheckBuyItem(2))
        {
            _scoutButton.onClick.AddListener(() => player.ScoutPlayer());
            _scoutButton.onClick.AddListener(() => FirebaseAnalytics.LogEvent("player_scouted", new Parameter("percentage_scouted", player.GetScoutingPercentage())));
            _scoutButton.onClick.AddListener(() => SetDetails(player));
        }
        else
        {
            _scoutButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Store));
        }

        _draftButton.ToggleButtonStatus(FindAnyObjectByType<DraftSystem>().UserNowPicking());

        _draftButton.onClick.RemoveAllListeners();
        _draftButton.onClick.AddListener(() => OnPlayerDrafted?.Invoke(player));
        _draftButton.onClick.AddListener(() => FirebaseAnalytics.LogEvent("player_drafted", new Parameter("percentage_scouted", player.GetScoutingPercentage())));
        _draftButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(false, CanvasKey.Draft));
    }

    private void SetSkills(Player player)
    {
        SkillBar[] skillBars = GetComponentsInChildren<SkillBar>();
        List<PlayerSkill> playerSkills = player.GetSkills();
        int skillCount = playerSkills.Count;

        // Set the overall as the top bar
        skillBars[0].gameObject.SetActive(true);
        float accuracy = player.GetScoutingPercentage();

        if (accuracy < 1f)
        {
            int overall = player.CalculateRatingForPosition();
            int seed = player.GetFullName().GetHashCode();

            skillBars[0].SetSkillBarWithRange("Overall", overall.GetRatingRangeNumbers(accuracy, seed).Item1, overall.GetRatingRangeNumbers(accuracy, seed).Item2);

            for (int i = 1; i < skillBars.Length; i++)
            {
                int index = i;
                if (i < skillCount)
                {
                    skillBars[i].gameObject.SetActive(true);
                    skillBars[i].SetSkillBarWithRange(playerSkills[index].GetSkill().ToString(), playerSkills[index].GetRatingForSkill().GetRatingRangeNumbers(accuracy, seed).Item1, playerSkills[index].GetRatingForSkill().GetRatingRangeNumbers(accuracy, seed).Item2);
                }
                else
                {
                    skillBars[i].gameObject.SetActive(false);
                }
            }
        } else
        {
            int overall = player.CalculateRatingForPosition();
            skillBars[0].SetSkillBar("Overall", overall);

            for (int i = 1; i < skillBars.Length; i++)
            {
                int index = i;
                if (i < skillCount)
                {
                    skillBars[i].gameObject.SetActive(true);
                    skillBars[i].SetSkillBar(playerSkills[index]);
                }
                else
                {
                    skillBars[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
