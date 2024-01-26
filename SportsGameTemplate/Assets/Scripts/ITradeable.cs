using System.Collections.Generic;

public interface ITradeable
{
    public int CalculateTradeValue();
    public string GetTradeableID();

    public void AddToTrade(int teamID = 0);
    public void AddTradeOffer(TradeOffer tradeOffer);
    public List<TradeOffer> GetTradeOffers();
    public void RemoveTradeOffer(TradeOffer tradeOffer);
    void RemoveTradeOffers();
}
