using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DraftPick : ITradeable
{
    [SerializeField][ReadOnly] string _pickID;
    [SerializeField][ReadOnly] int _tradeValue;
    [SerializeField] int _round;
    [SerializeField] int _pickNumber;
    // TODO: Add season to draft pick to add future draft picks

    public DraftPick(int round, int number)
    {
        _pickID = RandomIDGenerator.GenerateRandomID();

        _round = round;
        _pickNumber = number;

        CalculateTradeValue();
    }

    private int GetTotalPickNumber()
    {
        return (_round - 1) * ConfigManager.Instance.GetCurrentConfig().PlayersPerDraftRound + _pickNumber;
    }

    private int GetFirstPickValue()
    {
        return 30 + ConfigManager.Instance.GetCurrentConfig().PlayersPerDraftRound * ConfigManager.Instance.GetCurrentConfig().DraftRounds * 30;
    }

    public int CalculateTradeValue()
    {
        _tradeValue = GetFirstPickValue() - (GetTotalPickNumber() * 30);
        return _tradeValue;
    }

    public string GetPickID()
    {
        return _pickID;
    }
}
