using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    public void SetSlot(OwnedGameItem item)
    {
        if (item != null)
        {
            _usedSlotOverlay.SetActive(true);
            _unusedSlotOverlay.SetActive(false);

            GameItem itemDetails = ItemDatabase.Instance.GetGameItemByID(item.GetItemID());
            _itemImage.sprite = itemDetails.GetItemImage();
            _itemName.text = itemDetails.GetItemName();

            _itemDetails.text = "";
            foreach (SkillBoost boost in itemDetails.GetSkillBoosts())
            {
                _itemDetails.text += $"<color=\"white\">+{boost.GetBoost()} {boost.GetSkill()}</color>\n";
            }

            _gamesRemaining.text = $"{item.GetGamesRemaining()} games left";
        } else
        {
            _usedSlotOverlay.SetActive(false);
            _unusedSlotOverlay.SetActive(true);
            _addItemText.text = $"Add {_itemTypeSlot}";
        }
    }

    public ItemType GetItemType()
    {
        return _itemTypeSlot;
    }
}
