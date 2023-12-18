using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryGameItem : MonoBehaviour
{
    [SerializeField] Image _itemImage;
    [SerializeField] TextMeshProUGUI _itemName;
    [SerializeField] TextMeshProUGUI _itemBoosts;
    [SerializeField] TextMeshProUGUI _gamesRemaining;
    [SerializeField] TextMeshProUGUI _amountInInventory;
    [SerializeField] GameObject _amountInInventoryObject;
    [SerializeField] Button _assignButton;

    public void SetItem(Player player, OwnedGameItem item)
    {
        GameItem details = ItemDatabase.Instance.GetGameItemByID(item.GetItemID());

        _itemImage.sprite = details.GetItemImage();
        _itemName.text = details.GetItemName();
        _itemBoosts.text = details.GetSkillBoostsString();
        _gamesRemaining.text = item.GetGamesRemainingString();

        if (item.GetAmountInInventory() > 1)
        {
            _amountInInventoryObject.SetActive(true);
            _amountInInventory.text = $"{item.GetAmountInInventory()}x";
        } else
        {
            _amountInInventoryObject.SetActive(false);
        }

        _assignButton.gameObject.SetActive(true);

        _assignButton.onClick.RemoveAllListeners();
        _assignButton.onClick.AddListener(() => player.AssignItem(item));
        _assignButton.onClick.AddListener(() => GameManager.Instance.RemoveItem(item));
        _assignButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(false, CanvasKey.MainMenu, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID())));
        _assignButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Team, LeagueSystem.Instance.GetTeam(GameManager.Instance.GetTeamID())));
        _assignButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.Player, player));
    }

    public void SetItem(OwnedGameItem item)
    {
        GameItem details = ItemDatabase.Instance.GetGameItemByID(item.GetItemID());

        _itemImage.sprite = details.GetItemImage();
        _itemName.text = details.GetItemName();
        _itemBoosts.text = details.GetSkillBoostsString();
        _gamesRemaining.text = item.GetGamesRemainingString();

        if (item.GetAmountInInventory() > 1)
        {
            _amountInInventoryObject.SetActive(true);
            _amountInInventory.text = $"{item.GetAmountInInventory()}x";
        }
        else
        {
            _amountInInventoryObject.SetActive(false);
        }

        _assignButton.gameObject.SetActive(false);
    }
}
