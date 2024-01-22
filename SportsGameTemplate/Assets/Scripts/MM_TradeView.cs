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
    [SerializeField] Button _searchPlayerButton;
    [SerializeField] GameObject _tradeOffersRoot;

    [SerializeField] Transform _tradeOfferPrefabParent;
    [SerializeField] TradeOfferItem _tradeOfferPrefab;

    [SerializeField] TextMeshProUGUI _noPlayersTradingBlockText;
    [SerializeField] TextMeshProUGUI _noTradeOffersText;

    [SerializeField] TextMeshProUGUI _tradeOfferCountText;

    public void SetDetails<T>(T item) where T : class
    {
        Team team = item as Team;

        List<Player> playersOnBlock = team.GetPlayersFromTeam().Where(x => x.GetTradingBlockStatus() == true).Take(5).ToList();

        SetPlayersOnTradingBlock(_tradingBlockRoot.GetComponentsInChildren<PlayerItem>(true).ToList(), playersOnBlock);

        SetTradeOffers(_tradeOffersRoot.GetComponentsInChildren<TradeOfferItem>(true).ToList(), LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID()).GetAllTradeOffers());

        _searchPlayerButton.onClick.RemoveAllListeners();
        _searchPlayerButton.onClick.AddListener(() => { TransitionAnimation.Instance.StartTransition(() => { Navigation.Instance.GoToScreen(true, CanvasKey.PlayerSearch, team); }); });
    }

    private void SetPlayersOnTradingBlock(List<PlayerItem> playerItems, List<Player> playersOnBlock)
    {
        if (playersOnBlock.Count <= 0)
        {
            _noPlayersTradingBlockText.gameObject.SetActive(true);
        } else
        {
            _noPlayersTradingBlockText.gameObject.SetActive(false);
        }

        int index = 0;
        for (int i = 0; i < playerItems.Count; i++)
        {
            if (i < playersOnBlock.Count)
            {
                index = i;
                playerItems[i].gameObject.SetActive(true);
                playerItems[i].SetPlayerDetails(playersOnBlock[index], false, false);
            } else
            {
                playerItems[i].gameObject.SetActive(false);
            }
        }
    }

    private void SetTradeOffers(List<TradeOfferItem> tradeOfferItems, List<(TradeOffer, string)> tradeOffers)
    {
        if (tradeOffers.Count <= 0)
        {
            _noTradeOffersText.gameObject.SetActive(true);
            _tradeOfferCountText.gameObject.SetActive(false);

            foreach (var tradeOfferItem in tradeOfferItems)
            {
                tradeOfferItem.gameObject.SetActive(false);
            }
            return;
        }

        _tradeOfferCountText.gameObject.SetActive(true);
        if (tradeOffers.Count > 9)
        {
            _tradeOfferCountText.text = "9+";
        } else
        {
            _tradeOfferCountText.text = tradeOffers.Count.ToString("F0");
        }

        _noTradeOffersText.gameObject.SetActive(false);

        int index = 0;

        int newInstances = Mathf.Clamp(tradeOffers.Count - tradeOfferItems.Count, 0, 999);

        if (newInstances > 0)
        {
            for (int i = 0; i < newInstances; i++)
            {
                TradeOfferItem tradeOfferItem = Instantiate(_tradeOfferPrefab, _tradeOfferPrefabParent);
                tradeOfferItems.Add(tradeOfferItem);
            }
        }

        for (int i = 0; i < tradeOfferItems.Count; i++)
        {
            if (i < tradeOffers.Count)
            {
                index = i;

                if (tradeOffers[index].Item1.GetAssets().Item1.Count != 0 && tradeOffers[index].Item1.GetAssets().Item2.Count != 0)
                {
                    tradeOfferItems[i].gameObject.SetActive(true);
                    tradeOfferItems[i].SetTradeOffer(tradeOffers[index]);
                } else
                {
                    tradeOfferItems[i].gameObject.SetActive(false);
                }
            } else
            {
                tradeOfferItems[i].gameObject.SetActive(false);
            }
        }
    }
}
