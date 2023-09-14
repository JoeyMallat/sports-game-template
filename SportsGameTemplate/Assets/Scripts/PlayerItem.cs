using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerItem : MonoBehaviour
{
    [SerializeField] Image _playerPortrait;
    [SerializeField] TextMeshProUGUI _playerName;
    [SerializeField] TextMeshProUGUI _playerPosition;
    [SerializeField] TextMeshProUGUI _playerRating;

    public void SetPlayerDetails(Player player)
    {
        _playerPortrait.sprite = player.GetPlayerPortrait();
        _playerName.text = player.GetFullName();
        _playerPosition.text = player.GetPosition();
        _playerRating.text = player.CalculateRatingForPosition().ToString();

        SetButton(player);
    }

    private void SetButton(Player player)
    {
        Button button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(() => Debug.Log(player.GetFullName()));
        }
    }
}
