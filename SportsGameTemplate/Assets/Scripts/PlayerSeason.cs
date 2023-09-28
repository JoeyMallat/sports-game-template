using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSeason
{
    [SerializeField] int _season;
    [SerializeField] int _teamID;
    [SerializeField] List<PlayerMatchStats> _matchStats;
    
    public PlayerSeason(int season, int teamID)
    {
        _season = season;
        _teamID = teamID;

        _matchStats = new List<PlayerMatchStats>();

        DebugRandomSeason();
    }

    private void DebugRandomSeason()
    {
        for (int i = 0; i < 82; i++)
        {
            int min = UnityEngine.Random.Range(0, 48);
            int ass = UnityEngine.Random.Range(0, 16);
            int stl = UnityEngine.Random.Range(0, 6);
            int reb = UnityEngine.Random.Range(0, 16);
            int blck = UnityEngine.Random.Range(0, 10);
            int ftAttempts = UnityEngine.Random.Range(0, 10);
            int ftMade = UnityEngine.Random.Range(0, ftAttempts);
            int twoAttempts = UnityEngine.Random.Range(0, 20);
            int twoMade = UnityEngine.Random.Range(0, twoAttempts);
            int threeAttempts = UnityEngine.Random.Range(0, 20);
            int threeMade = UnityEngine.Random.Range(0, threeAttempts);
            _matchStats.Add(new PlayerMatchStats(min, ass, stl, reb, blck, ftAttempts, ftMade, twoAttempts, twoMade, threeAttempts, threeMade));
        }
    }

    public void AddMatch(PlayerMatchStats match)
    {
        _matchStats.Add(match);
    }

    public float GetPointsPerGame()
    {
        if (_matchStats.Count == 0) { return 0; }

        float totalPoints = 0;

        foreach (PlayerMatchStats match in _matchStats)
        {
            totalPoints += match.GetPoints();
        }

        float ppg = totalPoints / _matchStats.Count;

        return ppg;
    }

    public float GetAssistsPerGame()
    {
        if (_matchStats.Count == 0) { return 0; }

        float totalAssists = 0;

        foreach (PlayerMatchStats match in _matchStats)
        {
            totalAssists += match.GetAssists();
        }

        float apg = totalAssists / _matchStats.Count;

        return apg;
    }
}
