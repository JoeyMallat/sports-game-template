using System;
using Firebase.Analytics;
using TMPro;
using Unity.Services.RemoteConfig;
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
        GameManager.OnStoreItemsUpdated += UpdateBalance;
        GameManager.OnStoreItemsUpdated += TogglePremiumObject;
        RewardedAdsManager.OnRewardedAdWatched += GrantRewardedAdReward;
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
            _reviewForGemsButton.onClick.AddListener(() => GameManager.Instance.AddToGems(20));
            _reviewForGemsButton.onClick.AddListener(() => PlayerPrefs.SetInt("reviewed", 1));

            if (Application.platform == RuntimePlatform.Android)
            {
                _reviewForGemsButton.onClick.AddListener(() => Application.OpenURL(RemoteConfigService.Instance.appConfig.GetString("review_link_android", "market://details?id=" + Application.productName)));
            }
            else
            {
                _reviewForGemsButton.onClick.AddListener(() => Application.OpenURL(RemoteConfigService.Instance.appConfig.GetString("review_link_ios", "market://details?id=" + Application.productName)));
            }

            _reviewForGemsButton.onClick.AddListener(() => ToggleReviewButton());
        }
    }

    private void TogglePremiumObject(int gems, bool premium)
    {
        _premiumButton.ToggleStoreButtonStatus(!GameManager.Instance.GetPremiumStatus());
    }


    private void ToggleSubscriptionObject()
    {
        _subscriptionObject.SetActive(RemoteConfigService.Instance.appConfig.GetBool("show_subscription", false));
    }

    private void UpdateBalance(int gems, bool premiumStatus)
    {
        Debug.Log($"Set balance to {GameManager.Instance.GetGems()}");
        _balanceText.text = $"Current Balance: {GameManager.Instance.GetGems()} <sprite name=\"Gem\">";
        _premiumButton.ToggleStoreButtonStatus(!GameManager.Instance.GetPremiumStatus());
    }

    private void GrantRewardedAdReward(string adCode)
    {
        if (adCode == "gems")
        {
            GameManager.Instance.AddToGems(5);
            _rewardedAdsButton.ToggleStoreButtonStatus(false);
        }
    }

    public void SetDetails<T>(T item) where T : class
    {
        ToggleSubscriptionObject();
        ToggleReviewButton();
        TogglePromoItem();

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
