using System;
using Firebase.Analytics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreView : MonoBehaviour, ISettable
{
    [SerializeField] TextMeshProUGUI _balanceText;
    [SerializeField] GameObject _subscriptionObject;
    [SerializeField] Button _reviewForGemsButton;
    [SerializeField] Button _premiumButton;
    [SerializeField] Button _rewardedAdsButton;

    [SerializeField] GameObject _promoObject;
    [SerializeField] TextMeshProUGUI _promoTimerText;

    private void Awake()
    {
        SetDetails(this);
        GameManager.OnStoreItemsUpdated += TogglePremiumObject;
        DevelopmentPointsManager.OnDevelopmentPointsChanged += UpdateBalance;
        RewardedAdsManager.OnRewardedAdWatched += GrantRewardedAdReward;
    }

    void OnDestroy()
    {
        GameManager.OnStoreItemsUpdated -= TogglePremiumObject;
        DevelopmentPointsManager.OnDevelopmentPointsChanged -= UpdateBalance;
        RewardedAdsManager.OnRewardedAdWatched -= GrantRewardedAdReward;
    }

    private void ToggleReviewButton()
    {
        if (PlayerPrefs.GetInt("reviewed") == 1)
        {
            _reviewForGemsButton.ToggleStoreButtonStatus(false);
        }
        else
        {
            _reviewForGemsButton.ToggleStoreButtonStatus(true);
            _reviewForGemsButton.onClick.RemoveAllListeners();
            _reviewForGemsButton.onClick.AddListener(() => DevelopmentPointsManager.Instance.Award(
                DevelopmentPointsManager.Instance.Settings != null ? DevelopmentPointsManager.Instance.Settings.ReviewReward : 20,
                DevelopmentPointsSource.ReviewReward,
                "review:app-store",
                "Store review reward"));
            _reviewForGemsButton.onClick.AddListener(() => PlayerPrefs.SetInt("reviewed", 1));

            _reviewForGemsButton.onClick.AddListener(() => Application.OpenURL("market://details?id=" + Application.productName));

            _reviewForGemsButton.onClick.AddListener(() => ToggleReviewButton());
        }
    }

    private void TogglePremiumObject(int gems, bool premium)
    {
        _premiumButton.ToggleStoreButtonStatus(!GameManager.Instance.GetPremiumStatus());
    }


    private void ToggleSubscriptionObject()
    {
        _subscriptionObject.SetActive(false);
    }

    private void UpdateBalance(DevelopmentPointsChange change)
    {
        _balanceText.text = $"Current Balance: {DevelopmentPointsManager.Instance.CurrentBalance} DP";
        _premiumButton.ToggleStoreButtonStatus(!GameManager.Instance.GetPremiumStatus());
    }

    private void GrantRewardedAdReward(string adCode)
    {
        if (adCode == "gems")
        {
            int reward = DevelopmentPointsManager.Instance.Settings != null ? DevelopmentPointsManager.Instance.Settings.RewardedAdvertisementReward : 5;
            DevelopmentPointsManager.Instance.Award(reward, DevelopmentPointsSource.RewardedAdvertisement, DevelopmentPointsManager.CreateTransactionId("rewarded-ad:" + adCode), "Rewarded advertisement");
            _rewardedAdsButton.ToggleStoreButtonStatus(false);
        }
    }

    public void SetDetails<T>(T item) where T : class
    {
        ToggleSubscriptionObject();
        ToggleReviewButton();
        TogglePromoItem();
        if (DevelopmentPointsManager.Instance != null)
            _balanceText.text = $"Current Balance: {DevelopmentPointsManager.Instance.CurrentBalance} DP";

        FirebaseAnalytics.LogEvent("opened_store");
    }

    private void TogglePromoItem()
    {
        DateTime endDate = new DateTime(2024, 1, 31, 23, 59, 59);
        DateTime now = DateTime.Now;

        if (endDate < now)
        {
            _promoObject.SetActive(false);
        }
        else
        {
            _promoTimerText.text = $"special offers   <color=\"white\">Expires in {(endDate - now).Days} days and {(endDate - now).Hours} hours";
            _promoObject.SetActive(true);
        }
    }
}
