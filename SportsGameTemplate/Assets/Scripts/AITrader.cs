using System.Collections;
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
}
