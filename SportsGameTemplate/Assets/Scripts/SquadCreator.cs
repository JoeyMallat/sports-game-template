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
                // Create player with position y
                int randomizedRating = UnityEngine.Random.Range(rating - 10, rating + 10);
                Player newPlayer = CreatePlayer(false, position, randomizedRating);
                players.Add(newPlayer);
            }
        }

        return players;
    }

    public List<Player> CreateDraftClass(int rating)
    {
        List<Player> draftClass = new();
        List<Position> positions = ConfigManager.Instance.GetCurrentConfig().PositionConfig.GetPositions();
        int playersInDraftClass = ConfigManager.Instance.GetCurrentConfig().DraftRounds * ConfigManager.Instance.GetCurrentConfig().PlayersPerDraftRound;
        for (int i = 0; i < playersInDraftClass; i++)
        {
            Player player = CreatePlayer(true, positions[UnityEngine.Random.Range(0, positions.Count)], rating);
            draftClass.Add(player);
        }

        return draftClass;
    }

    private Player CreatePlayer(bool draft, Position position, int rating)
    {
        // Get random name from list of names
        string firstName = GetRandomNameFromList(Resources.Load<TextAsset>("Names/first_names"));
        string lastName = GetRandomNameFromList(Resources.Load<TextAsset>("Names/last_names"));

        if (draft) { return new Player(draft, firstName, lastName, position, rating); }
        else { return new Player(firstName, lastName, position, rating); }
    }

    private string GetRandomNameFromList(TextAsset nameList)
    {
        string[] names = nameList.text.Split('\n');
        return names[UnityEngine.Random.Range(0, names.Length)];
    }
}
