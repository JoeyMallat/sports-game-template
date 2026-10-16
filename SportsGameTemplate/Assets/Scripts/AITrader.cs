using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AITrader
{
    public List<ITradeable> GenerateOffer(int tradeValue, List<ITradeable> assetsToUse)
    {
        if (assetsToUse == null || assetsToUse.Count == 0 || tradeValue <= 0)
            return new List<ITradeable>();

        TradeValueService valuation = new TradeValueService(LeagueSystem.Instance.GetTeams());
        return FindBestPackage(assetsToUse, tradeValue, valuation.Context.Settings.MaximumAssetsPerSide, valuation, null);
    }

    public TradeOffer GenerateOffer(List<ITradeable> teamAssets, int offeringTeamID, int tradeValue, List<ITradeable> assetsToUse)
    {
        if (teamAssets == null || assetsToUse == null)
            return null;

        TradeOffer tradeOffer = new TradeOffer(offeringTeamID);
        Team userTeam = LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID());
        Team offeringTeam = LeagueSystem.Instance.GetTeam(offeringTeamID);
        TradeValueService valuation = new TradeValueService(LeagueSystem.Instance.GetTeams());
        TradeValueSettings settings = valuation.Context.Settings;

        List<ITradeable> myTeamAssets = FindBestPackage(teamAssets, tradeValue, settings.MaximumAssetsPerSide, valuation, offeringTeam);

        // Return null if no assets were added
        if (myTeamAssets.Count == 0) return null;

        // Add these assets to trade offer (include the first player as there is no other to add him to the trade)
        foreach (ITradeable tradeable in myTeamAssets)
        {
            tradeOffer.AddTeammate(tradeable);
        }

        // Generate an offer for the selected myTeamAssets
        int requestedValue = valuation.GetPackageValueForTeam(myTeamAssets, offeringTeam);
        List<ITradeable> offeredAssets = FindBestPackage(assetsToUse, requestedValue, settings.MaximumAssetsPerSide, valuation, userTeam);

        if (offeredAssets.Count == 0) return null;

        foreach (ITradeable asset in offeredAssets)
        {
            tradeOffer.AddAsset(asset);
        }

        Debug.Log($"Generated trade offer with {offeredAssets.Count} assets offered");
        myTeamAssets[0].AddTradeOffer(tradeOffer);

        return tradeOffer;
    }

    private List<ITradeable> FindBestPackage(List<ITradeable> assets, int targetValue, int maximumAssets, TradeValueService valuation, Team receivingTeam)
    {
        List<ITradeable> candidates = assets
            .Where(asset => asset != null)
            .OrderBy(asset => Mathf.Abs(GetValue(asset, valuation, receivingTeam) - targetValue))
            .Take(12)
            .ToList();
        List<ITradeable> bestPackage = new List<ITradeable>();
        float bestScore = float.MaxValue;

        SearchPackages(candidates, 0, targetValue, maximumAssets, valuation, receivingTeam, new List<ITradeable>(), ref bestPackage, ref bestScore);
        return bestPackage;
    }

    private void SearchPackages(
        List<ITradeable> candidates,
        int index,
        int targetValue,
        int maximumAssets,
        TradeValueService valuation,
        Team receivingTeam,
        List<ITradeable> currentPackage,
        ref List<ITradeable> bestPackage,
        ref float bestScore)
    {
        if (currentPackage.Count > 0)
        {
            int packageValue = GetPackageValue(currentPackage, valuation, receivingTeam);
            float difference = Mathf.Abs(packageValue - targetValue) / (float)Mathf.Max(1, targetValue);
            float complexityPenalty = (currentPackage.Count - 1) * 0.025f;
            float score = difference + complexityPenalty;
            if (score < bestScore)
            {
                bestScore = score;
                bestPackage = new List<ITradeable>(currentPackage);
            }

            if (currentPackage.Count >= maximumAssets || packageValue > targetValue * 1.25f)
                return;
        }

        for (int i = index; i < candidates.Count; i++)
        {
            currentPackage.Add(candidates[i]);
            SearchPackages(candidates, i + 1, targetValue, maximumAssets, valuation, receivingTeam, currentPackage, ref bestPackage, ref bestScore);
            currentPackage.RemoveAt(currentPackage.Count - 1);
        }
    }

    public string GetOfferDebugSummary(IEnumerable<ITradeable> assets, Team receivingTeam)
    {
        TradeValueService valuation = new TradeValueService(LeagueSystem.Instance.GetTeams());
        List<ITradeable> package = assets.Where(asset => asset != null).ToList();
        int marketValue = valuation.GetPackageMarketValue(package);
        int teamValue = valuation.GetPackageValueForTeam(package, receivingTeam);
        return $"{receivingTeam.GetTeamName()}: {package.Count} assets, market {marketValue}, team value {teamValue}, direction {valuation.GetProfile(receivingTeam).Direction}";
    }

    private int GetValue(ITradeable asset, TradeValueService valuation, Team receivingTeam)
    {
        return receivingTeam == null
            ? valuation.GetMarketValue(asset)
            : valuation.GetValueForTeam(asset, receivingTeam);
    }

    private int GetPackageValue(IEnumerable<ITradeable> assets, TradeValueService valuation, Team receivingTeam)
    {
        return receivingTeam == null
            ? valuation.GetPackageMarketValue(assets)
            : valuation.GetPackageValueForTeam(assets, receivingTeam);
    }
}
