using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CloudSaveData
{
    public bool PremiumStatus;
    public int GemAmount;
    public List<CloudInventoryItem> Inventory;

    public CloudSaveData(bool premiumStatus, int gems, List<OwnedGameItem> items)
    {
        PremiumStatus = premiumStatus;
        GemAmount = gems;
        Inventory = new List<CloudInventoryItem>();

        foreach (OwnedGameItem item in items)
        {
            Inventory.Add(new CloudInventoryItem(item));
        }
    }
}

[System.Serializable]
public class CloudInventoryItem
{
    public int ItemID;
    public int GamesRemaining;
    public int AmountInInventory;

    public CloudInventoryItem (OwnedGameItem item)
    {
        ConvertToCloudInventoryItem(item);
    }

    public CloudInventoryItem ConvertToCloudInventoryItem(OwnedGameItem item)
    {
        ItemID = item.GetItemID();
        GamesRemaining = item.GetGamesRemaining();
        AmountInInventory = item.GetAmountInInventory();

        return this;
    }

    public OwnedGameItem ConvertToOwnedGameItem(CloudInventoryItem item)
    {
        return new OwnedGameItem(item.ItemID, item.GamesRemaining, item.AmountInInventory);
    }
}
