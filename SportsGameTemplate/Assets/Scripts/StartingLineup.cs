using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class StartingLineup
{
    [SerializeField] string _startingPointGuard = "";
    [SerializeField] string _startingShootingGuard = "";
    [SerializeField] string _startingCenter = "";
    [SerializeField] string _startingSmallForward = "";
    [SerializeField] string _startingPowerForward = "";

    public StartingLineup()
    {
        _startingPointGuard = "";
        _startingShootingGuard = "";
        _startingCenter = "";
        _startingSmallForward = "";
        _startingPowerForward = "";

    }

    public StartingLineup (List<Player> players)
    {
        _startingPointGuard = GetBestPlayerFromPosition(players, "Point Guard").GetTradeableID();
        _startingShootingGuard = GetBestPlayerFromPosition(players, "Shooting Guard").GetTradeableID();
        _startingCenter = GetBestPlayerFromPosition(players, "Center").GetTradeableID();
        _startingSmallForward = GetBestPlayerFromPosition(players, "Small Forward").GetTradeableID();
        _startingPowerForward = GetBestPlayerFromPosition(players, "Power Forward").GetTradeableID();
    }

    public StartingLineup(List<Player> players, bool myTeam)
    {
        _startingPointGuard = ChoosePlayerWithMinutes(players, "Point Guard").GetTradeableID();
        _startingShootingGuard = ChoosePlayerWithMinutes(players, "Shooting Guard").GetTradeableID();
        _startingCenter = ChoosePlayerWithMinutes(players, "Center").GetTradeableID();
        _startingSmallForward = ChoosePlayerWithMinutes(players, "Small Forward").GetTradeableID();
        _startingPowerForward = ChoosePlayerWithMinutes(players, "Power Forward").GetTradeableID();
    }

    public List<string> GetStartingLineup()
    {
        return new List<string>() { _startingPowerForward, _startingSmallForward, _startingShootingGuard, _startingPointGuard, _startingCenter };
    }

    private Player GetBestPlayerFromPosition(List<Player> players, string position)
    {
        List<Player> playersFromPosition = players.Where(x => x.GetPosition() == position).ToList();
        
        if (playersFromPosition.Count > 0)
        {
            Player selectedPlayer = playersFromPosition.OrderByDescending(x => x.CalculateRatingForPosition() * UnityEngine.Random.Range(0.9f, 1.1f)).ToList()[0];
            return selectedPlayer;
        }
        else
        {
            return players.OrderByDescending(x => x.CalculateRatingForPosition()).ToList().Last();
        }
    }

    private Player ChoosePlayerWithMinutes(List<Player> players, string position)
    {
        List<Player> playersFromPosition = players.Where(x => x.GetPosition() == position).ToList();

        int totalMinutes = 0;
        playersFromPosition.ForEach(x => totalMinutes += x.GetMinutes());

        int random = UnityEngine.Random.Range(0, totalMinutes);

        foreach (Player player in playersFromPosition)
        {
            if (random < player.GetMinutes())
            {
                return player;
            } else
            {
                random -= player.GetMinutes();
            }
        }

        return playersFromPosition[0];
    }

    public void SetPosition(string playerID, string position)
    {
        switch (position)
        {
            case "Point Guard":
                _startingPointGuard = playerID;
                break;
            case "Shooting Guard":
                _startingShootingGuard = playerID;
                break;
            case "Power Forward":
                _startingPowerForward = playerID;
                break;
            case "Center":
                _startingCenter = playerID;
                break;
            case "Small Forward":
                _startingSmallForward = playerID;
                break;
            default:
                break;
        }
    }
}
