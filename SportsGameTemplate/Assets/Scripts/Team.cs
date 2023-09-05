using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Team
{
    [SerializeField] int _teamID;
    [SerializeField] string _teamName;
    [SerializeField] int _teamRating;
    List<Player> _players;
    List<DraftPick> _draftPicks;
    [SerializeField] private List<int> _matchdays;
    private List<int> _availableMatchdays;

    public Team(int id, string name, int rating, List<Player> players)
    {
        _teamID = id;
        _teamName = name;
        _teamRating = rating;
        _players = players;
        _draftPicks = new();
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

    public List<int> GetMatchdays()
    {
        if (_matchdays != null)
            return _matchdays;

        return new List<int>();
    }

    public int GetFirstAvailableMatchday()
    {
        for (int i = 0; i < ConfigManager.Instance.GetCurrentConfig().GamesPerTeamInRegularSeason; i++)
        {
            if (_matchdays.Contains(i))
            {
                continue;
            } else
            {
                return i;
            }
        }

        return 0;
    }

    public List<int> GetAvailableMatchdays()
    {
        if (_availableMatchdays == null)
        {
            _availableMatchdays = new List<int>();

            for (int i = 0; i < ConfigManager.Instance.GetCurrentConfig().GamesPerTeamInRegularSeason * 1.2f; i++)
            {
                _availableMatchdays.Add(i);
            }
        }

        return _availableMatchdays;
    }

    public void AddMatchdayAsTaken(int week)
    {
        _availableMatchdays.Remove(week);

        if (_matchdays == null)
            _matchdays = new List<int>();

        _matchdays.Add(week);
    }
}
