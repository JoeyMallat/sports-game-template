using System;
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

            for (int j = 0; j < amount; j++)
            {
                // Get random name from list of names
                string firstName = GetRandomNameFromList(Resources.Load<TextAsset>("Names/first_names"));
                string lastName = GetRandomNameFromList(Resources.Load<TextAsset>("Names/last_names"));

                // Create player with position y
                Player newPlayer = new Player(firstName, lastName, position.GetPositionName());
                players.Add(newPlayer);
                newPlayer.SetRandomSkills(rating, position.GetPositionStats());
            }
        }

        return players;
    }

    private string GetRandomNameFromList(TextAsset nameList)
    {
        string[] names = nameList.text.Split('\n');
        return names[UnityEngine.Random.Range(0, names.Length)];
    }
}
