using System.Text;

public static class TradeValueExplanation
{
    public static string ForPlayer(Player player, TradeValueContext context)
    {
        TradeValueResult result = player.CalculateTradeValue(context);
        StringBuilder explanation = new StringBuilder();
        explanation.Append($"{player.GetFullName()}: {result.TotalValue}");
        explanation.Append($" | talent {result.TalentValue:0}");
        explanation.Append($" | age x{result.AgeMultiplier:0.00}");
        explanation.Append($" | potential x{result.PotentialMultiplier:0.00}");
        explanation.Append($" | production x{result.ProductionMultiplier:0.00}");
        explanation.Append($" | contract x{result.ContractMultiplier:0.00}");
        return explanation.ToString();
    }

    public static string ForPick(DraftPick pick, TradeValueContext context, System.Collections.Generic.IReadOnlyList<Team> teams)
    {
        TradeValueResult result = pick.CalculateTradeValue(context, teams);
        return $"{pick.GetPickDataString()} ({pick.GetDraftSeason()}): {result.TotalValue} | slot {result.TalentValue:0} | future x{result.AgeMultiplier:0.00} | class x{result.PotentialMultiplier:0.00}";
    }
}
