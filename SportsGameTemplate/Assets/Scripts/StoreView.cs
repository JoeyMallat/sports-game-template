using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.RemoteConfig;
using Firebase.Analytics;

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
    }

    private void ToggleReviewButton()
    {
        Debug.Log(PlayerPrefs.GetInt("reviewed"));
        if (PlayerPrefs.GetInt("reviewed") == 1)
        {
            _reviewForGemsButton.ToggleStoreButtonStatus(false);
        } else
        {
            _reviewForGemsButton.ToggleStoreButtonStatus(true);
            _reviewForGemsButton.onClick.RemoveAllListeners();
            _reviewForGemsButton.onClick.AddListener(() => GameManager.Instance.AddToGems(20));
            _reviewForGemsButton.onClick.AddListener(() => PlayerPrefs.SetInt("reviewed", 1));

            if (Application.platform == RuntimePlatform.Android)
            {
                _reviewForGemsButton.onClick.AddListener(() => Application.OpenURL(RemoteConfigService.Instance.appConfig.GetString("review_link_android", "market://details?id=" + Application.productName)));
            } else
            {
                _reviewForGemsButton.onClick.AddListener(() => Application.OpenURL(RemoteConfigService.Instance.appConfig.GetString("review_link_ios", "market://details?id=" + Application.productName)));
            }

            _reviewForGemsButton.onClick.AddListener(() => ToggleReviewButton());
        }
    }

    private void TogglePremiumObject(CloudSaveData cloudSaveData)
    {
        _premiumButton.ToggleStoreButtonStatus(!GameManager.Instance.GetPremiumStatus());
    }


    private void ToggleSubscriptionObject()
    {
        _subscriptionObject.SetActive(RemoteConfigService.Instance.appConfig.GetBool("show_subscription", false));
    }

    private void UpdateBalance(CloudSaveData data)
    {
        _premiumButton.ToggleStoreButtonStatus(!GameManager.Instance.GetPremiumStatus());
        _balanceText.text = $"Current Balance: {data.GemAmount} <sprite name=\"Gem\">";
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
        GameManager.OnGemsUpdated += UpdateBalance;
        RewardedAdsManager.OnRewardedAdWatched += GrantRewardedAdReward;
        ToggleSubscriptionObject();
        GameManager.OnPremiumStatusUpdated += TogglePremiumObject;
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
        } else
        {
            _promoTimerText.text = $"special offers   <color=\"white\">Expires in {(endDate - now).Days} days and {(endDate - now).Hours} hours";
            _promoObject.SetActive(true);
        }
    }
}
