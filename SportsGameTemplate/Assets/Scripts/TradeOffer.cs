using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TradeOffer
{
    [SerializeField] int _teamID;
    [SerializeField] List<TradeAssetWrapper> _tradeAssetsOffered;

    public TradeOffer (int teamID)
    {
        _teamID = teamID;
        _tradeAssetsOffered = new List<TradeAssetWrapper>();
    }

    public int GetOfferingTeamID()
    {
        return _teamID;
    }

    public void AddAsset(ITradeable asset)
    {
        AssetType assetType = asset.GetType() == typeof(Player) ? AssetType.Player : AssetType.DraftPick;
        _tradeAssetsOffered.Add(new TradeAssetWrapper(assetType, asset.GetTradeableID()));
    }

    public List<ITradeable> GetAssets()
    {
        List<ITradeable> assets = new List<ITradeable>();

        foreach (TradeAssetWrapper asset in _tradeAssetsOffered)
        {
            assets.Add(asset.GetTradeable(_teamID));
        }

        return assets;
    }
}
