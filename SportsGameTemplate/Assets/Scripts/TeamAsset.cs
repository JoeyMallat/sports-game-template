using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeamAsset : MonoBehaviour
{
    [Header("Empty Asset")]
    [SerializeField] GameObject _emptyAssetOverlay;

    [Header("Player Asset")]
    [SerializeField] GameObject _playerAssetOverlay;
    [SerializeField] Image _playerPortrait;
    [SerializeField] TextMeshProUGUI _nameText;
    [SerializeField] TextMeshProUGUI _positionText;
    [SerializeField] TextMeshProUGUI _ratingText;
    [SerializeField] TextMeshProUGUI _ageText;
    [SerializeField] TextMeshProUGUI _contractYearlyValueText;
    [SerializeField] TextMeshProUGUI _contractLengthText;

    [Header("Pick Asset")]
    [SerializeField] GameObject _pickAssetOverlay;
    [SerializeField] TextMeshProUGUI _pickNumberText;
    [SerializeField] TextMeshProUGUI _pickYearText;

    public void SetAssetDetails(ITradeable asset)
    {
        if (asset.GetType() == typeof(Player))
        {
            SetPlayerAssetDetails(asset as Player);
        } else if (asset.GetType() == typeof(DraftPick))
        {
            SetPickAssetDetails(asset as DraftPick);
        } else
        {
            SetEmptyAssetDetails();
        }
    }

    private void SetEmptyAssetDetails()
    {
        _emptyAssetOverlay.SetActive(true);
        _playerAssetOverlay.SetActive(false);
        _pickAssetOverlay.SetActive(false);
    }

    private void SetPlayerAssetDetails(Player player)
    {
        _emptyAssetOverlay.SetActive(false);
        _playerAssetOverlay.SetActive(true);
        _pickAssetOverlay.SetActive(false);

        _playerPortrait.sprite = player.GetPlayerPortrait();
        _nameText.text = player.GetFullName();
        _positionText.text = player.GetPosition();
        _ratingText.text = player.CalculateRatingForPosition().ToString();
        _contractYearlyValueText.text = player.GetContract().GetYearlySalary().ConvertToMonetaryString();
        _contractLengthText.text = $"{player.GetContract().GetYearsOnContract()} YRS";
    }

    private void SetPickAssetDetails(DraftPick pick)
    {
        _emptyAssetOverlay.SetActive(false);
        _playerAssetOverlay.SetActive(false);
        _pickAssetOverlay.SetActive(true);

        _pickNumberText.text = $"Round {pick.GetPickData().Item1} - Pick {pick.GetPickData().Item2}";
    }
}
