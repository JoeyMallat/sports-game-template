using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadCreator
{
    public SquadCreator() { }

    public List<Player> CreateSquad(int rating = 0)
    {
        List<Player> players = new List<Player>();

        // Create x amount of player for each position
        foreach (Position position in ConfigManager.Instance.GetCurrentConfig().PositionConfig.GetPositions())
        {
            int amount = position.GetAmountInStartingTeam();
            Debug.Log(amount);

            for (int j = 0; j < amount; j++)
            {
                // Create player with position y
                Player newPlayer = new Player($"Player_{position.GetPositionName()}_{j}", position.GetPositionName());
                players.Add(newPlayer);
                newPlayer.SetRandomSkills(rating, position.GetPositionStats());
            }
        }

        return players;
    }
}
