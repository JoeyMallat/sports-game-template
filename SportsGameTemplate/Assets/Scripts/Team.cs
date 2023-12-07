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
    [SerializeField][HideInInspector] List<DraftPick> _draftPicks;
    [SerializeField][HideInInspector]  List<int> _matchdays;
    [SerializeField][HideInInspector] List<int> _availableMatchdays;
    [SerializeField][HideInInspector] int _seed;
    [SerializeField] TeamSeason _seasonStats;
    [SerializeField] StartingLineup _startingLineup;

    public Team(int id, string name, int rating, List<Player> players)
    {
        _teamID = id;
        _seed = id;
        _teamName = name;
        _teamRating = rating;
        _players = players;
        _draftPicks = new();
        _matchdays = new List<int>();
        InitializeAvailableMatchdays();

        LeagueSystem.OnRegularSeasonFinished += SetSeed;
        LeagueSystem.OnRegularSeasonFinished += ExtendContracts;
        GameManager.OnNewSeasonStarted += ResetForNewSeason;
        Player.OnContractExpired += RemovePlayersWithoutContract;
    }

    private void ResetForNewSeason()
    {
        _draftPicks = new();
        _matchdays = new List<int>();
        InitializeAvailableMatchdays();
        _seasonStats = new TeamSeason();
    }

    private void RemovePlayersWithoutContract(Player player)
    {
        if (_players.Contains(player))
        {
            _players.Remove(player);
        }
    }

    private void SetSeed(List<Team> teams, SeasonStage seasonStage)
    {
        SetSeed(teams.IndexOf(this) + 1);
    }

    public int GetSeed()
    {
        return _seed;
    }

    public void SetSeed(int seed)
    {
        _seed = seed;
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
        Debug.Log("Player moved!");
        _players.Add(player);
        player.ChangeTeam(_teamID);
    }

    public void AddPlayer(Player player, int pick)
    {
        _players.Add(player);
        player.ChangeTeam(_teamID, pick);
    }

    private void ExtendContracts(List<Team> teams, SeasonStage seasonStage)
    {
        foreach (Player player in _players.Where(x => x.GetContract().GetYearsOnContract() == 1))
        {
            if (UnityEngine.Random.Range(0f, 1f) < 0.6f)
            {
                player.GetContract().SetNewContract(Mathf.RoundToInt(player.GetContract().GetYearlySalary() * UnityEngine.Random.Range(0.5f, 1.5f)), UnityEngine.Random.Range(2, 5));
            }
        }
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

        for (int i = 1; i < ConfigManager.Instance.GetCurrentConfig().GamesPerTeamInRegularSeason * 2.5f; i++)
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
        foreach (ITradeable asset in GetTradeAssets())
        {
            if (asset.GetTradeOffers() == null) continue;

            foreach (TradeOffer tradeOffer in asset.GetTradeOffers())
            {
                tradeOffers.Add((tradeOffer, asset.GetTradeableID()));
            }
        }

        return tradeOffers;
    }

    public void GenerateLineup()
    {
        _startingLineup = new StartingLineup(_players);
    }

    public List<string> GetLineup()
    {
        return _startingLineup.GetStartingLineup();
    }

    public List<Player> GetPlayersFromIDs(List<string> playerIDs)
    {
        List<Player> playerList = new List<Player>();

        foreach (string id in playerIDs)
        {
            playerList.Add(_players.Where(x => x.GetTradeableID() == id).ToList()[0]);
        }

        return playerList;
    }
}
