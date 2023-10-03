using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamAssets : MonoBehaviour
{
    [SerializeField] int _teamIndex;
    int _teamID;
    [SerializeField] TextMeshProUGUI _teamName;
    List<TeamAsset> _teamAssets;

    private void Awake()
    {
        TradingSystem.OnAssetsUpdated += UpdateTeamAssets;
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
        _teamName.text = LeagueSystem.Instance.GetTeam(_teamID).GetTeamName();

        int tradeAssetsAmount = tradeAssets.Count;

        for (int i = 0; i < _teamAssets.Count; i++)
        {
            if (i < tradeAssetsAmount)
            {
                _teamAssets[i].SetAssetDetails(tradeAssets[i]);
            } else
            {
                _teamAssets[i].SetAssetDetails();
            }
        }
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
