using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class TradeOfferItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _tradeOfferText;
    [SerializeField] Button _tradeOfferButton;

    public void SetTradeOffer((TradeOffer, string) tradeOffer)
    {
        Team offeringTeam = LeagueSystem.Instance.GetTeam(tradeOffer.Item1.GetOfferingTeamID());
        if (tradeOffer.Item1.GetAssets().Count > 1)
        {
            _tradeOfferText.text = $"{offeringTeam.GetTeamName()} trade offer for {LeagueSystem.Instance.GetTeam(0).GetPlayersFromTeam().Where(x => x.GetTradeableID() == tradeOffer.Item2).ToList()[0].GetFullName()} + more";
        } else
        {
            _tradeOfferText.text = $"{offeringTeam.GetTeamName()} offer for {LeagueSystem.Instance.GetTeam(0).GetPlayersFromTeam().Where(x => x.GetTradeableID() == tradeOffer.Item2).ToList()[0].GetFullName()}";
        }
    }
}
