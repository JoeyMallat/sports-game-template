using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour, ISettable
{
    [Header("Settings")]
    [SerializeField] Color _positiveChangeColor;
    [SerializeField] Color _negativeChanceColor;

    [Header("Player Details")]
    [SerializeField] Image _playerPortrait;
    [SerializeField] Image _teamLogo;
    [SerializeField] TextMeshProUGUI _playerName;
    [SerializeField] TextMeshProUGUI _age;
    [SerializeField] TextMeshProUGUI _rating;
    [SerializeField] TextMeshProUGUI _ratingDifferenceText;
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

    [SerializeField] GameObject _statObjectRoot;
    [SerializeField] StatObject _statObjectPrefab;

    [Header("Buttons")]
    [SerializeField] Button _extendContractButton;
    [SerializeField] Button _addToTradingBlock;
    [SerializeField] Button _addToTradeButton;

    [Header("Items")]
    [SerializeField] List<ItemSlot> _itemSlots;

    [Header("Tabs, buttons and menu targets")]
    [SerializeField] GameObject _itemsScreen;
    [SerializeField] GameObject _itemsTabTitle;

    public void SetDetails<T>(T itemDetails) where T : class
    {
        Player player = itemDetails as Player;
        _playerPortrait.sprite = player.GetPlayerPortrait();
        _playerName.text = player.GetFullName();
        _age.text = player.GetAge().ToString();
        _rating.text = player.CalculateRatingForPosition().ToString();
        SetRatingDifference(player);
        _height.text = $"6\'{UnityEngine.Random.Range(1, 11)}\"";
        _team.text = LeagueSystem.Instance.GetTeam(player.GetTeamID()).GetTeamName();
        _teamLogo.sprite = LeagueSystem.Instance.GetTeam(player.GetTeamID()).GetTeamLogo();
        _contract.text = $"{player.GetContract().GetYearlySalary().ConvertToMonetaryString()}\n{player.GetContract().GetYearsOnContract()} YRS";
        _position.text = player.GetPosition();

        SetItems(player);
        SetSkills(player);
        SetStats(player);
        SetButtons(player);
        SetTabs(player);
    }

    private void SetTabs(Player player)
    {
        bool inMyTeam = player.GetTeamID() == GameManager.Instance.GetTeamID();

        if (inMyTeam)
        {
            _itemsScreen.SetActive(true);
            _itemsTabTitle.SetActive(true);
        }
        else
        {
            _itemsScreen.SetActive(false);
            _itemsTabTitle.SetActive(false);
        }
    }

    private void SetRatingDifference(Player player)
    {
        if (player.GetSeasonImprovement() == 0)
        {
            _ratingDifferenceText.gameObject.SetActive(false);
            return;
        }

        _ratingDifferenceText.gameObject.SetActive(true);
        _ratingDifferenceText.text = player.GetSeasonImprovement() > 0 ? "+" + player.GetSeasonImprovement().ToString() : player.GetSeasonImprovement().ToString();
        _ratingDifferenceText.color = player.GetSeasonImprovement() > 0 ? _positiveChangeColor : _negativeChanceColor;
    }

    private void SetItems(Player player)
    {
        List<OwnedGameItem> items = player.GetEquippedItems();

        for (int i = 0; i < _itemSlots.Count; i++)
        {
            // Find all possible items that correspond with the game slot
            List<OwnedGameItem> gameItem = items.Where(x => ItemDatabase.Instance.GetGameItemByID(x.GetItemID()).GetItemType() == _itemSlots[i].GetItemType()).ToList();

            if (gameItem.Count == 0)
            {
                _itemSlots[i].SetSlot(player, null);
            }
            else
            {
                _itemSlots[i].SetSlot(player, gameItem[0]);
            }
        }
    }

    private void SetButtons(Player player)
    {
        int teamID = GameManager.Instance.GetTeamID();

        if (player.GetTeamID() == teamID)
        {
            _addToTradingBlock.gameObject.SetActive(true);
            _addToTradeButton.gameObject.SetActive(true);
            _extendContractButton.gameObject.SetActive(true);
            _extendContractButton.GetComponentInChildren<TextMeshProUGUI>().text = "Offer extension";
        }
        else if (player.GetTeamID() != teamID && GameManager.Instance.GetSeasonStage() == SeasonStage.OffSeason)
        {
            _addToTradingBlock.gameObject.SetActive(false);
            _addToTradeButton.gameObject.SetActive(false);
            _extendContractButton.gameObject.SetActive(true);
            _extendContractButton.GetComponentInChildren<TextMeshProUGUI>().text = "Offer contract";
        }
        else
        {
            _addToTradingBlock.gameObject.SetActive(false);
            _addToTradeButton.gameObject.SetActive(true);
            _extendContractButton.gameObject.SetActive(false);
        }

        if (player.GetAge() >= 40)
        {
            _extendContractButton.gameObject.SetActive(false);
        }

        _addToTradingBlock.onClick.RemoveAllListeners();
        _addToTradingBlock.onClick.AddListener(() => player.ToggleTradingBlockStatus());
        // TODO: Change Trading block status indicator
        _addToTradingBlock.onClick.AddListener(() => { _playerName.text = player.GetTradingBlockStatus() == true ? $"{player.GetFullName()}*" : player.GetFullName(); });

        _addToTradeButton.onClick.RemoveAllListeners();
        _addToTradeButton.onClick.AddListener(() => player.AddToTrade(player.GetTeamID()));

        _extendContractButton.onClick.RemoveAllListeners();
        _extendContractButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.ContractNegotiations, player));
    }

    private void SetStats(Player player)
    {
        _careerMinutes.text = player.GetAllTimeStats("minutes").ToString("F1");
        _careerPoints.text = player.GetAllTimePoints().ToString("F1");
        _careerAssists.text = player.GetAllTimeStats("assists").ToString("F1");
        _careerRebounds.text = player.GetAllTimeStats("rebounds").ToString("F1");

        _seasonMinutes.text = player.GetLatestSeason().GetAverageOfStat("minutes").ToString("F1");
        _seasonPoints.text = player.GetLatestSeason().GetAveragePoints().ToString("F1");
        _seasonAssists.text = player.GetLatestSeason().GetAverageOfStat("assists").ToString("F1");
        _seasonRebounds.text = player.GetLatestSeason().GetAverageOfStat("rebounds").ToString("F1");

        SetBoxScores(player);
    }

    private void SetBoxScores(Player player)
    {
        List<PlayerMatchStats> playerMatchStats = player.GetLatestSeason().GetMatchStats();
        int amountOfGames = playerMatchStats.Count;
        List<StatObject> statObjects = _statObjectRoot.GetComponentsInChildren<StatObject>(true).ToList();
        int amountOfObjectsToSpawn = Mathf.Clamp(amountOfGames - statObjects.Count, 0, 999);

        for (int i = 0; i < amountOfObjectsToSpawn; i++)
        {
            StatObject statObject = Instantiate(_statObjectPrefab);
            statObject.transform.SetParent(_statObjectRoot.transform, false);
            statObjects.Add(statObject);
        }

        for (int i = 0; i < statObjects.Count; i++)
        {
            int index = i;

            if (i < amountOfGames)
            {
                statObjects[i].gameObject.SetActive(true);
                statObjects[i].SetDetails(new StatObjectWrapper(GetMatchOpponent(playerMatchStats[index].GetMatchID()),
                    new List<float>() {
                    playerMatchStats[index].GetTotal("minutes"),
                    playerMatchStats[index].GetPoints(),
                    playerMatchStats[index].GetTotal("assists"),
                    playerMatchStats[index].GetTotal("rebounds") }
                    ));
            }
            else
            {
                statObjects[i].gameObject.SetActive(false);
            }
        }

        Canvas.ForceUpdateCanvases();
    }

    private string GetMatchOpponent(int matchID)
    {
        Match match = null;
        match = LeagueSystem.Instance.GetMatches().FirstOrDefault(x => x.GetMatchID() == matchID);

        if (match != null)
        {
            return $"{LeagueSystem.Instance.GetTeam(match.GetAwayTeamID()).GetTeamName()} @ {LeagueSystem.Instance.GetTeam(match.GetHomeTeamID()).GetTeamName()}";
        }
        else
        {
            foreach (var matchup in PlayoffSystem.Instance.GetAllPlayoffsMatchups())
            {
                match = matchup.GetMatches().FirstOrDefault(x => x.GetMatchID() == matchID);
                if (match != null)
                {
                    return $"{LeagueSystem.Instance.GetTeam(match.GetAwayTeamID()).GetTeamName()} @ {LeagueSystem.Instance.GetTeam(match.GetHomeTeamID()).GetTeamName()}";
                }
            }

            Debug.Log($"No match found with matchID {matchID}");
            return $"";
        }
    }

    private void SetSkills(Player player)
    {
        SkillBar[] skillBars = GetComponentsInChildren<SkillBar>();
        List<PlayerSkill> playerSkills = player.GetSkills();
        int skillCount = playerSkills.Count;

        // Set the overall as the top bar
        skillBars[0].gameObject.SetActive(true);
        skillBars[0].SetSkillBar("Overall", player.CalculateRatingForPosition());

        // Get the boosts for items
        List<OwnedGameItem> equippedItems = player.GetEquippedItems();
        List<SkillBoost> skillBoosts = new List<SkillBoost>();

        equippedItems.ForEach((x) => skillBoosts.AddRange(ItemDatabase.Instance.GetGameItemByID(x.GetItemID()).GetSkillBoosts()));

        for (int i = 1; i < skillBars.Length; i++)
        {
            int index = i;
            if (i < skillCount)
            {
                int boost = GetTotalBoost(skillBoosts, playerSkills[index].GetSkill());
                skillBars[i].gameObject.SetActive(true);
                if (boost == 0)
                    skillBars[i].SetSkillBar(playerSkills[index]);
                else
                    skillBars[i].SetSkillBar(playerSkills[index].GetSkill().ToString(), playerSkills[index].GetRatingForSkill(), boost);
            }
            else
            {
                skillBars[i].gameObject.SetActive(false);
            }
        }
    }

    private int GetTotalBoost(List<SkillBoost> skillBoosts, Skill skill)
    {
        int total = 0;

        skillBoosts.ForEach((x) => { if (x.GetSkill() == skill) { total += x.GetBoost(); }; });

        return total;
    }
}
