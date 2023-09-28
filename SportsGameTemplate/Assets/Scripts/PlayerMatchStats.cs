using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerMatchStats
{
    [SerializeField] int _minutes;
    [SerializeField] int _assists;
    [SerializeField] int _steals;
    [SerializeField] int _rebounds;
    [SerializeField] int _blocks;
    [SerializeField] int _freeThrowsAttempted;
    [SerializeField] int _freeThrowsMade;
    [SerializeField] int _twoPointersAttempted;
    [SerializeField] int _twoPointersMade;
    [SerializeField] int _threePointersAttempted;
    [SerializeField] int _threePointersMade;

    public PlayerMatchStats()
    {
        _minutes = 0;
        _assists = 0;
        _steals = 0;
        _rebounds = 0;
        _blocks = 0;
        _freeThrowsAttempted = 0;
        _freeThrowsMade = 0;
        _twoPointersAttempted = 0;
        _twoPointersMade = 0;
        _threePointersAttempted = 0;
        _threePointersMade = 0;
    }

    public PlayerMatchStats(int minutes, int assists, int steals, int rebounds, int blocks, int ftAttempts, int ftMade, int twoAttempts, int twoMade, int threeAttempts, int threeMade)
    {
        _minutes = minutes;
        _assists = assists;
        _steals = steals;
        _rebounds = rebounds;
        _blocks = blocks;
        _freeThrowsAttempted = ftAttempts;
        _freeThrowsMade = ftMade;
        _twoPointersAttempted = ftAttempts;
        _twoPointersMade = ftMade;
        _threePointersAttempted = threeAttempts;
        _threePointersMade = threeMade;
    }

    public int GetPoints()
    {
        return _freeThrowsMade + (_twoPointersMade * 2) + (_threePointersMade * 3);
    }

    public int GetAssists()
    {
        return _assists;
    }

    public int GetRebounds()
    {
        return _rebounds;
    }
}
