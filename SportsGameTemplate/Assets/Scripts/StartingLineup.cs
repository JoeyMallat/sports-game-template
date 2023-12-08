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

    }

    public StartingLineup (List<Player> players)
    {
        _startingPointGuard = GetBestPlayerFromPosition(players, "Point Guard").GetTradeableID();
        _startingShootingGuard = GetBestPlayerFromPosition(players, "Shooting Guard").GetTradeableID();
        _startingCenter = GetBestPlayerFromPosition(players, "Center").GetTradeableID();
        _startingSmallForward = GetBestPlayerFromPosition(players, "Small Forward").GetTradeableID();
        _startingPowerForward = GetBestPlayerFromPosition(players, "Power Forward").GetTradeableID();
    }

    public List<string> GetStartingLineup()
    {
        return new List<string>() { _startingCenter, _startingPowerForward, _startingShootingGuard, _startingSmallForward, _startingPointGuard };
    }

    private Player GetBestPlayerFromPosition(List<Player> players, string position)
    {
        List<Player> playersFromPosition = players.Where(x => x.GetPosition() == position).ToList();
        
        if (playersFromPosition.Count > 0)
        {
            Player selectedPlayer = playersFromPosition.OrderByDescending(x => x.CalculateRatingForPosition() * UnityEngine.Random.Range(0.9f, 1.1f)).ToList()[0];
            return selectedPlayer;
        }

        return null;
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
