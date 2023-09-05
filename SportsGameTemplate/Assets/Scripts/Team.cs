using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Team
{
    [SerializeField] int _teamID;
    [SerializeField] string _teamName;
    [SerializeField] int _teamRating;
    [SerializeField] List<Player> _players;
    [SerializeField] List<DraftPick> _draftPicks;

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
}
