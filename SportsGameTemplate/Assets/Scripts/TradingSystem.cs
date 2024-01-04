using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Analytics;

public class TradingSystem : MonoBehaviour
{
    [InfoBox("@GetTotalTradeValue(_teamATradingAssets)")]
    [SerializeField] int _teamAID = 0;
    [SerializeReference] List<ITradeable> _teamATradingAssets;
    [InfoBox("@GetTotalTradeValue(_teamBTradingAssets)")]
    [SerializeField] int _teamBID = 10;
    [SerializeReference] List<ITradeable> _teamBTradingAssets;

    [Header("Trade confirmation UI")]
    [SerializeField] bool _willAlwaysAccept;
    [SerializeField] string _tradeValueTooLowString;
    [SerializeField] string _overSalaryCapString;
    [SerializeField] string _willAcceptString;
    [SerializeField] TextMeshProUGUI _willAcceptText;
    [SerializeField] Button _confirmTradeButton;

    public static event Action<int, int, List<ITradeable>, bool> OnAssetsUpdated;
    public static event Action OnTradeCompleted;

    private void Awake()
    {
        Player.OnAddedToTrade += AddAssetToTrade;
        DraftPick.OnAddedToTrade += AddAssetToTrade;
        TeamAsset.OnRemoveFromTrade += RemoveFromTrade;
        TradeOfferItem.OnNewTradeOpened += ClearTrades;
        GameManager.OnAdvance += GenerateTradeForPlayer;
        TradeOfferItem.OnTradeOfferOpened += SetTradeWillingnessToTrue;
        OnAssetsUpdated += CheckTradeWillingness;
    }

    public void ConfirmTrade()
    {
        FirebaseAnalytics.LogEvent("trade_confirmed", new Parameter("value_sent", GetTotalTradeValue(_teamATradingAssets)), new Parameter("value_received", GetTotalTradeValue(_teamBTradingAssets)));

        TradeAssets(_teamAID, _teamBID, _teamATradingAssets);
        TradeAssets(_teamBID, _teamAID, _teamBTradingAssets);

        ClearTrades(false);
        UpdateAcceptButtonAndText(false, "");
        OnTradeCompleted?.Invoke();

        Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()));
        Navigation.Instance.GoToScreen(true, CanvasKey.Team, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()));
    }

    private void SetTradeWillingnessToTrue()
    {
        _willAlwaysAccept = true;
        CheckTradeWillingness(1, _teamBID, _teamBTradingAssets, true);
    }

    private void GenerateTradeForPlayer(SeasonStage seasonStage, int week)
    {
        int randomAmount = UnityEngine.Random.Range(1, 3);

        if (LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()).GetAllTradeOffers().Count >= 25) return;

        for (int i = 0; i < randomAmount; i++)
        {
            int teamOne = GameManager.Instance.GetTeamID();

            int teamTwo = UnityEngine.Random.Range(0, LeagueSystem.Instance.GetTeams().Count - 1);

            AITrader aiTrader = new AITrader();
            aiTrader.GenerateOffer(LeagueSystem.Instance.GetTeam(teamOne).GetTradeAssets(), teamTwo, UnityEngine.Random.Range(1000, 2500), LeagueSystem.Instance.GetTeam(teamTwo).GetTradeAssets());
        }
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
            asset.RemoveTradeOffers();
        }
    }

    public void ClearTrades(bool toTradeScreen)
    {
        _teamAID = GameManager.Instance.GetTeamID();
        _teamATradingAssets = new List<ITradeable>();
        _teamBTradingAssets = new List<ITradeable>();
        _willAlwaysAccept = false;
        UpdateBothTeamsAssets(toTradeScreen);
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

    public void StartNewTrade()
    {
        _teamAID = GameManager.Instance.GetTeamID();
        Navigation.Instance.GoToScreen(true, CanvasKey.TeamOverview, LeagueSystem.Instance.GetTeamsSortedByID());
    }

    public void DebugCounterOffer()
    {
        _teamBTradingAssets = GetOffer(1);
    }

    public void AddAssetToTrade(int team, ITradeable assetToAdd)
    {
        _teamAID = GameManager.Instance.GetTeamID();

        if (team == GameManager.Instance.GetTeamID())
        {
            if (_teamATradingAssets.Contains(assetToAdd)) return;

            _teamATradingAssets.Add(assetToAdd);
            _willAlwaysAccept = false;
        }
        else
        {
            if (_teamBTradingAssets.Contains(assetToAdd)) return;

            if (_teamBID != team)
            {
                _teamBID = team;
                _teamBTradingAssets = new List<ITradeable>();
                _teamBTradingAssets.Add(assetToAdd);
            }
            else
            {
                _teamBTradingAssets.Add(assetToAdd);
                _willAlwaysAccept = false;
            }
        }

        UpdateBothTeamsAssets(true);
    }

    private void RemoveFromTrade(int teamID, ITradeable asset)
    {
        if (teamID == 0)
        {
            _willAlwaysAccept = false;
            _teamATradingAssets.Remove(asset);
            UpdateBothTeamsAssets(true);
        }
        else if (teamID == 1)
        {
            _teamBTradingAssets.Remove(asset);
            UpdateBothTeamsAssets(true);

            if (_teamBTradingAssets.Count == 0)
            {
                _willAlwaysAccept = false;
                OnTradeCompleted?.Invoke();
                UpdateAcceptButtonAndText(false, "");
            }
        }
    }

    public void GenerateRandomAITrade()
    {
        _teamAID = GameManager.Instance.GetTeamID();
        _teamATradingAssets = new List<ITradeable>() { LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()).GetPlayersFromTeam()[0], LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()).GetDraftPicks()[0] };

        _teamBID = UnityEngine.Random.Range(0, LeagueSystem.Instance.GetTeams().Count - 1);
        _teamBTradingAssets = GetTradeOffer(_teamBID).GetAssets().Item2;

        UpdateBothTeamsAssets(true);
    }

    private TradeOffer GetTradeOffer(int teamID)
    {
        AITrader aiTrader = new AITrader();
        return aiTrader.GenerateOffer(_teamATradingAssets, teamID, GetTotalTradeValue(_teamATradingAssets), LeagueSystem.Instance.GetTeam(_teamBID).GetTradeAssets());
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

    public void CheckTradeWillingness(int teamIndex, int teamID, List<ITradeable> assets, bool reloadScreen)
    {
        if (_teamBID == -1) return;

        if (_willAlwaysAccept)
        {
            Debug.Log("Will accept!");
            UpdateAcceptButtonAndText(true, _willAcceptString);
            return;
        }

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
            _confirmTradeButton.ToggleButtonStatus(true);
        }
        else
        {
            _confirmTradeButton.ToggleButtonStatus(false);
        }

        if (GameManager.Instance.GetGems() >= 1)
        {
            _confirmTradeButton.onClick.RemoveAllListeners();
            _confirmTradeButton.onClick.AddListener(() => ConfirmTrade());
            _confirmTradeButton.onClick.AddListener(() => GameManager.Instance.AddToGems(-1));
        }
        else
        {
            _confirmTradeButton.onClick.RemoveAllListeners();
            _confirmTradeButton.onClick.AddListener(() => TransitionAnimation.Instance.StartTransition(() => Navigation.Instance.GoToScreen(true, CanvasKey.Store)));
        }
    }

    private void UpdateBothTeamsAssets(bool reloadScreen)
    {
        OnAssetsUpdated?.Invoke(0, _teamAID, _teamATradingAssets, reloadScreen);
        OnAssetsUpdated?.Invoke(1, _teamBID, _teamBTradingAssets, reloadScreen);
    }
}
