using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradingSystem : MonoBehaviour
{
    [InfoBox("@GetTotalTradeValue(_teamATradingAssets)")]
    [SerializeField] int _teamAID = 0;
    [SerializeReference] List<ITradeable> _teamATradingAssets;
    [InfoBox("@GetTotalTradeValue(_teamBTradingAssets)")]
    [SerializeField] int _teamBID = 10;
    [SerializeReference] List<ITradeable> _teamBTradingAssets;

    [Header("Trade confirmation UI")]
    [SerializeField] string _tradeValueTooLowString;
    [SerializeField] string _overSalaryCapString;
    [SerializeField] string _willAcceptString;
    [SerializeField] TextMeshProUGUI _willAcceptText;
    [SerializeField] Button _confirmTradeButton;

    public static event Action<int, int, List<ITradeable>> OnAssetsUpdated;

    private void Awake()
    {
        Player.OnAddedToTrade += AddAssetToTrade;
        TeamAsset.OnRemoveFromTrade += RemoveFromTrade;
        OnAssetsUpdated += CheckTradeWillingness;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            DebugCounterOffer();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            GenerateRandomAITrade();
        }
    }

    [Button(ButtonSizes.Large)]
    public void ConfirmTrade()
    {
        TradeAssets(_teamAID, _teamBID, _teamATradingAssets);
        TradeAssets(_teamBID, _teamAID, _teamBTradingAssets);
    }

    private void TradeAssets(int currentTeamID, int newTeamID, List<ITradeable> assets)
    {
        foreach (ITradeable asset in assets)
        {
            if (asset.GetType() == typeof(Player))
            {
                Player playerInAsset = (Player)asset;
                TradePlayer(currentTeamID, newTeamID, playerInAsset);
            } else if (asset.GetType() == typeof(DraftPick))
            {
                DraftPick draftPick = (DraftPick)asset;
                TradePick(currentTeamID, newTeamID, draftPick);
            }
        }
    }

    public void ClearTrades()
    {
        _teamATradingAssets = new List<ITradeable>();
        _teamBTradingAssets = new List<ITradeable>();

        UpdateBothTeamsAssets();
    }

    private void TradePlayer(int currentTeamID, int newTeamID, Player player)
    {
        LeagueSystem.Instance.GetTeam(currentTeamID).RemovePlayer(player);
        LeagueSystem.Instance.GetTeam(newTeamID).AddPlayer(player);
    }

    private void TradePick(int currentTeamID, int newTeamID, DraftPick pick)
    {
        LeagueSystem.Instance.GetTeam(currentTeamID).RemoveDraftPick(pick);
        LeagueSystem.Instance.GetTeam(newTeamID).AddDraftPick(pick);
    }

    public void DebugCounterOffer()
    {
        _teamBTradingAssets = GetOffer(1);
    }

    public void AddAssetToTrade(int team, ITradeable assetToAdd)
    {
        if (_teamAID == team)
        {
            if (_teamATradingAssets.Contains(assetToAdd)) return;

            _teamATradingAssets.Add(assetToAdd);

        }
        else if (_teamBID == team || _teamBTradingAssets.Count <= 0)
        {
            if (_teamBTradingAssets.Contains(assetToAdd)) return;

            _teamBTradingAssets.Add(assetToAdd);
            _teamBID = team;
        }
        else
        {
            ClearTrades();
            _teamATradingAssets.Add(assetToAdd);
            _teamAID = team;
        }

        UpdateBothTeamsAssets();
    }

    private void RemoveFromTrade(int teamID, ITradeable asset)
    {
        if (teamID == 0)
        {
            _teamATradingAssets.Remove(asset);
            UpdateBothTeamsAssets();
        }
        else if (teamID == 1)
        {
            _teamBTradingAssets.Remove(asset);
            UpdateBothTeamsAssets();
        }
    }

    public void GenerateRandomAITrade()
    {
        _teamAID = UnityEngine.Random.Range(0, LeagueSystem.Instance.GetTeams().Count - 1);
        AITrader aiTrader = new AITrader();
        _teamATradingAssets = aiTrader.GenerateOffer(UnityEngine.Random.Range(100, 450), LeagueSystem.Instance.GetTeam(_teamAID).GetTradeAssets());

        _teamBID = UnityEngine.Random.Range(0, LeagueSystem.Instance.GetTeams().Count - 1);
        _teamBTradingAssets = GetOffer(1);

        UpdateBothTeamsAssets();
    }

    public int GetTotalTradeValue(List<ITradeable> assets)
    {
        int total = 0;
        foreach (ITradeable asset in assets)
        {
            total += asset.CalculateTradeValue();
        }

        return total;
    }

    public List<ITradeable> GetOffer(int teamToOffer = 1)
    {
        AITrader aiTrader = new AITrader();

        switch (teamToOffer)
        {
            case 0:
                return aiTrader.GenerateOffer(GetTotalTradeValue(_teamBTradingAssets), LeagueSystem.Instance.GetTeam(_teamAID).GetTradeAssets());
            case 1:
                return aiTrader.GenerateOffer(GetTotalTradeValue(_teamATradingAssets), LeagueSystem.Instance.GetTeam(_teamBID).GetTradeAssets());
            default:
                break;
        }

        return null;
    }

    public bool CheckTradeEligibility(int teamID, List<ITradeable> assetsToReceive, List<ITradeable> assetsToSendAway)
    {
        int totalSalaryToAdd = 0;

        foreach (ITradeable asset in assetsToReceive)
        {
            if (asset.GetType() == typeof(Player))
            {
                Player player = (Player)asset;
                totalSalaryToAdd += player.GetContract().GetYearlySalary();
            }
        }

        int totalSalaryToReceive = 0;

        foreach (ITradeable asset in assetsToSendAway)
        {
            if (asset.GetType() == typeof(Player))
            {
                Player player = (Player)asset;
                totalSalaryToReceive += player.GetContract().GetYearlySalary();
            }
        }

        int totalSalary = totalSalaryToAdd - totalSalaryToReceive;

        return CheckSalaryForSalaryCap(teamID, totalSalary);
    }

    public bool CheckSalaryForSalaryCap(int teamID, int totalSalary)
    {
        Team team = LeagueSystem.Instance.GetTeam(teamID);

        return team.GetTotalSalaryAmount() + totalSalary < ConfigManager.Instance.GetCurrentConfig().SalaryCap;
    }

    public void CheckTradeWillingness(int teamIndex, int teamID, List<ITradeable> assets)
    {
        if (!CheckTradeEligibility(_teamBID, _teamATradingAssets, _teamBTradingAssets))
        {
            Debug.Log("Salary cap issues");
            UpdateAcceptButtonAndText(false, _overSalaryCapString);
            return;
        }

        if (GetTotalTradeValue(_teamATradingAssets) < GetTotalTradeValue(_teamBTradingAssets))
        {
            Debug.Log("Trade value issues");
            UpdateAcceptButtonAndText(false, _tradeValueTooLowString);
            return;
        }
        else
        {
            Debug.Log("Will accept!");
            UpdateAcceptButtonAndText(true, _willAcceptString);
            return;
        }
    }

    private void UpdateAcceptButtonAndText(bool willAccept, string text)
    {
        string teamName = LeagueSystem.Instance.GetTeam(_teamBID).GetTeamName();
        _willAcceptText.text = text.Replace("{{TEAMNAME}}", teamName);

        if (willAccept)
        {
            _confirmTradeButton.enabled = true;
        }
        else
        {
            _confirmTradeButton.enabled = false;
        }
    }

    private void UpdateBothTeamsAssets()
    {
        OnAssetsUpdated?.Invoke(0, _teamAID, _teamATradingAssets);
        OnAssetsUpdated?.Invoke(1, _teamBID, _teamBTradingAssets);
    }
}
