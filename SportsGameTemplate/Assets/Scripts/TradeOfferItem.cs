using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

public class TradeOfferItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _tradeOfferText;
    [SerializeField] Button _tradeOfferButton;

    public static event Action<bool> OnNewTradeOpened;

    public void SetTradeOffer((TradeOffer, string) tradeOffer)
    {
        Team offeringTeam = LeagueSystem.Instance.GetTeam(tradeOffer.Item1.GetOfferingTeamID());
        if (tradeOffer.Item1.GetAssets().Item1.Count > 1)
        {
            _tradeOfferText.text = $"{offeringTeam.GetTeamName()} trade offer for {LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()).GetPlayersFromTeam().Where(x => x.GetTradeableID() == tradeOffer.Item2).ToList()[0].GetFullName()} + more";
        } else
        {
            _tradeOfferText.text = $"{offeringTeam.GetTeamName()} offer for {LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()).GetPlayersFromTeam().Where(x => x.GetTradeableID() == tradeOffer.Item2).ToList()[0].GetFullName()}";
        }

        SetButton(tradeOffer.Item1);
    }

    delegate void AddToTradeDelegate(TradeOffer tradeOffer);

    private void SetButton(TradeOffer tradeOffer)
    {
        AddToTradeDelegate onClickAction;

        onClickAction = AddAssetsToTrade;

        _tradeOfferButton.onClick.RemoveAllListeners();
        // Remove trade offer from player when clicked on trade offer
        _tradeOfferButton.onClick.AddListener(() => tradeOffer.GetAssets().Item1[0].RemoveTradeOffer(tradeOffer));
        _tradeOfferButton.onClick.AddListener(() => OnNewTradeOpened?.Invoke(true));
        _tradeOfferButton.onClick.AddListener(() => onClickAction(tradeOffer));
    }

    private void AddAssetsToTrade(TradeOffer tradeOffer)
    {
        foreach (ITradeable asset in tradeOffer.GetAssets().Item1)
        {
            asset.AddToTrade(GameManager.Instance.GetTeamID());
        }

        foreach (ITradeable asset in tradeOffer.GetAssets().Item2)
        {
            asset.AddToTrade(tradeOffer.GetOfferingTeamID());
        }
    }
}
