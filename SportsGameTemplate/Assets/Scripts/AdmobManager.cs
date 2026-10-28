using System;
using CAS;
using UnityEngine;
using UnityEngine.Serialization;

public class AdmobManager : MonoBehaviour
{
    public static AdmobManager Instance { get; private set; }

    [Header("CAS Settings")]
    [SerializeField, Min(0)] int _managerIdIndex;
    [SerializeField] bool _initializeOnAwake = true;
    [SerializeField] bool _loadAdsOnInitialize = true;
    [SerializeField] bool _respectProStatus = true;

    [Header("Interstitial Timing")]
    [SerializeField] bool _adReady;
    [FormerlySerializedAs("_secondsBetweenInterstitialAds")]
    [SerializeField] float _secondsBetweenAds = 120f;

    [Header("Ad Free")]
    [SerializeField] bool _isAdFree;
    [SerializeField] DateTime _adFreeUntil;
    [SerializeField] string _adFreeUntilKey = "ad_free_time_key";
    [SerializeField] string _adFreeStartKey = "ad_free_start_key";

    [Header("Session Ad Grace")]
    [SerializeField, Min(0f)] float _firstSessionAdFreeMinutes = 10f;
    [SerializeField, Min(0f)] float _returningSessionFallbackAdFreeMinutes = 2f;

    IMediationManager _manager;
    float _secondsLeft;
    int _adsShown;
    bool _isInitialized;
    bool _isInitializing;
    float _sessionElapsedSeconds;
    float _sessionAdFreeDurationSeconds;
    Action _pendingRewardCallback;
    string _pendingRewardType;

    public event Action OnInterstitialClosed;
    public event Action OnRewardedClosed;
    public event Action<int> OnRewardEarned;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        _secondsLeft = _secondsBetweenAds;

        DateTime.TryParse(
            PlayerPrefs.GetString(_adFreeUntilKey),
            out _adFreeUntil);

        bool isFirstSession =
            PlayerPrefs.GetInt(_adFreeStartKey, 0) == 0;

        if (isFirstSession)
        {
            PlayerPrefs.SetInt(_adFreeStartKey, 1);
            PlayerPrefs.Save();
            _sessionAdFreeDurationSeconds =
                _firstSessionAdFreeMinutes * 60f;
        }
        else
        {
            _sessionAdFreeDurationSeconds =
                _returningSessionFallbackAdFreeMinutes * 60f;
        }

        GameManager.OnAdvance += CheckShowInterstitialAd;

