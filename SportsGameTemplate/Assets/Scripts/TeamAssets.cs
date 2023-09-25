using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamAssets : MonoBehaviour
{
    [SerializeField] int _teamID;
    List<TeamAsset> _teamAssets;

    private void Awake()
    {
        TradingSystem.OnAssetsUpdated += UpdateTeamAssets;
    }

    public void UpdateTeamAssets(int teamID, List<ITradeable> tradeAssets)
    {
        Navigation.Instance.GoToScreen(false, Navigation.Instance.GetCanvas(CanvasKey.Trade));

        if (teamID != _teamID) return;

        if (_teamAssets == null)
        {
            _teamAssets = GetComponentsInChildren<TeamAsset>().ToList();
        }

        int tradeAssetsAmount = tradeAssets.Count;

        for (int i = 0; i < _teamAssets.Count; i++)
        {
            if (i < tradeAssetsAmount)
            {
                _teamAssets[i].SetAssetDetails(tradeAssets[i]);
            }
        }
    }
}
