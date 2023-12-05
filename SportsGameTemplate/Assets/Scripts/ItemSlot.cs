using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] ItemType _itemTypeSlot;

    [SerializeField] GameObject _usedSlotOverlay;
    [SerializeField] GameObject _unusedSlotOverlay;

    [SerializeField] Image _itemImage;
    [SerializeField] TextMeshProUGUI _itemName;
    [SerializeField] TextMeshProUGUI _itemDetails;
    [SerializeField] TextMeshProUGUI _gamesRemaining;
    [SerializeField] TextMeshProUGUI _addItemText;

    [SerializeField] Button _addItemButton;

    public static event Action<bool, ItemType> OnFilterUpdated;

    public void SetSlot(Player player, OwnedGameItem item)
    {
        if (item != null)
        {
            _usedSlotOverlay.SetActive(true);
            _unusedSlotOverlay.SetActive(false);

            GameItem itemDetails = ItemDatabase.Instance.GetGameItemByID(item.GetItemID());
            _itemImage.sprite = itemDetails.GetItemImage();
            _itemName.text = itemDetails.GetItemName();
            _itemDetails.text = itemDetails.GetSkillBoostsString();
            _gamesRemaining.text = item.GetGamesRemainingString();
        } else
        {
            _usedSlotOverlay.SetActive(false);
            _unusedSlotOverlay.SetActive(true);

            _addItemText.text = $"Add {_itemTypeSlot}";

            _addItemButton.onClick.RemoveAllListeners();
            _addItemButton.onClick.AddListener(() => OnFilterUpdated?.Invoke(true, _itemTypeSlot));
            _addItemButton.onClick.AddListener(() => Navigation.Instance.GoToScreen(true, CanvasKey.ItemInventory, player));
        }
    }

    public ItemType GetItemType()
    {
        return _itemTypeSlot;
    }
}
