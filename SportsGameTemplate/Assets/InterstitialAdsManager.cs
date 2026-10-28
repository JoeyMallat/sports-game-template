using UnityEngine;

public class InterstitialAdsManager : MonoBehaviour
{
    public static InterstitialAdsManager Instance;

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
        AdmobManager.Instance?.LoadInterstitialAd();
    }

    public void ShowAd()
    {
        AdmobManager.Instance?.ShowInterstitialAd();
    }
}
