using System;

[Serializable]
public class TradeValueResult
{
    public int TotalValue { get; }
    public float TalentValue { get; }
    public float AgeMultiplier { get; }
    public float PotentialMultiplier { get; }
    public float ProductionMultiplier { get; }
    public float ContractMultiplier { get; }
    public float TeamFitMultiplier { get; }

    public TradeValueResult(
        float talentValue,
        float ageMultiplier = 1f,
        float potentialMultiplier = 1f,
        float productionMultiplier = 1f,
        float contractMultiplier = 1f,
        float teamFitMultiplier = 1f)
    {
        TalentValue = talentValue;
        AgeMultiplier = ageMultiplier;
        PotentialMultiplier = potentialMultiplier;
        ProductionMultiplier = productionMultiplier;
        ContractMultiplier = contractMultiplier;
        TeamFitMultiplier = teamFitMultiplier;
        TotalValue = Math.Max(1, (int)Math.Round(talentValue * ageMultiplier * potentialMultiplier * productionMultiplier * contractMultiplier * teamFitMultiplier));
    }
}
