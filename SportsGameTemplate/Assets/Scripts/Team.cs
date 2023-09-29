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
    [SerializeField] TeamSeason _seasonStats;

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

    public string GetTeamAbbreviation()
    {
        string[] parts = _teamName.Split(" ");

        string abbreviation = "";

        if (parts.Length == 1)
            return _teamName.Substring(0, 1);

        foreach (string part in parts)
        {
            abbreviation += part.Substring(0, 1);
        }

        return abbreviation;
    }

    public TeamSeason GetCurrentSeasonStats()
    {
        if (_seasonStats == null)
        {
            return _seasonStats = new TeamSeason();
        }
        return _seasonStats;
    }

    public int GetAverageTeamRating()
    {
        int rating = 0;
        foreach (Player player in _players)
        {
            rating += player.CalculateRatingForPosition();
        }

        rating /= _players.Count;

        _teamRating = rating;
        return rating;
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
        _players.RemoveByPlayerID(player.GetTradeableID());
    }

    public void AddPlayer(Player player)
    {
        _players.Add(player);
        player.ChangeTeam(_teamID);
    }

    public void AddPlayer(Player player, int pick)
    {
        _players.Add(player);
        player.ChangeTeam(_teamID, pick);
    }

    public void RemoveDraftPick(DraftPick draftPick)
    {
        _draftPicks.RemoveByPickID(draftPick.GetTradeableID());
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

    public Sprite GetTeamLogo()
    {
        return Resources.Load<Sprite>($"Logos/{_teamID}");
    }

    public List<(TradeOffer, string)> GetAllTradeOffers()
    {
        List<(TradeOffer, string)> tradeOffers = new List<(TradeOffer, string)>();
        foreach (Player player in _players)
        {
            if (player.GetTradeOffers() == null) continue;

            foreach (TradeOffer tradeOffer in player.GetTradeOffers())
            {
                tradeOffers.Add((tradeOffer, player.GetTradeableID()));
            }
        }

        return tradeOffers;
    }

    public void DebugShowStats()
    {
        List<Match> matches = LeagueSystem.Instance.GetMatchesForTeam(this);

        int wins = 0;
        int losses = 0;

        foreach (Match match in matches)
        {
            wins += match.GetWinStatForTeam(_teamID).Item1;
            losses += match.GetWinStatForTeam(_teamID).Item2;
        }

        Debug.Log($"{_teamName} finished the season with a {wins}-{losses} record");
    }
}
