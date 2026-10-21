using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TradeOfferView : MonoBehaviour, ISettable
{
    [SerializeField] Transform _tradeOfferItemParent;
    [SerializeField] TradeOfferItemNew _tradeOfferItemPrefab;
    [SerializeField] GameObject _noTradeOffersObject;

    readonly List<TradeOfferItemNew> _tradeOfferItems = new List<TradeOfferItemNew>();

    void Awake()
    {
        Player.OnTradeOfferReceived += HandleTradeOfferReceived;
        DraftPick.OnTradeOfferReceived += HandleTradeOfferReceived;
        TradeOfferItemNew.OnTradeOfferDeclined += DeclineTradeOffer;
        TradingSystem.OnTradeAccepted += RemoveOverlappingTradeOffers;
    }

    void OnDestroy()
    {
        Player.OnTradeOfferReceived -= HandleTradeOfferReceived;
        DraftPick.OnTradeOfferReceived -= HandleTradeOfferReceived;
        TradeOfferItemNew.OnTradeOfferDeclined -= DeclineTradeOffer;
        TradingSystem.OnTradeAccepted -= RemoveOverlappingTradeOffers;
    }

    public void SetDetails<T>(T item) where T : class
    {
        RefreshTradeOffers();
    }

    void HandleTradeOfferReceived(ITradeable asset)
    {
        RefreshTradeOffers();
    }

    void RefreshTradeOffers()
    {
        if (LeagueSystem.Instance == null || GameManager.Instance == null || _tradeOfferItemParent == null)
            return;

        CacheExistingItems();
        List<TradeOffer> tradeOffers = GetUnansweredTradeOffers();
        EnsureItemCount(tradeOffers.Count);

        for (int i = 0; i < _tradeOfferItems.Count; i++)
        {
            bool hasOffer = i < tradeOffers.Count;
            _tradeOfferItems[i].gameObject.SetActive(hasOffer);
            if (hasOffer)
                _tradeOfferItems[i].SetTradeOffer(tradeOffers[i]);
        }

        if (_noTradeOffersObject != null)
            _noTradeOffersObject.SetActive(tradeOffers.Count == 0);
    }

    List<TradeOffer> GetUnansweredTradeOffers()
    {
        Team userTeam = LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID());
        HashSet<string> seenOfferIDs = new HashSet<string>();
        List<TradeOffer> tradeOffers = new List<TradeOffer>();

        foreach ((TradeOffer tradeOffer, string _) in userTeam.GetAllTradeOffers())
        {
            if (tradeOffer == null || !seenOfferIDs.Add(tradeOffer.GetOfferID()))
                continue;

            (List<ITradeable> requestedAssets, List<ITradeable> offeredAssets) = tradeOffer.GetAssets();
            if (requestedAssets.Count > 0 && offeredAssets.Count > 0)
                tradeOffers.Add(tradeOffer);
        }

        return tradeOffers;
    }

    void DeclineTradeOffer(TradeOffer tradeOffer)
    {
        if (tradeOffer == null)
            return;

        RemoveOffersMatching(offer => offer.GetOfferID() == tradeOffer.GetOfferID());
        RefreshTradeOffers();
        SaveResolvedOffers();
    }

    void RemoveOverlappingTradeOffers(IReadOnlyCollection<string> acceptedAssetIDs)
    {
        HashSet<string> assetIDs = acceptedAssetIDs as HashSet<string> ?? new HashSet<string>(acceptedAssetIDs);
        RemoveOffersMatching(offer => offer.ReferencesAnyAsset(assetIDs));
        RefreshTradeOffers();
    }

    void RemoveOffersMatching(System.Predicate<TradeOffer> predicate)
    {
        Team userTeam = LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID());
        List<ITradeable> userAssets = userTeam.GetTradeAssets();

        for (int assetIndex = 0; assetIndex < userAssets.Count; assetIndex++)
        {
            List<TradeOffer> offers = userAssets[assetIndex].GetTradeOffers();
            if (offers == null)
                continue;

            for (int offerIndex = offers.Count - 1; offerIndex >= 0; offerIndex--)
            {
                if (predicate(offers[offerIndex]))
                    userAssets[assetIndex].RemoveTradeOffer(offers[offerIndex]);
            }
        }
    }

    void CacheExistingItems()
    {
        if (_tradeOfferItems.Count > 0)
            return;

        _tradeOfferItems.AddRange(_tradeOfferItemParent.GetComponentsInChildren<TradeOfferItemNew>(true));
    }

    void EnsureItemCount(int requiredCount)
    {
        if (_tradeOfferItemPrefab == null)
            return;

        while (_tradeOfferItems.Count < requiredCount)
            _tradeOfferItems.Add(Instantiate(_tradeOfferItemPrefab, _tradeOfferItemParent));
    }

    void SaveResolvedOffers()
    {
        LocalSaveManager localSaveManager = FindFirstObjectByType<LocalSaveManager>();
        if (localSaveManager != null)
            localSaveManager.SaveGame(false);
    }
}
