using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class DraftPick : ITradeable
{
    [SerializeField][ReadOnly] string _pickID;
    [SerializeField][ReadOnly] int _tradeValue;
    [SerializeField] int _round;
    [SerializeField] int _pickNumber;
    [SerializeField] List<TradeOffer> _tradeOffers;

    public static event Action<int, ITradeable> OnAddedToTrade;
    public static event Action<ITradeable> OnTradeOfferReceived;

    public DraftPick(int round, int number)
    {
        _pickID = RandomIDGenerator.GenerateRandomID();

        _round = round;
        _pickNumber = number;

        CalculateTradeValue();

        _tradeOffers = new List<TradeOffer>();
    }

    public int GetTotalPickNumber()
    {
        return (_round - 1) * ConfigManager.Instance.GetCurrentConfig().PlayersPerDraftRound + _pickNumber;
    }

    public (int, int) GetPickData()
    {
        return (_round, _pickNumber);
    }

    public string GetPickDataString()
    {
        return $"Round {_round} - Pick {_pickNumber}";
    }

    private int GetFirstPickValue()
    {
        // 450 is the minimum value for the draft picks
        // TODO: Base the value of off the upcoming draft class

        return 450 + ConfigManager.Instance.GetCurrentConfig().PlayersPerDraftRound * ConfigManager.Instance.GetCurrentConfig().DraftRounds * 450;
    }

    public int CalculateTradeValue()
    {
        _tradeValue = GetFirstPickValue() - (GetTotalPickNumber() * 450);
        return Mathf.RoundToInt(Mathf.Clamp(_tradeValue, 0, Mathf.Infinity));
    }

    public string GetTradeableID()
    {
        return _pickID;
    }

    public void AddToTrade(int teamID)
    {
        OnAddedToTrade?.Invoke(teamID, this);
    }

    public void AddTradeOffer(TradeOffer tradeOffer)
    {
        _tradeOffers.Add(tradeOffer);
        OnTradeOfferReceived?.Invoke(this);
    }

    public void RemoveTradeOffers()
    {
        _tradeOffers = new List<TradeOffer>();
    }

    public List<TradeOffer> GetTradeOffers()
    {
        return _tradeOffers;
    }

    public void RemoveTradeOffer(TradeOffer tradeOffer)
    {
        _tradeOffers.Remove(tradeOffer);
    }
}
