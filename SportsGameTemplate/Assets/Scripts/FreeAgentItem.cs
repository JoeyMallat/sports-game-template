using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FreeAgentItem : MonoBehaviour
{
    [Header("Player Asset")]
    [SerializeField] Image _playerPortrait;
    [SerializeField] TextMeshProUGUI _nameText;
    [SerializeField] TextMeshProUGUI _positionText;
    [SerializeField] TextMeshProUGUI _ratingText;
    [SerializeField] TextMeshProUGUI _ageText;
    [SerializeField] TextMeshProUGUI _salaryText;
    [SerializeField] Button _button;

    public void SetPlayerAssets(Player player)
    {
        _playerPortrait.sprite = player.GetPlayerPortrait();
        _nameText.text = player.GetFullName();
        _positionText.text = player.GetPosition();

        _ratingText.text = player.CalculateRatingForPosition().ToString();
        _ageText.text = player.GetAge().ToString();
        _salaryText.text = player.GetContract().GetYearlySalary().ConvertToMonetaryString();
        SetButton(player);
    }

    private void SetButton(Player player)
    {
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Player, player));
    }
}
