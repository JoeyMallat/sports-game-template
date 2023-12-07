using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LineupItem : MonoBehaviour
{
    [SerializeField] string _positionString;

    [SerializeField] GameObject _noPlayerOverlay;
    [SerializeField] GameObject _playerOverlay;

    [SerializeField] Image _playerPortrait;
    [SerializeField] TextMeshProUGUI _playerName;
    [SerializeField] TextMeshProUGUI _playerRating;

    [SerializeField] GameObject _playerMenuOverlay;
    [SerializeField] Button _removeFromLineupButton;
    [SerializeField] Button _AddToLineupButton;
    [SerializeField] TextMeshProUGUI _addToLineUpText;

    public void SetPlayerDetails(Player player)
    {
        if (player == null)
        {
            _noPlayerOverlay.SetActive(true);
            _playerOverlay.SetActive(false);
            SetNoPlayerButton();
            return;
        }

        _noPlayerOverlay.SetActive(false);
        _playerOverlay.SetActive(true);

        _playerPortrait.sprite = player.GetPlayerPortrait();
        _playerName.text = player.GetFullName();
        _playerRating.text = player.CalculateRatingForPosition().ToString();

        ToggleMenuOverlay(false);

        SetButton(player);
    }

    private void SetNoPlayerButton()
    {
        _addToLineUpText.text = $"+\n<size=20%>Add {_positionString}";
        _AddToLineupButton.onClick.RemoveAllListeners();
        //
    }

    private void SetButton(Player player)
    {
        Button button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Player, player));
        }
    }

    public void ToggleMenuOverlay(bool status)
    {
        _playerMenuOverlay.SetActive(status);
    }
}
