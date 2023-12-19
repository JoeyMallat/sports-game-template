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

    private void Awake()
    {
        GameManager.OnGemsUpdated += UpdateBalance;
        ToggleSubscriptionObject();
    }

    private void ToggleSubscriptionObject()
    {
        _subscriptionObject.SetActive(RemoteConfigService.Instance.appConfig.GetBool("show_subscription", false));
    }

    private void UpdateBalance(CloudSaveData data)
    {
        _balanceText.text = $"Current Balance: {data.GemAmount} <sprite name=\"Gem\">";
    }
}
