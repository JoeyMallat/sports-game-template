using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OwnedGameItem
{
    [SerializeField] int _itemID;
    [SerializeField] int _gamesRemaining;

    public OwnedGameItem(int id, int gamesRemaining)
    {
        _itemID = id;
        _gamesRemaining = gamesRemaining;
    }

    public int GetItemID()
    {
        return _itemID;
    }

    public int GetGamesRemaining()
    {
        return _gamesRemaining;
    }
}
