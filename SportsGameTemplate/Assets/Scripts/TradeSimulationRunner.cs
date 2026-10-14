using System.Collections.Generic;
using UnityEngine;

public static class TradeSimulationRunner
{
    public static TradeSimulationReport Run(IReadOnlyList<Team> teams, int attempts, int seed)
    {
        TradeSimulationReport report = new TradeSimulationReport { Attempts = attempts };
        if (teams == null || teams.Count < 2 || attempts <= 0)
            return report;

        Random.State previousState = Random.state;
        Random.InitState(seed);
        AITrader trader = new AITrader();
        TradingSystem.Validator validator = new TradingSystem.Validator(teams);
        TradeValueService valuation = new TradeValueService(teams);

        for (int attempt = 0; attempt < attempts; attempt++)
        {
            Team requestingTeam = GameManager.Instance == null
                ? teams[0]
                : LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID());
            Team offeringTeam = teams[Random.Range(0, teams.Count)];
            if (requestingTeam == offeringTeam)
                continue;

            List<ITradeable> requestedAssets = requestingTeam.GetTradeAssets();
            if (requestedAssets.Count == 0 || offeringTeam.GetTradeAssets().Count == 0)
                continue;

            ITradeable target = requestedAssets[Random.Range(0, requestedAssets.Count)];
            TradeOffer offer = trader.GenerateOffer(requestedAssets, offeringTeam.GetTeamID(), valuation.GetMarketValue(target), offeringTeam.GetTradeAssets());
            if (offer == null)
                continue;

            (List<ITradeable> fromRequestingTeam, List<ITradeable> fromOfferingTeam) = offer.GetAssets();
            if (fromRequestingTeam == null || fromOfferingTeam == null)
                continue;

            report.OffersGenerated++;
            CountAssets(fromRequestingTeam, report);
            CountAssets(fromOfferingTeam, report);
            int offeredValue = valuation.GetPackageValueForTeam(fromOfferingTeam, requestingTeam);
            int requestedValue = valuation.GetPackageValueForTeam(fromRequestingTeam, offeringTeam);
            report.TotalValueRatio += offeredValue / (float)Mathf.Max(1, requestedValue);

            if (validator.Validate(requestingTeam, fromRequestingTeam, offeringTeam, fromOfferingTeam).IsValid)
                report.ValidOffers++;
        }

        Random.state = previousState;
        return report;
    }

    private static void CountAssets(IEnumerable<ITradeable> assets, TradeSimulationReport report)
    {
        foreach (ITradeable asset in assets)
        {
            report.TotalAssets++;
            if (asset is Player)
                report.PlayerAssets++;
            else if (asset is DraftPick)
                report.DraftPickAssets++;
        }
    }
}
