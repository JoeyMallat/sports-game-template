using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TradeOffer
{
    [SerializeField] int _teamID;
    [SerializeField] List<TradeAssetWrapper> _includedTeammates;
    [SerializeField] List<TradeAssetWrapper> _tradeAssetsOffered;

    public TradeOffer (int teamID)
    {
        _teamID = teamID;
        _tradeAssetsOffered = new List<TradeAssetWrapper>();
        _includedTeammates = new List<TradeAssetWrapper>();
    }

    public int GetOfferingTeamID()
    {
        return _teamID;
    }

    public void AddTeammate(ITradeable teammate)
    {
        AssetType assetType = teammate.GetType() == typeof(Player) ? AssetType.Player : AssetType.DraftPick;
        _includedTeammates.Add(new TradeAssetWrapper(assetType, teammate.GetTradeableID()));
    }

    public void AddAsset(ITradeable asset)
    {
        AssetType assetType = asset.GetType() == typeof(Player) ? AssetType.Player : AssetType.DraftPick;
        _tradeAssetsOffered.Add(new TradeAssetWrapper(assetType, asset.GetTradeableID()));
    }

    public (List<ITradeable>, List<ITradeable>) GetAssets()
    {
        if (_tradeAssetsOffered.Count == 0) return (null, null);
        List<ITradeable> teammates = new List<ITradeable>();

        foreach (TradeAssetWrapper teammate in _includedTeammates)
        {
            teammates.Add(teammate.GetTradeable(GameManager.Instance.GetTeamID()));
        }

        List<ITradeable> assets = new List<ITradeable>();

        foreach (TradeAssetWrapper asset in _tradeAssetsOffered)
        {
            assets.Add(asset.GetTradeable(_teamID));
        }

        return (teammates, assets);
    }
}
