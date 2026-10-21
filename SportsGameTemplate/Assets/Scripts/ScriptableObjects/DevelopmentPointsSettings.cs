using System;
using System.Collections.Generic;
using UnityEngine;

public enum DevelopmentPointsOwnershipScope
{
    AccountWide,
    Career
}

[Serializable]
public struct DevelopmentPointsProductGrant
{
    [SerializeField] string _productId;
    [SerializeField, Min(1)] int _amount;

    public string ProductId => _productId;
    public int Amount => _amount;
}

[CreateAssetMenu(fileName = "DevelopmentPointsSettings", menuName = "Sports Game Template/Development Points Settings")]
public class DevelopmentPointsSettings : ScriptableObject
{
    [Header("Ownership")]
    [SerializeField] DevelopmentPointsOwnershipScope _ownershipScope = DevelopmentPointsOwnershipScope.AccountWide;

    [Header("Match Rewards")]
    [SerializeField, Min(0)] int _winReward = 40;
    [SerializeField, Min(0)] int _lossReward = 10;
    [SerializeField, Min(1f)] float _franchisePremiumMatchMultiplier = 1.5f;

    [Header("Other Rewards")]
    [SerializeField, Min(0)] int _rewardedAdvertisementReward = 5;
    [SerializeField, Min(0)] int _reviewReward = 20;
    [SerializeField, Min(0)] int _welcomeReward = 10;

    [Header("Facility Costs")]
    [SerializeField, Min(0)] int _trainingCenterCost = 500;
    [SerializeField, Min(0)] int _medicalCenterCost = 500;
    [SerializeField, Min(0)] int _scoutingCenterCost = 500;

    [Header("Staff And Scouting Costs")]
    [SerializeField, Min(0)] int _hireCoachCost = 250;
    [SerializeField, Min(0)] int _renewCoachCost = 200;
    [SerializeField, Min(0)] int _draftScoutingReportCost = 100;

    [Header("Persistence")]
    [SerializeField, Min(10)] int _maximumTransactionHistory = 500;

    [Header("IAP Grants")]
    [SerializeField] List<DevelopmentPointsProductGrant> _productGrants = new List<DevelopmentPointsProductGrant>();

    public DevelopmentPointsOwnershipScope OwnershipScope => _ownershipScope;
    public int WinReward => _winReward;
    public int LossReward => _lossReward;
    public float FranchisePremiumMatchMultiplier => _franchisePremiumMatchMultiplier;
    public int RewardedAdvertisementReward => _rewardedAdvertisementReward;
    public int ReviewReward => _reviewReward;
    public int WelcomeReward => _welcomeReward;
    public int TrainingCenterCost => _trainingCenterCost;
    public int MedicalCenterCost => _medicalCenterCost;
    public int ScoutingCenterCost => _scoutingCenterCost;
    public int HireCoachCost => _hireCoachCost;
    public int RenewCoachCost => _renewCoachCost;
    public int DraftScoutingReportCost => _draftScoutingReportCost;
    public int MaximumTransactionHistory => _maximumTransactionHistory;

    public int GetProductGrant(string productId)
    {
        if (string.IsNullOrEmpty(productId))
            return 0;

        for (int i = 0; i < _productGrants.Count; i++)
        {
            if (string.Equals(_productGrants[i].ProductId, productId, StringComparison.Ordinal))
                return _productGrants[i].Amount;
        }

        return GetDefaultProductGrant(productId);
    }

    public static int GetDefaultProductGrant(string productId)
    {
        switch (productId)
        {
            case "com.basketballgm.allstar": return 250;
            case "com.basketballgm.mvp": return 250;
            case "com.basketballgm.mvppromo": return 250;
            case "com.basketballgm.promo599": return 2024;
            case "com.basketballgm.gems10": return 10;
            case "com.basketballgm.gems100": return 110;
            case "com.basketballgm.gems250": return 275;
            case "com.basketballgm.gems500": return 600;
            case "com.basketballgm.gems1000": return 1500;
            case "com.basketballgm.gems10k": return 12000;
            case "com.basketballgm.gems50k": return 60000;
            case "com.basketballgm.gems100k": return 200000;
            default: return 0;
        }
    }
}
