using System;
using System.Collections.Generic;
using UnityEngine;

public enum DevelopmentPointsSource
{
    Unknown,
    MatchWin,
    MatchLoss,
    DailyObjective,
    WeeklyObjective,
    Achievement,
    RewardedAdvertisement,
    InAppPurchase,
    WelcomeGrant,
    ReviewReward,
    FacilityUpgrade,
    CoachingStaff,
    DraftScoutingReport,
    GameplayPurchase,
    Refund,
    Migration,
    Administrative
}

[Serializable]
public sealed class DevelopmentPointsTransaction
{
    [SerializeField] string _transactionId;
    [SerializeField] DevelopmentPointsSource _source;
    [SerializeField] int _amountChanged;
    [SerializeField] int _balanceAfter;
    [SerializeField] long _utcTicks;
    [SerializeField] string _description;

    public string TransactionId => _transactionId;
    public DevelopmentPointsSource Source => _source;
    public int AmountChanged => _amountChanged;
    public int BalanceAfter => _balanceAfter;
    public DateTime TimestampUtc => new DateTime(_utcTicks, DateTimeKind.Utc);
    public string Description => _description;

    public DevelopmentPointsTransaction(string transactionId, DevelopmentPointsSource source, int amountChanged, int balanceAfter, string description)
    {
        _transactionId = transactionId;
        _source = source;
        _amountChanged = amountChanged;
        _balanceAfter = balanceAfter;
        _utcTicks = DateTime.UtcNow.Ticks;
        _description = description ?? string.Empty;
    }
}

[Serializable]
public sealed class DevelopmentPointsSaveData
{
    public int Balance;
    public string ScopeId;
    public List<DevelopmentPointsTransaction> Transactions = new List<DevelopmentPointsTransaction>();
    public List<string> ProcessedTransactionIds = new List<string>();
}

public readonly struct DevelopmentPointsChange
{
    public int PreviousBalance { get; }
    public int NewBalance { get; }
    public int AmountChanged { get; }
    public DevelopmentPointsSource Source { get; }
    public string TransactionId { get; }

    public DevelopmentPointsChange(int previousBalance, int newBalance, int amountChanged, DevelopmentPointsSource source, string transactionId)
    {
        PreviousBalance = previousBalance;
        NewBalance = newBalance;
        AmountChanged = amountChanged;
        Source = source;
        TransactionId = transactionId;
    }
}

/// <summary>
/// The only authority allowed to mutate Development Points. All grants and spends require a stable,
/// globally unique transaction ID so retries are safe and traceable.
/// </summary>
public class DevelopmentPointsManager : MonoBehaviour
{
    const int DefaultWinReward = 40;
    const int DefaultLossReward = 10;
    const float DefaultPremiumMultiplier = 1.5f;
    const int DefaultMaximumHistory = 500;

    [SerializeField] DevelopmentPointsSettings _settings;
    [SerializeField, Min(0)] int _startingBalance;

    readonly object _syncRoot = new object();
    readonly List<DevelopmentPointsTransaction> _transactions = new List<DevelopmentPointsTransaction>();
    readonly HashSet<string> _processedTransactionIds = new HashSet<string>(StringComparer.Ordinal);

    int _balance;
    string _scopeId;
    string _careerId;

    public static DevelopmentPointsManager Instance { get; private set; }
    public static event Action<DevelopmentPointsChange> OnDevelopmentPointsChanged;
    public static event Action<DevelopmentPointsChange> OnDevelopmentPointsEarned;
    public static event Action<DevelopmentPointsChange> OnDevelopmentPointsSpent;
    public static event Action<DevelopmentPointsSaveData> OnDevelopmentPointsSaveRequested;

    public int CurrentBalance
    {
        get
        {
            lock (_syncRoot)
                return _balance;
        }
    }

