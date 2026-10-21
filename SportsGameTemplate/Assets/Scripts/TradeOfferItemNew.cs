using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeOfferItemNew : MonoBehaviour
{
    [SerializeField] Image _tradingTeamLogo;
    [SerializeField] TextMeshProUGUI _tradingTeamName;
    [SerializeField] Image _highestRatedPlayerPortrait;
    [SerializeField] TextMeshProUGUI _playerName; // Format: F. Lastname
    [SerializeField] TextMeshProUGUI _playerPosition; // Format: SG
    [SerializeField] TextMeshProUGUI _playerOverall; // Format: 90
    [SerializeField] GameObject _andMoreTag;
    [SerializeField] TextMeshProUGUI _andMoreText; // Format : +3 more
    [SerializeField] Button _goToTradeButton;
    [SerializeField] Button _declineButton;

    TradeOffer _tradeOffer;

    public static event Action<TradeOffer> OnTradeOfferDeclined;

    // Displayed player will always be from our team
    // If only draft picks are in trade, playername will be "Draft Pick", position will be Year and overall will be pick number in format "Pick 30". Portrait can be disabled

    public void SetTradeOffer(TradeOffer tradeOffer)
    {
        _tradeOffer = tradeOffer;
        Team offeringTeam = LeagueSystem.Instance.GetTeam(tradeOffer.GetOfferingTeamID());
        _tradingTeamLogo.sprite = offeringTeam.GetTeamLogo();
        _tradingTeamName.text = offeringTeam.GetTeamName();

        (List<ITradeable> requestedAssets, _) = tradeOffer.GetAssets();
        Player displayedPlayer = requestedAssets.OfType<Player>()
            .OrderByDescending(player => player.CalculateRatingForPosition())
            .FirstOrDefault();

        if (displayedPlayer != null)
            SetPlayer(displayedPlayer);
        else
            SetDraftPick(requestedAssets.OfType<DraftPick>().FirstOrDefault());

        int additionalAssets = Mathf.Max(0, requestedAssets.Count - 1);
        _andMoreTag.SetActive(additionalAssets > 0);
        _andMoreText.text = $"+{additionalAssets} more";

        _goToTradeButton.onClick.RemoveAllListeners();
        _goToTradeButton.onClick.AddListener(OpenTradeOffer);

        if (_declineButton != null)
        {
            _declineButton.onClick.RemoveAllListeners();
            _declineButton.onClick.AddListener(DeclineTradeOffer);
        }
    }

    void SetPlayer(Player player)
    {
        _highestRatedPlayerPortrait.enabled = true;
        _highestRatedPlayerPortrait.sprite = player.GetPlayerPortrait();
        _playerName.text = AbbreviateName(player.GetFullName());
        _playerPosition.text = AbbreviatePosition(player.GetPosition());
        _playerOverall.text = player.CalculateRatingForPosition().ToString();
    }

    void SetDraftPick(DraftPick draftPick)
    {
        _highestRatedPlayerPortrait.enabled = false;
        _playerName.text = "Draft Pick";
        _playerPosition.text = draftPick == null ? string.Empty : $"Year {draftPick.GetDraftSeason()}";
        _playerOverall.text = draftPick == null ? string.Empty : $"Pick {draftPick.GetTotalPickNumber()}";
    }

    void OpenTradeOffer()
    {
        TradingSystem tradingSystem = FindFirstObjectByType<TradingSystem>(FindObjectsInactive.Include);
        if (tradingSystem == null || !tradingSystem.OpenIncomingTradeOffer(_tradeOffer))
            OnTradeOfferDeclined?.Invoke(_tradeOffer);
    }

    void DeclineTradeOffer()
    {
        OnTradeOfferDeclined?.Invoke(_tradeOffer);
    }

    static string AbbreviateName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return string.Empty;

        string[] names = fullName.Split(' ');
        if (names.Length < 2)
            return fullName;

        return names[0][0] + ". " + names[names.Length - 1];
    }

    static string AbbreviatePosition(string position)
    {
        switch (position)
        {
            case "Point Guard": return "PG";
            case "Shooting Guard": return "SG";
            case "Small Forward": return "SF";
            case "Power Forward": return "PF";
            case "Center": return "C";
            default:
                if (string.IsNullOrWhiteSpace(position))
                    return string.Empty;

                string[] words = position.Split(' ');
                if (words.Length == 1)
                    return position.Length <= 3 ? position.ToUpperInvariant() : position.Substring(0, 3).ToUpperInvariant();

                string abbreviation = string.Empty;
                for (int i = 0; i < words.Length; i++)
                    abbreviation += char.ToUpperInvariant(words[i][0]);
                return abbreviation;
        }
    }
}
