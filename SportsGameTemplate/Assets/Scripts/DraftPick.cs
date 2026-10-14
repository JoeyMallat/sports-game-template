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
    [SerializeField] int _draftSeason = -1;
    [SerializeField] int _originalTeamID = -1;
    [SerializeField] List<TradeOffer> _tradeOffers;

    public static event Action<int, ITradeable> OnAddedToTrade;
    public static event Action<ITradeable> OnTradeOfferReceived;

    public DraftPick(int round, int number, int draftSeason = -1, int originalTeamID = -1)
    {
        _pickID = RandomIDGenerator.GenerateRandomID();

        _round = round;
        _pickNumber = number;
        _draftSeason = draftSeason;
        _originalTeamID = originalTeamID;

        CalculateTradeValue();

        _tradeOffers = new List<TradeOffer>();
    }

    public int GetDraftSeason()
    {
        if (_draftSeason < 0 && GameManager.Instance != null)
            _draftSeason = GameManager.Instance.GetCurrentSeason();

        return _draftSeason;
    }

    public int GetOriginalTeamID()
    {
        return _originalTeamID;
    }

    public void SetLegacyMetadata(int draftSeason, int originalTeamID)
    {
        if (_draftSeason < 0)
            _draftSeason = draftSeason;

        if (_originalTeamID < 0)
            _originalTeamID = originalTeamID;
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

    public int CalculateTradeValue()
    {
        TradeValueSettings settings = ConfigManager.Instance.GetCurrentConfig().TradeValueSettings ?? TradeValueSettings.Default;
        Dictionary<int, float> classQuality = DraftSystem.Instance == null ? null : DraftSystem.Instance.GetDraftClassQuality();
        TradeValueContext context = TradeValueContext.Create(LeagueSystem.Instance.GetTeams(), settings, classQuality);
        _tradeValue = DraftPickTradeValuator.Evaluate(this, context, LeagueSystem.Instance.GetTeams()).TotalValue;
        return _tradeValue;
    }

    public TradeValueResult CalculateTradeValue(TradeValueContext context, IReadOnlyList<Team> teams)
    {
        TradeValueResult result = DraftPickTradeValuator.Evaluate(this, context, teams);
        _tradeValue = result.TotalValue;
        return result;
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
