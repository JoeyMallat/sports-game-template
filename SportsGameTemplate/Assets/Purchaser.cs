using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class Purchaser : MonoBehaviour
{
    IAPManager _iapManager;

    private void Awake()
    {
        _iapManager = new IAPManager();
        IAPManager.OnProductPurchased += DistributeBoughtItems;
    }

    private void DistributeBoughtItems(string productID)
    {
        switch (productID)
        {
            case "com.basketballgm.allstar":
                GameManager.Instance.EditGems(250);
                break;
            case "com.basketballgm.mvp":
                GameManager.Instance.EditGems(250);
                break;
            case "com.basketballgm.gems10":
                GameManager.Instance.EditGems(10);
                break;
            case  "com.basketballgm.gems100":
                // 100 gems + 10 gems free
                GameManager.Instance.EditGems(110);
                break;
            case "com.basketballgm.gems250":
                // 250 gems + 25 gems free
                GameManager.Instance.EditGems(275);
                break;
            case "com.basketballgm.gems500":
                // 500 gems + 100 gems free
                GameManager.Instance.EditGems(600);
                break;
            case "com.basketballgm.gems1000":
                // 1.000 gems + 500 gems free
                GameManager.Instance.EditGems(1500);
                break;
            case "com.basketballgm.gems10k":
                // 10.000 gems + 2.000 gems free
                GameManager.Instance.EditGems(12000);
                break;
            case "com.basketballgm.gems50k":
                // 50.000 gems + 10.000 gems free
                GameManager.Instance.EditGems(60000);
                break;
            case "com.basketballgm.gems100k":
                // 100.000 gems + 100.000 gems free
                GameManager.Instance.EditGems(200000);
                break;
            default:
                Debug.Log("Could not distribute rewards");
                break;
        }
    }

    public void OnProductPurchase(string productID)
    {
        _iapManager.GetController().InitiatePurchase(productID);
    }
}
