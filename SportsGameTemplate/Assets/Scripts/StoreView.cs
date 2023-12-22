using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.RemoteConfig;

public class StoreView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _balanceText;
    [SerializeField] GameObject _subscriptionObject;
    [SerializeField] Button _premiumButton;
    [SerializeField] Button _rewardedAdsButton;

    private void Awake()
    {
        GameManager.OnGemsUpdated += UpdateBalance;
        RewardedAdsManager.OnRewardedAdWatched += GrantRewardedAdReward;
        ToggleSubscriptionObject();
        GameManager.OnPremiumStatusUpdated += TogglePremiumObject;
    }

    private void TogglePremiumObject(CloudSaveData cloudSaveData)
    {
        _premiumButton.ToggleButtonStatus(GameManager.Instance.GetPremiumStatus());
    }


    private void ToggleSubscriptionObject()
    {
        _subscriptionObject.SetActive(RemoteConfigService.Instance.appConfig.GetBool("show_subscription", false));
    }

    private void UpdateBalance(CloudSaveData data)
    {
        _balanceText.text = $"Current Balance: {data.GemAmount} <sprite name=\"Gem\">";
    }

    private void GrantRewardedAdReward(string adCode)
    {
        if (adCode == "gems")
        {
            GameManager.Instance.AddToGems(5);
            _rewardedAdsButton.ToggleButtonStatus(false);
        }
    }
}
