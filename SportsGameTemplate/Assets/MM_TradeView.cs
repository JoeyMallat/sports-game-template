using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System;

public class MM_TradeView : MonoBehaviour, ISettable
{
    [SerializeField] GameObject _tradingBlockRoot;
    [SerializeField] Button _startNewTradeButton;
    [SerializeField] GameObject _tradeOffersRoot;

    public void SetDetails<T>(T item) where T : class
    {
        Team team = item as Team;

        List<Player> playersOnBlock = team.GetPlayersFromTeam().Where(x => x.GetTradingBlockStatus() == true).Take(5).ToList();

        SetPlayersOnTradingBlock(_tradingBlockRoot.GetComponentsInChildren<PlayerItem>().ToList(), playersOnBlock);

        SetTradeOffers(_tradeOffersRoot.GetComponentsInChildren<TradeOfferItem>().ToList(), LeagueSystem.Instance.GetTeam(0).GetAllTradeOffers());
    }

    private void SetPlayersOnTradingBlock(List<PlayerItem> playerItems, List<Player> playersOnBlock)
    {
        for (int i = 0; i < playerItems.Count; i++)
        {
            if (i < playersOnBlock.Count)
            {
                playerItems[i].gameObject.SetActive(true);
                playerItems[i].SetPlayerDetails(playersOnBlock[i]);
            } else
            {
                playerItems[i].gameObject.SetActive(false);
            }
        }
    }

    private void SetTradeOffers(List<TradeOfferItem> tradeOfferItems, List<(TradeOffer, string)> tradeOffers)
    {
        for (int i = 0; i < tradeOffers.Count; i++)
        {
            tradeOfferItems[i].SetTradeOffer(tradeOffers[i]);
        }
    }
}
