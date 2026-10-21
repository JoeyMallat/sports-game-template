using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TradeOffer
{
    [SerializeField] string _offerID;
    [SerializeField] int _teamID;
    [SerializeField] List<TradeAssetWrapper> _includedTeammates;
    [SerializeField] List<TradeAssetWrapper> _tradeAssetsOffered;

    public TradeOffer(int teamID)
    {
        _offerID = Guid.NewGuid().ToString("N");
        _teamID = teamID;
        _tradeAssetsOffered = new List<TradeAssetWrapper>();
        _includedTeammates = new List<TradeAssetWrapper>();
    }

    public string GetOfferID()
    {
        if (string.IsNullOrWhiteSpace(_offerID))
            _offerID = Guid.NewGuid().ToString("N");

        return _offerID;
    }

    public HashSet<string> GetReferencedAssetIDs()
    {
        HashSet<string> assetIDs = new HashSet<string>();
        AddAssetIDs(_includedTeammates, assetIDs);
        AddAssetIDs(_tradeAssetsOffered, assetIDs);
        return assetIDs;
    }

    public bool ReferencesAnyAsset(ISet<string> assetIDs)
    {
        if (assetIDs == null || assetIDs.Count == 0)
            return false;

        return ContainsAnyAsset(_includedTeammates, assetIDs) || ContainsAnyAsset(_tradeAssetsOffered, assetIDs);
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
        if (_tradeAssetsOffered == null || _tradeAssetsOffered.Count == 0)
            return (new List<ITradeable>(), new List<ITradeable>());

        List<ITradeable> teammates = new List<ITradeable>();

        if (_includedTeammates != null)
        {
            foreach (TradeAssetWrapper teammate in _includedTeammates)
            {
                ITradeable tradeable = teammate.GetTradeable(GameManager.Instance.GetTeamID());
                if (tradeable != null)
                    teammates.Add(tradeable);
            }
        }

        List<ITradeable> assets = new List<ITradeable>();

        foreach (TradeAssetWrapper asset in _tradeAssetsOffered)
        {
            ITradeable tradeable = asset.GetTradeable(_teamID);
            if (tradeable != null)
                assets.Add(tradeable);
        }

        return (teammates, assets);
    }

    static void AddAssetIDs(List<TradeAssetWrapper> wrappers, HashSet<string> assetIDs)
    {
        if (wrappers == null)
            return;

        for (int i = 0; i < wrappers.Count; i++)
        {
            string assetID = wrappers[i].GetAssetID();
            if (!string.IsNullOrWhiteSpace(assetID))
                assetIDs.Add(assetID);
        }
    }

    static bool ContainsAnyAsset(List<TradeAssetWrapper> wrappers, ISet<string> assetIDs)
    {
        if (wrappers == null)
            return false;

        for (int i = 0; i < wrappers.Count; i++)
        {
            if (assetIDs.Contains(wrappers[i].GetAssetID()))
                return true;
        }

        return false;
    }
}
