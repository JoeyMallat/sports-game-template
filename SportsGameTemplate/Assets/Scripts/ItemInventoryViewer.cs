using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;

public class ItemInventoryViewer : MonoBehaviour, ISettable
{
    [SerializeField] bool _filterActive;
    [SerializeField] ItemType _typeFilter;

    [SerializeField] GameObject _itemRoot;
    [SerializeField] InventoryGameItem _itemPrefab;

    private void Awake()
    {
        ItemSlot.OnFilterUpdated += UpdateFilter;
    }

    private void UpdateFilter(bool filterActive, ItemType itemType)
    {
        _filterActive = filterActive;
        _typeFilter = itemType;
    }

    public void SetDetails<T>(T item) where T : class
    {
        if (item.GetType() == typeof(Player))
        {
            Player player = item as Player;
            ShowItems(player);

        } else
        {
            _filterActive = false;
            Team team = item as Team;
            ShowItems();
        }
    }

    private void ShowItems(Player player)
    {
        List<OwnedGameItem> items = GameManager.Instance.GetItems();

        if (_filterActive)
        {
            items = GameManager.Instance.GetItems().Where(x => ItemDatabase.Instance.GetGameItemByID(x.GetItemID()).GetItemType() == _typeFilter).ToList();
        }

        List<InventoryGameItem> inventoryGameItems = _itemRoot.GetComponentsInChildren<InventoryGameItem>(true).ToList();

        int prefabsToSpawn = items.Count - inventoryGameItems.Count;

        for (int i = 0; i < prefabsToSpawn; i++)
        {
            inventoryGameItems.Add(Instantiate(_itemPrefab, _itemRoot.transform));
        }

        for (int i = 0; i < inventoryGameItems.Count; i++)
        {
            int index = i;

            if (i < items.Count)
            {
                inventoryGameItems[i].gameObject.SetActive(true);
                inventoryGameItems[i].SetItem(player, items[index]);
            } else
            {
                inventoryGameItems[i].gameObject.SetActive(false);
            }
        }
    }

    private void ShowItems()
    {
        List<OwnedGameItem> items = GameManager.Instance.GetItems();

        if (_filterActive)
        {
            items = GameManager.Instance.GetItems().Where(x => ItemDatabase.Instance.GetGameItemByID(x.GetItemID()).GetItemType() == _typeFilter).ToList();
        }

        List<InventoryGameItem> inventoryGameItems = _itemRoot.GetComponentsInChildren<InventoryGameItem>(true).ToList();

        int prefabsToSpawn = items.Count - inventoryGameItems.Count;

        for (int i = 0; i < prefabsToSpawn; i++)
        {
            inventoryGameItems.Add(Instantiate(_itemPrefab, _itemRoot.transform));
        }

        for (int i = 0; i < inventoryGameItems.Count; i++)
        {
            int index = i;

            if (i < items.Count)
            {
                inventoryGameItems[i].gameObject.SetActive(true);
                inventoryGameItems[i].SetItem(items[index]);
            }
            else
            {
                inventoryGameItems[i].gameObject.SetActive(false);
            }
        }
    }
}
