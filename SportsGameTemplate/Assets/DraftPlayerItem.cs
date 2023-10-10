using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DraftPlayerItem : MonoBehaviour
{
    [Header("Player Asset")]
    [SerializeField] Image _playerPortrait;
    [SerializeField] TextMeshProUGUI _nameText;
    [SerializeField] TextMeshProUGUI _positionText;
    [SerializeField] TextMeshProUGUI _ratingText;
    [SerializeField] TextMeshProUGUI _ageText;
    [SerializeField] TextMeshProUGUI _potentialText;
    [SerializeField] Button _button;

    public void SetPlayerAssets(Player player)
    {
        _playerPortrait.sprite = player.GetPlayerPortrait();
        _nameText.text = player.GetFullName();
        _positionText.text = player.GetPosition();

        UnityEngine.Random.InitState(player.GetFullName().GetHashCode());

        if (player.GetScoutingPercentage() < 0.2f)
        {
            _ratingText.text = "?";
        } else
        {
            int trueRating = player.CalculateRatingForPosition();
            int minRating = trueRating + Mathf.RoundToInt(UnityEngine.Random.Range(-10 * (1f - player.GetScoutingPercentage()), 0));
            int maxRating = trueRating + Mathf.RoundToInt(UnityEngine.Random.Range(1, 10 * (1f - player.GetScoutingPercentage())));
            _ratingText.text = $"{minRating} - {maxRating}";
        }

        _ageText.text = player.GetAge().ToString();
        _potentialText.text = player.GetPotential().GetPotentialRange(player.GetScoutingPercentage(), player.GetFullName().GetHashCode());
        SetButton(player);
    }

    private void SetButton(Player player)
    {
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Player, player));
    }
}
