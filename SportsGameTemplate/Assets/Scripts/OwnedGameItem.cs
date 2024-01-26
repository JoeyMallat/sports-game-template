using UnityEngine;

[System.Serializable]
public class OwnedGameItem
{
    [SerializeField] int _itemID;
    [SerializeField] int _gamesRemaining;
    [SerializeField] int _amountInInventory;

    public OwnedGameItem(int id, int gamesRemaining, int amount)
    {
        _itemID = id;
        _gamesRemaining = gamesRemaining;
        _amountInInventory = amount;
    }

    public int GetItemID()
    {
        return _itemID;
    }

    public int GetGamesRemaining()
    {
        return _gamesRemaining;
    }

    public int GetAmountInInventory()
    {
        return _amountInInventory;
    }

    public string GetGamesRemainingString()
    {
        return $"<sprite name=\"Time\"> {_gamesRemaining} games";
    }

    public void UpdateAmount(int amountAdded)
    {
        _amountInInventory += amountAdded;
    }

    public void DecreaseGamesRemaining()
    {
        _gamesRemaining--;
    }
}
