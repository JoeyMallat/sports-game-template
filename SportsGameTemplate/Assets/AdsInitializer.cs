using UnityEngine;

public class AdsInitializer : MonoBehaviour
{
    void Awake()
    {
        InitializeAds();
    }

    public void InitializeAds()
    {
        AdmobManager manager = AdmobManager.Instance;

        if (manager == null)
        {
            manager = FindObjectOfType<AdmobManager>();
        }

        if (manager == null)
        {
            manager = gameObject.AddComponent<AdmobManager>();
        }

        manager.InitializeAds();
    }
}