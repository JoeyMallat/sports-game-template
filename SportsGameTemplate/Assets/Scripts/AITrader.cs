using System.Collections.Generic;
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
            if (valueOfAssetsPicked <= tradeValue * Random.Range(0.65f, 1.05f) && assets.Count < 5)
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

        List<ITradeable> assets = GenerateOffer(tradeValue, assetsToUse);

        foreach (ITradeable tradeable in assets)
        {
            tradeOffer.AddAsset(tradeable);
        }

        foreach (ITradeable asset in teamAssets)
        {
            tradeOffer.AddTeammate(asset);
        }

        Player player = teamAssets[0] as Player;
        player.AddTradeOffer(tradeOffer);

        return tradeOffer;
    }
}
