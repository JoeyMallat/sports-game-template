using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPreviewItem : MonoBehaviour
{
    [SerializeField] Image _itemImage;

    public void SetItemDetails(Sprite itemImage)
    {
        _itemImage.sprite = itemImage;
    }
}
