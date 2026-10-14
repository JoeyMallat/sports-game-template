using System.Linq;
using UnityEngine;

public static class PlayerTradeValuator
{
    public static TradeValueResult Evaluate(Player player, TradeValueContext context)
    {
        TradeValueSettings settings = context.Settings;
        float overall = (float)player.CalculateRatingForPosition();
        float averageAttribute = player.GetSkills().Count == 0
            ? overall
            : player.GetSkills().Sum(skill => skill.GetRatingForSkill()) / (float)player.GetSkills().Count;
        float talentRating = Mathf.Lerp(overall, averageAttribute, settings.AttributeWeight);
        float talentValue = settings.PlayerValueScale * Mathf.Pow(Mathf.Max(1f, talentRating) / 50f, settings.RatingExponent);

        float ageMultiplier = GetAgeMultiplier(player.GetAge());
        float potentialMultiplier = GetPotentialMultiplier(player.GetPotential(), player.GetAge());
        float productionMultiplier = GetProductionMultiplier(player, context);
        float contractMultiplier = GetContractMultiplier(player, context);

        return new TradeValueResult(
            talentValue,
            ageMultiplier,
            potentialMultiplier,
            productionMultiplier,
            contractMultiplier);
    }

    private static float GetAgeMultiplier(int age)
    {
        if (age <= 21)
            return 1.18f;
        if (age <= 27)
            return Mathf.Lerp(1.15f, 1f, (age - 21f) / 6f);
        if (age <= 32)
            return Mathf.Lerp(1f, 0.72f, (age - 27f) / 5f);

        return Mathf.Lerp(0.72f, 0.3f, Mathf.Clamp01((age - 32f) / 8f));
    }

    private static float GetPotentialMultiplier(Potential potential, int age)
    {
        float potentialMultiplier;
        switch (potential)
        {
            case Potential.Elite:
                potentialMultiplier = 1.32f;
                break;
            case Potential.WorldClass:
                potentialMultiplier = 1.2f;
                break;
            case Potential.Starter:
                potentialMultiplier = 1.08f;
                break;
            case Potential.Bench:
                potentialMultiplier = 0.94f;
                break;
            default:
                potentialMultiplier = 0.82f;
                break;
        }

        float potentialRelevance = Mathf.Clamp01((32f - age) / 10f);
        return Mathf.Lerp(1f, potentialMultiplier, potentialRelevance);
    }

    private static float GetProductionMultiplier(Player player, TradeValueContext context)
    {
        PlayerSeason season = player.GetLatestSeason();
        int gamesPlayed = season.GetMatchStats().Count;
        if (gamesPlayed == 0)
            return 1f;

        float pointsScore = GetRelativeScore(season.GetAveragePoints(), context.LeagueAveragePoints);
        float assistsScore = GetRelativeScore(season.GetAverageOfStat("assists"), context.LeagueAverageAssists);
        float reboundsScore = GetRelativeScore(season.GetAverageOfStat("rebounds"), context.LeagueAverageRebounds);
        float productionScore = pointsScore * 0.6f + assistsScore * 0.2f + reboundsScore * 0.2f;
        float fullProductionMultiplier = Mathf.Clamp(0.75f + productionScore * 0.25f, 0.75f, 1.25f);
        float confidence = Mathf.Clamp01(gamesPlayed / (float)Mathf.Max(1, context.Settings.ProductionGamesForFullConfidence));

        return Mathf.Lerp(1f, fullProductionMultiplier, confidence * context.Settings.ProductionWeight);
    }

    private static float GetRelativeScore(float value, float leagueAverage)
    {
        if (leagueAverage <= 0f)
            return 1f;

        return Mathf.Clamp(value / leagueAverage, 0f, 2f);
    }

    private static float GetContractMultiplier(Player player, TradeValueContext context)
    {
        Contract contract = player.GetContract();
        if (contract == null)
            return 1f;

        float ratingRatio = player.CalculateRatingForPosition() / Mathf.Max(1f, context.LeagueAverageRating);
        float estimatedMarketSalary = context.LeagueAverageSalary * Mathf.Pow(ratingRatio, 3f);
        float salarySurplusRatio = (estimatedMarketSalary - contract.GetYearlySalary()) / Mathf.Max(estimatedMarketSalary, context.LeagueAverageSalary * 0.25f);
        float yearsImpact = Mathf.Clamp01(contract.GetYearsOnContract() / 4f);
        float adjustment = salarySurplusRatio * yearsImpact * context.Settings.SalaryValueScale;
        adjustment = Mathf.Clamp(adjustment, -context.Settings.MaximumContractAdjustment, context.Settings.MaximumContractAdjustment);

        return 1f + adjustment;
    }
}
