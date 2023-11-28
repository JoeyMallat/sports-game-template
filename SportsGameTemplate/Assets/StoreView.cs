using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _balanceText;

    private void Awake()
    {
        GameManager.OnGemsUpdated += UpdateBalance;
    }

    private void UpdateBalance(CloudSaveData data)
    {
        _balanceText.text = $"Current Balance: {data.GemAmount} <sprite name=\"Gem\">";
    }
}
