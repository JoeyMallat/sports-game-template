using System;
using UnityEngine;

public class RewardedAdsManager : MonoBehaviour
{
    public static RewardedAdsManager Instance;
    public static event Action<string> OnRewardedAdWatched;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void LoadAd()
    {
        AdmobManager.Instance?.LoadRewardedAd();
    }

    public void ShowAdForGems()
    {
        ShowAd("gems");
    }

    public void ShowAdForChecklist()
    {
        ShowAd("checklist");
    }

    void ShowAd(string adCode)
    {
        AdmobManager.Instance?.ShowRewardedAd(
            () => OnRewardedAdWatched?.Invoke(adCode),
            adCode);
    }
}