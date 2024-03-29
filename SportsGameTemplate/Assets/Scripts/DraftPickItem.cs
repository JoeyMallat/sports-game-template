using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DraftPickItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _pickRoundAndNumber;
    [SerializeField] TextMeshProUGUI _pickSeason;
    [SerializeField] Button _addToTradeButton;
    public void SetPickDetails(DraftPick pick, int teamID)
    {
        _pickRoundAndNumber.text = pick.GetPickDataString();
        _addToTradeButton.onClick.RemoveAllListeners();
        _addToTradeButton.onClick.AddListener(() => pick.AddToTrade(teamID));
    }
}
