using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class Team
{
    [SerializeField] int _teamID;
    [SerializeField] string _teamName;
    [SerializeField] int _teamRating;
    [InfoBox("@GetTotalSalaryAmount().ToString(\"C0\")")][SerializeField] List<Player> _players;
    List<DraftPick> _draftPicks;
    List<int> _matchdays;
    List<int> _availableMatchdays;

    public Team(int id, string name, int rating, List<Player> players)
    {
        _teamID = id;
        _teamName = name;
        _teamRating = rating;
        _players = players;
        _draftPicks = new();
        _matchdays = new List<int>();
        InitializeAvailableMatchdays();
    }

    public List<Player> GetPlayersFromTeam()
    {
        return _players;
    }

    public int GetTeamID()
    {
        return _teamID;
    }

    public string GetTeamName()
    {
        return _teamName;
    }

    public void RemovePlayer(Player player)
    {
        _players.RemoveByPlayerID(player.GetPlayerID());
    }

    public void AddPlayer(Player player)
    {
        _players.Add(player);
    }

    public void RemoveDraftPick(DraftPick draftPick)
    {
        _draftPicks.RemoveByPickID(draftPick.GetPickID());
    }

    public void AddDraftPick(DraftPick draftPick)
    {
        _draftPicks.Add(draftPick);
    }

    public List<DraftPick> GetDraftPicks()
    {
        return _draftPicks;
    }

    public List<ITradeable> GetTradeAssets()
    {
        List<ITradeable> assets = new List<ITradeable>();
        assets.AddRange(_draftPicks);
        assets.AddRange(_players);
        return assets;
    }

    public List<int> GetAvailableMatchdays()
    {
        return _availableMatchdays;
    }

    private void InitializeAvailableMatchdays()
    {
        _availableMatchdays = new List<int>();

        for (int i = 0; i < ConfigManager.Instance.GetCurrentConfig().GamesPerTeamInRegularSeason * 2.5f; i++)
        {
            _availableMatchdays.Add(i);
        }
    }

    public void AddMatchdayAsTaken(int week)
    {
        if (_availableMatchdays.Contains(week))
        {
            _availableMatchdays.Remove(week);
        }

        _matchdays.Add(week);
    }

    public int GetMatchdayCount()
    {
        return _matchdays.Count;
    }

    public int GetTotalSalaryAmount()
    {
        int total = 0;
        foreach (Player player in _players)
        {
            total += player.GetContract().GetYearlySalary();
        }

        return total;
    }
}
