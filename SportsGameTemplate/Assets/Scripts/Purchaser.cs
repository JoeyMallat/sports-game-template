using Firebase.Analytics;
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
        CheckSubscription();
    }

    private void DistributeBoughtItems(string productID)
    {
        switch (productID)
        {
            case "com.basketballgm.allstar":
                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, new Parameter("value", 2.99f));
                GameManager.Instance.AddToGems(250);
                break;
            case "com.basketballgm.mvp":
                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, new Parameter("value", 6.99f));
                GameManager.Instance.AddToGems(250);
                GameManager.Instance.SetPremiumStatus(true);
                break;
            case "com.basketballgm.gems10":
                GameManager.Instance.AddToGems(10);
                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, new Parameter("value", 0.99f));

                break;
            case  "com.basketballgm.gems100":
                // 100 gems + 10 gems free
                GameManager.Instance.AddToGems(110);
                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, new Parameter("value", 2.99f));

                break;
            case "com.basketballgm.gems250":
                // 250 gems + 25 gems free
                GameManager.Instance.AddToGems(275);
                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, new Parameter("value", 4.99f));

                break;
            case "com.basketballgm.gems500":
                // 500 gems + 100 gems free
                GameManager.Instance.AddToGems(600);
                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, new Parameter("value", 9.99f));

                break;
            case "com.basketballgm.gems1000":
                // 1.000 gems + 500 gems free
                GameManager.Instance.AddToGems(1500);
                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, new Parameter("value", 14.99f));

                break;
            case "com.basketballgm.gems10k":
                // 10.000 gems + 2.000 gems free
                GameManager.Instance.AddToGems(12000);
                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, new Parameter("value", 29.99f));

                break;
            case "com.basketballgm.gems50k":
                // 50.000 gems + 10.000 gems free
                GameManager.Instance.AddToGems(60000);
                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, new Parameter("value", 74.99f));

                break;
            case "com.basketballgm.gems100k":
                // 100.000 gems + 100.000 gems free
                GameManager.Instance.AddToGems(200000);
                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, new Parameter("value", 99.99f));

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

    private void CheckSubscription()
    {
        Dictionary<string, string> dict = new Dictionary<string, string>() { { "com.basketballgm.allstar", "2.99USD" } };
        foreach (Product item in _iapManager.GetController().products.all)
        {
           // this is the usage of SubscriptionManager class
           if (item.receipt != null) {
               if (item.definition.type == ProductType.Subscription) {
                   string intro_json = (dict == null || !dict.ContainsKey(item.definition.storeSpecificId)) ? null :  dict[item.definition.storeSpecificId];
                   SubscriptionManager p = new SubscriptionManager(item, intro_json);
                   SubscriptionInfo info = p.getSubscriptionInfo();
                   Debug.Log(info.getProductId());
                   Debug.Log(info.getPurchaseDate());
                   Debug.Log(info.getExpireDate());
                   Debug.Log(info.isSubscribed());
                   Debug.Log(info.isExpired());
                   Debug.Log(info.isCancelled());
                   Debug.Log(info.isFreeTrial());
                   Debug.Log(info.isAutoRenewing());
                   Debug.Log(info.getRemainingTime());
                   Debug.Log(info.isIntroductoryPricePeriod());
                   Debug.Log(info.getIntroductoryPrice());
                   Debug.Log(info.getIntroductoryPricePeriod());
                   Debug.Log(info.getIntroductoryPricePeriodCycles());
                    GameManager.Instance.SetPremiumStatus(info.isSubscribed() == Result.True);
               } else {
                   //Debug.Log("the product is not a subscription product");
               }
           } else {
               //Debug.Log($"{item.definition.id} should have a valid receipt");
            }
        }
    }
}
