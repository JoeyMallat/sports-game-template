using System;
using UnityEngine;

[Serializable]
public class TradeValueSettings
{
    [Header("Player Value")]
    [Min(1f)] public float PlayerValueScale = 1000f;
    [Min(1f)] public float RatingExponent = 3.2f;
    [Range(0f, 1f)] public float AttributeWeight = 0.2f;
    [Range(0f, 1f)] public float ProductionWeight = 0.2f;
    [Min(1)] public int ProductionGamesForFullConfidence = 20;
    [Min(0f)] public float SalaryValueScale = 0.35f;
    [Min(0f)] public float MaximumContractAdjustment = 0.4f;

    [Header("Draft Pick Value")]
    [Min(1f)] public float FirstPickValue = 95000f;
    [Min(0.1f)] public float PickValueExponent = 0.72f;
    [Range(0f, 1f)] public float FuturePickDiscount = 0.82f;
    [Range(0f, 1f)] public float MinimumClassMultiplier = 0.7f;
    [Min(1f)] public float MaximumClassMultiplier = 1.3f;

    [Header("Negotiation")]
    [Range(0f, 1f)] public float MinimumAcceptanceRatio = 0.95f;
    [Range(1f, 2f)] public float MaximumOfferRatio = 1.15f;
    [Range(0f, 1f)] public float TeamFitMaximumAdjustment = 0.2f;
    [Range(1, 5)] public int MaximumAssetsPerSide = 5;
    [Range(1, 5)] public int FutureDraftClassHorizon = 3;

    public static TradeValueSettings Default => new TradeValueSettings();
}
