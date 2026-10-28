using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Analytics;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradingSystem : MonoBehaviour
{
    [SerializeField] int _teamAID = 0;
    [SerializeReference] List<ITradeable> _teamATradingAssets;
    [SerializeField] int _teamBID = 10;
    [SerializeReference] List<ITradeable> _teamBTradingAssets;

    [Header("Trade confirmation UI")]
    [SerializeField] bool _willAlwaysAccept;
    [SerializeField] string _tradeValueTooLowString;
    [SerializeField] string _overSalaryCapString;
    [SerializeField] string _willAcceptString;
    [SerializeField] TextMeshProUGUI _willAcceptText;
    [SerializeField] Image _tradeWillingnessBar;
    [SerializeField] Button _confirmTradeButton;

    public static event Action<int, int, List<ITradeable>, bool> OnAssetsUpdated;
    public static event Action OnTradeCompleted;
    public static event Action<IReadOnlyCollection<string>> OnTradeAccepted;

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
        Team teamA = LeagueSystem.Instance.GetTeam(_teamAID);
        Team teamB = LeagueSystem.Instance.GetTeam(_teamBID);
        ValidationResult validation = new Validator(LeagueSystem.Instance.GetTeams())
            .Validate(teamA, _teamATradingAssets, teamB, _teamBTradingAssets, !_willAlwaysAccept);
        if (!validation.IsValid)
        {
            Notification.Instance.ShowNotification(validation.Reason, NotificationType.Warning, 3);
            CheckTradeWillingness(1, _teamBID, _teamBTradingAssets, false);
            return;
        }

        FirebaseAnalytics.LogEvent("trade_confirmed", new Parameter("value_sent", GetTotalTradeValue(_teamATradingAssets)), new Parameter("value_received", GetTotalTradeValue(_teamBTradingAssets)));

        HashSet<string> acceptedAssetIDs = new HashSet<string>();
        AddTradeableIDs(_teamATradingAssets, acceptedAssetIDs);
        AddTradeableIDs(_teamBTradingAssets, acceptedAssetIDs);

        TradeAssets(_teamAID, _teamBID, _teamATradingAssets);
        TradeAssets(_teamBID, _teamAID, _teamBTradingAssets);
        teamA.GenerateLineup();
        teamB.GenerateLineup();

        ClearTrades(false);
        UpdateAcceptButtonAndText(false, "");
        OnTradeAccepted?.Invoke(acceptedAssetIDs);
        OnTradeCompleted?.Invoke();
        LocalSaveManager localSaveManager = FindFirstObjectByType<LocalSaveManager>();
        if (localSaveManager != null)
            localSaveManager.SaveGame(false);

        Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()));
        Navigation.Instance.GoToScreen(true, CanvasKey.Team, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()));

        Notification.Instance.ShowNotification("Trade confirmed!", NotificationType.Success, 2);
    }

    private void SetTradeWillingnessToTrue()
    {
        _willAlwaysAccept = true;
        CheckTradeWillingness(1, _teamBID, _teamBTradingAssets, true);
    }

    public bool OpenIncomingTradeOffer(TradeOffer tradeOffer)
    {
        if (tradeOffer == null)
            return false;

        (List<ITradeable> requestedAssets, List<ITradeable> offeredAssets) = tradeOffer.GetAssets();
        if (requestedAssets.Count == 0 || offeredAssets.Count == 0)
            return false;

        _teamAID = GameManager.Instance.GetTeamID();
        _teamBID = tradeOffer.GetOfferingTeamID();
        _teamATradingAssets = new List<ITradeable>(requestedAssets);
        _teamBTradingAssets = new List<ITradeable>(offeredAssets);
        _willAlwaysAccept = true;
        UpdateBothTeamsAssets(false);
        CheckTradeWillingness(1, _teamBID, _teamBTradingAssets, true);
        Navigation.Instance.GoToScreen(true, CanvasKey.Trade);
        return true;
    }

    static void AddTradeableIDs(List<ITradeable> assets, HashSet<string> assetIDs)
    {
        if (assets == null)
            return;

        for (int i = 0; i < assets.Count; i++)
        {
            if (assets[i] != null)
                assetIDs.Add(assets[i].GetTradeableID());
        }
    }

    private void GenerateTradeForPlayer(SeasonStage seasonStage, int week)
    {
        int randomAmount = UnityEngine.Random.Range(1, 3);

        if (LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()).GetAllTradeOffers().Count >= 25) return;

        for (int i = 0; i < randomAmount; i++)
        {
            int teamOne = GameManager.Instance.GetTeamID();
            List<Team> possibleTradePartners = LeagueSystem.Instance.GetTeams().FindAll(team => team.GetTeamID() != teamOne);
            if (possibleTradePartners.Count == 0) return;
            int teamTwo = possibleTradePartners[UnityEngine.Random.Range(0, possibleTradePartners.Count)].GetTeamID();

            AITrader aiTrader = new AITrader();
            Team userTeam = LeagueSystem.Instance.GetTeam(teamOne);
            List<ITradeable> userAssets = userTeam.GetTradeAssets();
            if (userAssets.Count == 0) return;

            ITradeable target = userAssets[UnityEngine.Random.Range(0, userAssets.Count)];
            Team offeringTeam = LeagueSystem.Instance.GetTeam(teamTwo);
            TradeValueService valuation = new TradeValueService(LeagueSystem.Instance.GetTeams());
            aiTrader.GenerateOffer(userAssets, teamTwo, valuation.GetValueForTeam(target, offeringTeam), offeringTeam.GetTradeAssets());
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
            }
            else if (asset.GetType() == typeof(DraftPick))
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

        List<Team> possibleTradePartners = LeagueSystem.Instance.GetTeams().FindAll(team => team.GetTeamID() != _teamAID);
        if (possibleTradePartners.Count == 0) return;
        _teamBID = possibleTradePartners[UnityEngine.Random.Range(0, possibleTradePartners.Count)].GetTeamID();
        _teamBTradingAssets = GetTradeOffer(_teamBID).GetAssets().Item2;

        UpdateBothTeamsAssets(true);
    }

    private TradeOffer GetTradeOffer(int teamID)
    {
        AITrader aiTrader = new AITrader();
        return aiTrader.GenerateOffer(_teamATradingAssets, teamID, GetTotalTradeValueForTeam(_teamATradingAssets, teamID), LeagueSystem.Instance.GetTeam(_teamBID).GetTradeAssets());
    }

    public int GetTotalTradeValue(List<ITradeable> assets)
    {
        TradeValueService valuation = new TradeValueService(LeagueSystem.Instance.GetTeams());
        return valuation.GetPackageMarketValue(assets);
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
            ValidationResult offeredTradeValidation = new Validator(LeagueSystem.Instance.GetTeams())
                .Validate(LeagueSystem.Instance.GetTeam(_teamAID), _teamATradingAssets, LeagueSystem.Instance.GetTeam(_teamBID), _teamBTradingAssets, false);
            if (offeredTradeValidation.IsValid)
            {
                UpdateAcceptButtonAndText(true, _willAcceptString);
                _tradeWillingnessBar.fillAmount = 1;
                return;
            }

            _willAlwaysAccept = false;
            UpdateAcceptButtonAndText(false, offeredTradeValidation.Reason);
            return;
        }

        ValidationResult validation = new Validator(LeagueSystem.Instance.GetTeams())
            .Validate(LeagueSystem.Instance.GetTeam(_teamAID), _teamATradingAssets, LeagueSystem.Instance.GetTeam(_teamBID), _teamBTradingAssets);
        if (!validation.IsValid)
        {
            int valueReceivedByB = GetTotalTradeValueForTeam(_teamATradingAssets, _teamBID);
            int valueSentByB = GetTotalTradeValueForTeam(_teamBTradingAssets, _teamBID);
            _tradeWillingnessBar.fillAmount = valueReceivedByB / (float)Mathf.Max(1, valueSentByB);
            UpdateAcceptButtonAndText(false, validation.Reason);
            return;
        }
        else
        {
            _tradeWillingnessBar.fillAmount = 1;
            Debug.Log("Will accept!");
            UpdateAcceptButtonAndText(true, _willAcceptString);
            return;
        }
    }

    private int GetTotalTradeValueForTeam(IEnumerable<ITradeable> assets, int receivingTeamID)
    {
        TradeValueService valuation = new TradeValueService(LeagueSystem.Instance.GetTeams());
        return valuation.GetPackageValueForTeam(assets, LeagueSystem.Instance.GetTeam(receivingTeamID));
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

        if (DevelopmentPointsManager.Instance.CanAfford(1))
        {
            _confirmTradeButton.onClick.RemoveAllListeners();
            _confirmTradeButton.onClick.AddListener(() =>
            {
                if (DevelopmentPointsManager.Instance.Spend(1, DevelopmentPointsSource.GameplayPurchase, DevelopmentPointsManager.CreateTransactionId("trade"), "Confirm trade"))
                    ConfirmTrade();
            });
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

    public class ValidationResult
    {
        public bool IsValid { get; }
        public string Reason { get; }

        private ValidationResult(bool isValid, string reason)
        {
            IsValid = isValid;
            Reason = reason;
        }

        public static ValidationResult Valid()
        {
            return new ValidationResult(true, string.Empty);
        }

        public static ValidationResult Invalid(string reason)
        {
            return new ValidationResult(false, reason);
        }
    }

    public class Validator
    {
        private readonly TradeValueService _valuation;

        public Validator(IReadOnlyList<Team> teams)
        {
            _valuation = new TradeValueService(teams);
        }

        public ValidationResult Validate(Team teamA, List<ITradeable> assetsFromA, Team teamB, List<ITradeable> assetsFromB, bool validateValue = true)
        {
            if (teamA == null || teamB == null || teamA == teamB)
                return ValidationResult.Invalid("A trade requires two different teams.");
            if (assetsFromA == null || assetsFromB == null || assetsFromA.Count == 0 || assetsFromB.Count == 0)
                return ValidationResult.Invalid("Both teams must include at least one asset.");
            if (assetsFromA.Any(asset => asset == null) || assetsFromB.Any(asset => asset == null))
                return ValidationResult.Invalid("One or more assets are no longer available.");
            if (HasDuplicates(assetsFromA) || HasDuplicates(assetsFromB))
                return ValidationResult.Invalid("The same asset cannot be included more than once.");
            if (!OwnsAllAssets(teamA, assetsFromA) || !OwnsAllAssets(teamB, assetsFromB))
                return ValidationResult.Invalid("One or more assets are no longer owned by the offering team.");
            if (!HasLegalRoster(teamA, assetsFromA, assetsFromB) || !HasLegalRoster(teamB, assetsFromB, assetsFromA))
                return ValidationResult.Invalid("The trade would leave a team without a legal roster.");
            if (!FitsSalaryCap(teamA, assetsFromA, assetsFromB) || !FitsSalaryCap(teamB, assetsFromB, assetsFromA))
                return ValidationResult.Invalid("The trade would put a team over the salary cap.");

            if (validateValue)
            {
                int valueReceivedByB = _valuation.GetPackageValueForTeam(assetsFromA, teamB);
                int valueSentByB = _valuation.GetPackageValueForTeam(assetsFromB, teamB);
                if (valueReceivedByB < valueSentByB * _valuation.Context.Settings.MinimumAcceptanceRatio)
                    return ValidationResult.Invalid("The offered value is too low for the other team.");
            }

            return ValidationResult.Valid();
        }

        private static bool HasDuplicates(IEnumerable<ITradeable> assets)
        {
            return assets.GroupBy(asset => asset.GetTradeableID()).Any(group => group.Count() > 1);
        }

        private static bool OwnsAllAssets(Team team, IEnumerable<ITradeable> assets)
        {
            HashSet<string> ownedIDs = new HashSet<string>(team.GetTradeAssets().Select(asset => asset.GetTradeableID()));
            return assets.All(asset => ownedIDs.Contains(asset.GetTradeableID()));
        }

        private static bool HasLegalRoster(Team team, IEnumerable<ITradeable> outgoing, IEnumerable<ITradeable> incoming)
        {
            int outgoingPlayers = outgoing.Count(asset => asset is Player);
            int incomingPlayers = incoming.Count(asset => asset is Player);
            return team.GetPlayersFromTeam().Count - outgoingPlayers + incomingPlayers >= 5;
        }

        private static bool FitsSalaryCap(Team team, IEnumerable<ITradeable> outgoing, IEnumerable<ITradeable> incoming)
        {
            int outgoingSalary = outgoing.OfType<Player>().Sum(player => player.GetContract().GetYearlySalary());
            int incomingSalary = incoming.OfType<Player>().Sum(player => player.GetContract().GetYearlySalary());
            float capIncrease = GameManager.Instance != null && team.GetTeamID() == GameManager.Instance.GetTeamID()
                ? GameManager.Instance.GetSalaryCapIncrease()
                : 0f;
            float effectiveCap = ConfigManager.Instance.GetCurrentConfig().SalaryCap * (1f + capIncrease);
            return team.GetTotalSalaryAmount() - outgoingSalary + incomingSalary <= effectiveCap;
        }
    }
}