        if (_initializeOnAwake)
        {
            Initialize();
        }
    }

    void Update()
    {
        _sessionElapsedSeconds += Time.unscaledDeltaTime;

        if (ShouldSkipAds())
        {
            _adReady = false;
            return;
        }

        if (_secondsLeft > 0f)
        {
            _secondsLeft -= Time.deltaTime;
        }

        _adReady = _secondsLeft <= 0f;
    }

    void OnDestroy()
    {
        GameManager.OnAdvance -= CheckShowInterstitialAd;
        UnregisterAdEvents();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void InitializeAds()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (_isInitialized || _isInitializing)
        {
            return;
        }

        _isInitializing = true;

        try
        {
            _manager = MobileAds.BuildManager()
                .WithManagerIdAtIndex(_managerIdIndex)
                .WithCompletionListener(OnManagerInitialized)
                .Build();
        }
        catch (Exception exception)
        {
            _isInitializing = false;
            Debug.LogException(exception);
        }
    }

    public void LoadInterstitialAd()
    {
        if (!_isInitialized)
        {
            Initialize();
            return;
        }

        _manager.LoadAd(AdType.Interstitial);
    }

    public bool IsInterstitialReady()
    {
        return _isInitialized &&
            _manager != null &&
            _manager.IsReadyAd(AdType.Interstitial);
    }

    public bool GetAdReadyStatus()
    {
        return _adReady && IsInterstitialReady();
    }

    public bool GetInterstitialAdReadyStatus()
    {
        return GetAdReadyStatus();
    }

    public void ShowInterstitialAd()
    {
        if (ShouldSkipAds() || IsAdFree())
        {
            return;
        }

        if (!_isInitialized)
        {
            Initialize();
            return;
        }

        if (!IsInterstitialReady())
        {
            Debug.Log("CAS interstitial ad is not ready.");
            LoadInterstitialAd();
            return;
        }

        Debug.Log("Showing CAS interstitial ad.");
        _manager.ShowAd(AdType.Interstitial, "game_advance");

        _secondsLeft = _secondsBetweenAds;
        _adReady = false;
        _adsShown++;
    }

    public void LoadRewardedAd()
    {
        if (!_isInitialized)
        {
            Initialize();
            return;
        }

        _manager.LoadAd(AdType.Rewarded);
    }

    public bool IsRewardedReady()
    {
        return _isInitialized &&
            _manager != null &&
            _manager.IsReadyAd(AdType.Rewarded);
    }

    public bool GetRewardedAdReadyStatus()
    {
        return IsRewardedReady();
    }

    public void ShowRewardedAd(int gemReward)
    {
        ShowRewardedAd(() => GrantReward(gemReward), "gems");
    }

    public bool ShowRewardedAd(Action rewardCallback)
    {
        return ShowRewardedAd(rewardCallback, "custom");
    }

    public bool ShowRewardedAd(
        Action rewardCallback,
        string rewardType)
    {
        if (!_isInitialized)
        {
            Initialize();
            return false;
        }

        if (!IsRewardedReady())
        {
            Debug.Log("CAS rewarded ad is not ready.");
            LoadRewardedAd();
            return false;
        }

        _pendingRewardCallback = rewardCallback;
        _pendingRewardType = rewardType;

        Debug.Log("Showing CAS rewarded ad.");
        _manager.ShowAd(AdType.Rewarded, rewardType);
        return true;
    }

    public void SetAdFreeMinutes(int minutes)
    {
        _isAdFree = true;
        _adFreeUntil = DateTime.Now.AddMinutes(minutes);

        PlayerPrefs.SetString(
            _adFreeUntilKey,
            _adFreeUntil.ToString());
        PlayerPrefs.Save();
    }

    public bool IsAdFree()
    {
        if (_sessionElapsedSeconds < _sessionAdFreeDurationSeconds)
        {
            _isAdFree = true;
            return true;
        }

        if (DateTime.Now < _adFreeUntil)
        {
            _isAdFree = true;
            return true;
        }

        _isAdFree = false;
        return false;
    }

    void CheckShowInterstitialAd(SeasonStage seasonStage, int week)
    {
        if (IsAdFree() || week <= 1 || !GetAdReadyStatus())
        {
            return;
        }

        ShowInterstitialAd();

    }

    void GrantReward(int gemReward)
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.AddToGems(gemReward);
        OnRewardEarned?.Invoke(gemReward);
    }

    int GetReturningSessionAdFreeMinutes()
    {
        return Mathf.Max(
            0,
            Mathf.RoundToInt(
                _returningSessionFallbackAdFreeMinutes));
    }

    void OnManagerInitialized(InitialConfiguration configuration)
    {
        _isInitializing = false;

        if (configuration == null ||
            !string.IsNullOrEmpty(configuration.error) ||
            configuration.manager == null)
        {
            _isInitialized = false;
            Debug.LogError(
                "CAS initialization failed: " +
                (configuration != null
                    ? configuration.error
                    : "No configuration returned."));
            return;
        }

        if (_manager != null && _manager != configuration.manager)
        {
            UnregisterAdEvents();
        }

        _manager = configuration.manager;
        _isInitialized = true;
        RegisterAdEvents();

        Debug.Log("CAS initialization complete.");

        if (_loadAdsOnInitialize)
        {
            LoadInterstitialAd();
            LoadRewardedAd();
        }
    }

    void RegisterAdEvents()
    {
        if (_manager == null)
        {
            return;
        }

        _manager.OnInterstitialAdClosed += HandleInterstitialClosed;
        _manager.OnInterstitialAdFailedToShow +=
            HandleInterstitialFailedToShow;
        _manager.OnInterstitialAdImpression +=
            HandleInterstitialImpression;
        _manager.OnRewardedAdCompleted += HandleRewardedCompleted;
        _manager.OnRewardedAdClosed += HandleRewardedClosed;
        _manager.OnRewardedAdFailedToShow +=
            HandleRewardedFailedToShow;
        _manager.OnRewardedAdImpression += HandleRewardedImpression;
    }

    void UnregisterAdEvents()
    {
        if (_manager == null)
        {
            return;
        }

        _manager.OnInterstitialAdClosed -= HandleInterstitialClosed;
        _manager.OnInterstitialAdFailedToShow -=
            HandleInterstitialFailedToShow;
        _manager.OnInterstitialAdImpression -=
            HandleInterstitialImpression;
        _manager.OnRewardedAdCompleted -= HandleRewardedCompleted;
        _manager.OnRewardedAdClosed -= HandleRewardedClosed;
        _manager.OnRewardedAdFailedToShow -=
            HandleRewardedFailedToShow;
        _manager.OnRewardedAdImpression -= HandleRewardedImpression;
    }

    void HandleInterstitialClosed()
    {
        Debug.Log("CAS interstitial ad closed.");
        OnInterstitialClosed?.Invoke();
        LoadInterstitialAd();
    }

    void HandleInterstitialFailedToShow(string error)
    {
        Debug.LogError(
            "CAS interstitial ad failed to show: " + error);
        LoadInterstitialAd();
    }

    void HandleRewardedCompleted()
    {
        Action rewardCallback = _pendingRewardCallback;
        string rewardType = _pendingRewardType;
        _pendingRewardCallback = null;
        _pendingRewardType = null;

        rewardCallback?.Invoke();
        SetAdFreeMinutes(GetReturningSessionAdFreeMinutes());

        Firebase.Analytics.Parameter[] parameters =
        {
            new Firebase.Analytics.Parameter(
                "reward_type",
                rewardType ?? "custom")
        };

        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            "cas_rewarded_reward_earned",
            parameters);
    }

    void HandleRewardedClosed()
    {
        Debug.Log("CAS rewarded ad closed.");
        _pendingRewardCallback = null;
        _pendingRewardType = null;
        OnRewardedClosed?.Invoke();
        LoadRewardedAd();
    }

    void HandleRewardedFailedToShow(string error)
    {
        Debug.LogError(
            "CAS rewarded ad failed to show: " + error);
        _pendingRewardCallback = null;
        _pendingRewardType = null;
        LoadRewardedAd();
    }

    void HandleInterstitialImpression(AdMetaData metadata)
    {
        LogAdRevenue("interstitial", metadata);
    }

    void HandleRewardedImpression(AdMetaData metadata)
    {
        LogAdRevenue("rewarded", metadata);
    }

    void LogAdRevenue(string adFormat, AdMetaData metadata)
    {
        if (metadata == null)
        {
            return;
        }

        string mediationAdapter = metadata.network.ToString();
        double revenueValue = metadata.revenue;

        Debug.Log(
            $"CAS paid event - Format: {adFormat}, Value: {revenueValue}, Currency: USD, Adapter: {mediationAdapter}");

        Firebase.Analytics.Parameter[] parameters =
        {
            new Firebase.Analytics.Parameter("ad_platform", "CAS"),
            new Firebase.Analytics.Parameter(
                "ad_source",
                mediationAdapter),
            new Firebase.Analytics.Parameter("ad_format", adFormat),
            new Firebase.Analytics.Parameter("currency", "USD"),
            new Firebase.Analytics.Parameter("value", revenueValue)
        };

        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            "ad_impression",
            parameters);
    }

    bool ShouldSkipAds()
    {
        return _respectProStatus &&
            GameManager.Instance != null &&
            GameManager.Instance.GetPremiumStatus();
    }
}