using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AITrader
{
    public List<ITradeable> GenerateOffer(int tradeValue, List<ITradeable> assetsToUse)
    {
        List<ITradeable> assets = new List<ITradeable>();
        assetsToUse = assetsToUse.Shuffle();

        int valueOfAssetsPicked = 0;

        foreach (ITradeable asset in assetsToUse)
        {
            if (valueOfAssetsPicked <= tradeValue && assets.Count < 5)
            {
                assets.Add(asset);
                valueOfAssetsPicked += asset.CalculateTradeValue();
            } else
            {
                return assets;
            }
        }

        return assets;
    }

    public TradeOffer GenerateOffer(List<ITradeable> teamAssets, int offeringTeamID, int tradeValue, List<ITradeable> assetsToUse)
    {
        TradeOffer tradeOffer = new TradeOffer(offeringTeamID);
        tradeValue = Mathf.RoundToInt(tradeValue * Random.Range(0.65f, 1.05f));

        // Generate a starting set of players from list of assets
        List<ITradeable> myTeamAssets = GenerateOffer(tradeValue, teamAssets);

        // Return null if no assets were added
        if (myTeamAssets.Count == 0) return null;

        // Add these assets to trade offer (include the first player as there is no other to add him to the trade
        foreach (ITradeable tradeable in myTeamAssets)
        {
            tradeOffer.AddTeammate(tradeable);
        }

        // Generate an offer for the selected myTeamAssets
        int totalValueOfMyAssets = 0;
        myTeamAssets.ForEach(x => totalValueOfMyAssets += x.CalculateTradeValue());
        List<ITradeable> offeredAssets = GenerateOffer(Mathf.RoundToInt(totalValueOfMyAssets * UnityEngine.Random.Range(0.2f, 0.9f)), assetsToUse);

        if (offeredAssets.Count == 0) return null;

        foreach (ITradeable asset in offeredAssets)
        {
            tradeOffer.AddAsset(asset);
        }

        // Find the first player and add the trade offer to him
        myTeamAssets[0].AddTradeOffer(tradeOffer);

        return tradeOffer;
    }
}
