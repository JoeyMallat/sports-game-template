using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeOfferItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _tradeOfferText;
    [SerializeField] Button _tradeOfferButton;

    public static event Action<bool> OnNewTradeOpened;
    public static event Action OnTradeOfferOpened;

    public void SetTradeOffer((TradeOffer, string) tradeOffer)
    {
        Team offeringTeam = LeagueSystem.Instance.GetTeam(tradeOffer.Item1.GetOfferingTeamID());
        if (tradeOffer.Item1.GetAssets().Item1.Count > 1)
        {
            List<ITradeable> assets = LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()).GetTradeAssets().Where(x => x.GetTradeableID() == tradeOffer.Item2).ToList();
            if (assets[0].GetType() == typeof(Player))
            {
                _tradeOfferText.text = $"{offeringTeam.GetTeamName()} trade offer for {(assets[0] as Player).GetFullName()} + more";
            }
            else
            {
                _tradeOfferText.text = $"{offeringTeam.GetTeamName()} trade offer for pick #{(assets[0] as DraftPick).GetTotalPickNumber()} + more";
            }
        }
        else
        {
            List<ITradeable> assets = LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()).GetTradeAssets().Where(x => x.GetTradeableID() == tradeOffer.Item2).ToList();
            if (assets[0].GetType() == typeof(Player))
            {
                _tradeOfferText.text = $"{offeringTeam.GetTeamName()} trade offer for {(assets[0] as Player).GetFullName()}";
            }
            else
            {
                _tradeOfferText.text = $"{offeringTeam.GetTeamName()} trade offer for pick #{(assets[0] as DraftPick).GetTotalPickNumber()}";
            }
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
        _tradeOfferButton.onClick.AddListener(() => FindFirstObjectByType<MM_TradeView>().SetDetails(LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID())));
        _tradeOfferButton.onClick.AddListener(() => OnNewTradeOpened?.Invoke(true));
        _tradeOfferButton.onClick.AddListener(() => onClickAction(tradeOffer));
        _tradeOfferButton.onClick.AddListener(() => OnTradeOfferOpened?.Invoke());
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
