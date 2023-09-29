using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class TradeAssetWrapper
{
    [SerializeField] AssetType _assetType;
    [SerializeField] string _assetID;

    public TradeAssetWrapper(AssetType assetType, string id)
    {
        _assetType = assetType;
        _assetID = id;
    }

    public ITradeable GetTradeable(int teamID)
    {
        Team team = LeagueSystem.Instance.GetTeam(teamID);

        if (_assetType == AssetType.Player)
        {
            return team.GetPlayersFromTeam().Where(x => x.GetTradeableID() == _assetID).ToList()[0];
        } else if (_assetType == AssetType.DraftPick)
        {
            return team.GetDraftPicks().Where(x => x.GetTradeableID() == _assetID).ToList()[0];
        }

        return null;
    }
}