    public DevelopmentPointsOwnershipScope OwnershipScope => _settings != null ? _settings.OwnershipScope : DevelopmentPointsOwnershipScope.AccountWide;
    public DevelopmentPointsSettings Settings => _settings;
    public string ScopeId => _scopeId;
    public string CareerId => _careerId;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureInstance()
    {
        if (Instance != null)
            return;

        DevelopmentPointsManager existing = FindFirstObjectByType<DevelopmentPointsManager>();
        if (existing != null)
            return;

        new GameObject(nameof(DevelopmentPointsManager)).AddComponent<DevelopmentPointsManager>();
    }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (_settings == null)
            _settings = Resources.Load<DevelopmentPointsSettings>(nameof(DevelopmentPointsSettings));
        _balance = Mathf.Max(0, _startingBalance);
        _scopeId = Guid.NewGuid().ToString("N");
        _careerId = Guid.NewGuid().ToString("N");
    }

    public void StartNewCareerIdentity()
    {
        _careerId = Guid.NewGuid().ToString("N");
    }

    public void RestoreCareerIdentity(string careerId)
    {
        _careerId = string.IsNullOrWhiteSpace(careerId) ? Guid.NewGuid().ToString("N") : careerId;
    }

    protected virtual void OnEnable()
    {
        Match.OnMatchPlayed += RewardCompletedMatch;
    }

    protected virtual void OnDisable()
    {
        Match.OnMatchPlayed -= RewardCompletedMatch;
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool CanAfford(int cost)
    {
        return cost >= 0 && CurrentBalance >= cost;
    }

    public bool HasProcessed(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            return false;

        lock (_syncRoot)
            return _processedTransactionIds.Contains(transactionId);
    }

    public bool Award(int amount, DevelopmentPointsSource source, string transactionId, string description = null)
    {
        if (amount <= 0 || source == DevelopmentPointsSource.Unknown)
            return false;

        return TryApply(amount, source, transactionId, description);
    }

    public bool Spend(int amount, DevelopmentPointsSource source, string transactionId, string description = null)
    {
        if (amount <= 0 || source == DevelopmentPointsSource.Unknown)
            return false;

        return TryApply(-amount, source, transactionId, description);
    }

    public bool TryGetTransaction(string transactionId, out DevelopmentPointsTransaction transaction)
    {
        lock (_syncRoot)
        {
            for (int i = _transactions.Count - 1; i >= 0; i--)
            {
                if (string.Equals(_transactions[i].TransactionId, transactionId, StringComparison.Ordinal))
                {
                    transaction = _transactions[i];
                    return true;
                }
            }
        }

        transaction = null;
        return false;
    }

    public IReadOnlyList<DevelopmentPointsTransaction> GetTransactionHistory()
    {
        lock (_syncRoot)
            return _transactions.ToArray();
    }

    public int CalculateMatchReward(bool victory, bool ownsFranchisePremium)
    {
        int baseReward = victory
            ? (_settings != null ? _settings.WinReward : DefaultWinReward)
            : (_settings != null ? _settings.LossReward : DefaultLossReward);

        if (!ownsFranchisePremium)
            return Mathf.Max(0, baseReward);

        float multiplier = _settings != null ? _settings.FranchisePremiumMatchMultiplier : DefaultPremiumMultiplier;
        double multipliedReward = Math.Round(baseReward * (double)multiplier, MidpointRounding.AwayFromZero);
        return multipliedReward > int.MaxValue ? int.MaxValue : Mathf.Max(0, (int)multipliedReward);
    }

    public int PreviewCost(int configuredCost)
    {
        return Mathf.Max(0, configuredCost);
    }

    public int PreviewReward(int configuredReward, DevelopmentPointsSource source)
    {
        if (source == DevelopmentPointsSource.MatchWin)
            return CalculateMatchReward(true, GameManager.Instance != null && GameManager.Instance.GetPremiumStatus());
        if (source == DevelopmentPointsSource.MatchLoss)
            return CalculateMatchReward(false, GameManager.Instance != null && GameManager.Instance.GetPremiumStatus());
        return Mathf.Max(0, configuredReward);
    }

    public int GetProductGrant(string productId)
    {
        return _settings != null
            ? _settings.GetProductGrant(productId)
            : DevelopmentPointsSettings.GetDefaultProductGrant(productId);
    }

    public DevelopmentPointsSaveData CaptureSaveData()
    {
        lock (_syncRoot)
        {
            return new DevelopmentPointsSaveData
            {
                Balance = _balance,
                ScopeId = _scopeId,
                Transactions = new List<DevelopmentPointsTransaction>(_transactions),
                ProcessedTransactionIds = new List<string>(_processedTransactionIds)
            };
        }
    }

    public void RestoreSaveData(DevelopmentPointsSaveData data, int legacyBalance = 0)
    {
        lock (_syncRoot)
        {
            _transactions.Clear();
            _processedTransactionIds.Clear();

            if (data == null)
            {
                _balance = Mathf.Max(0, legacyBalance);
                _scopeId = Guid.NewGuid().ToString("N");
                return;
            }

            _balance = Mathf.Max(0, data.Balance);
            _scopeId = string.IsNullOrWhiteSpace(data.ScopeId) ? Guid.NewGuid().ToString("N") : data.ScopeId;

            if (data.Transactions != null)
                _transactions.AddRange(data.Transactions);
            if (data.ProcessedTransactionIds != null)
            {
                for (int i = 0; i < data.ProcessedTransactionIds.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(data.ProcessedTransactionIds[i]))
                        _processedTransactionIds.Add(data.ProcessedTransactionIds[i]);
                }
            }

            for (int i = 0; i < _transactions.Count; i++)
                _processedTransactionIds.Add(_transactions[i].TransactionId);
            TrimHistory();
        }
    }

    public static string CreateTransactionId(string sourceKey)
    {
        string prefix = string.IsNullOrWhiteSpace(sourceKey) ? "dp" : sourceKey.Trim();
        return prefix + ":" + Guid.NewGuid().ToString("N");
    }

    bool TryApply(int signedAmount, DevelopmentPointsSource source, string transactionId, string description)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            return false;

        DevelopmentPointsChange change;
        DevelopmentPointsSaveData saveData;

        lock (_syncRoot)
        {
            if (_processedTransactionIds.Contains(transactionId))
                return false;

            long candidateBalance = (long)_balance + signedAmount;
            if (candidateBalance < 0 || candidateBalance > int.MaxValue)
                return false;

            int previousBalance = _balance;
            _balance = (int)candidateBalance;
            _processedTransactionIds.Add(transactionId);
            _transactions.Add(new DevelopmentPointsTransaction(transactionId, source, signedAmount, _balance, description));
            TrimHistory();
            change = new DevelopmentPointsChange(previousBalance, _balance, signedAmount, source, transactionId);
            saveData = CaptureSaveData();
        }

        OnDevelopmentPointsChanged?.Invoke(change);
        if (signedAmount > 0)
            OnDevelopmentPointsEarned?.Invoke(change);
        else
            OnDevelopmentPointsSpent?.Invoke(change);
        OnDevelopmentPointsSaveRequested?.Invoke(saveData);
        return true;
    }

    void RewardCompletedMatch(Match match)
    {
        if (match == null || GameManager.Instance == null || !match.IsMyTeamMatch(GameManager.Instance.GetTeamID()))
            return;

        int teamId = GameManager.Instance.GetTeamID();
        (int wins, int losses) = match.GetWinStatForTeam(teamId);
        bool victory = wins > losses;
        int amount = CalculateMatchReward(victory, GameManager.Instance.GetPremiumStatus());
        string transactionId = string.Concat("match:", _careerId, ":", GameManager.Instance.GetCurrentSeason(), ":", match.GetMatchID());
        Award(amount, victory ? DevelopmentPointsSource.MatchWin : DevelopmentPointsSource.MatchLoss, transactionId, victory ? "Match win" : "Match loss");
    }

    void TrimHistory()
    {
        int maximumHistory = _settings != null ? _settings.MaximumTransactionHistory : DefaultMaximumHistory;
        int excess = _transactions.Count - Mathf.Max(10, maximumHistory);
        if (excess > 0)
            _transactions.RemoveRange(0, excess);
    }
}
