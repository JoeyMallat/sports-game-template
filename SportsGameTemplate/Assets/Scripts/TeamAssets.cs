using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TeamAssets : MonoBehaviour
{
    [SerializeField] int _teamIndex;
    [SerializeField] int _teamID;
    [SerializeField] TextMeshProUGUI _teamName;
    List<TeamAsset> _teamAssets;

    private void Awake()
    {
        TradingSystem.OnAssetsUpdated += UpdateTeamAssets;
        TradingSystem.OnTradeCompleted += SetTeamID;
    }

    private void SetTeamID()
    {
        _teamID = -1;
        UpdateTeamAssets(1, -1, new List<ITradeable>(), false);
    }

    public void UpdateTeamAssets(int teamIndex, int teamID, List<ITradeable> tradeAssets, bool reloadScreen)
    {
        if (reloadScreen)
            Navigation.Instance.GoToScreen(true, CanvasKey.Trade);

        if (teamIndex != _teamIndex) return;

        if (_teamAssets == null)
        {
            _teamAssets = GetComponentsInChildren<TeamAsset>().ToList();
        }

        _teamID = teamID;

        int tradeAssetsAmount = tradeAssets.Count;

        for (int i = 0; i < _teamAssets.Count; i++)
        {
            if (i < tradeAssetsAmount)
            {
                _teamAssets[i].SetAssetDetails(tradeAssets[i]);
            }
            else
            {
                _teamAssets[i].SetAssetDetails();
            }
        }

        _teamName.text = _teamID == -1 ? "" : LeagueSystem.Instance.GetTeam(_teamID).GetTeamName();
    }

    public int GetTeamID()
    {
        return _teamID;
    }

    public int GetTeamIndex()
    {
        return _teamIndex;
    }
}
