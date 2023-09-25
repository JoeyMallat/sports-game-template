using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Extensions
{
    public static void RemoveByPlayerID(this List<Player> players, string playerID)
    {
        List<Player> playersToRemove = players.Where(x => x.GetPlayerID() == playerID).ToList();

        if (playersToRemove.Any())
        {
            players.Remove(playersToRemove[0]);
        } else
        {
            Debug.LogWarning($"No player found with id {playersToRemove[0].GetPlayerID()}");
        }
    }

    public static void RemoveByPickID(this List<DraftPick> picks, string pickID)
    {
        List<DraftPick> picksToRemove = picks.Where(x => x.GetPickID() == pickID).ToList();

        if (picksToRemove.Any())
        {
            picks.Remove(picksToRemove[0]);
        }
        else
        {
            Debug.LogWarning($"No draft pick found with id {picksToRemove[0].GetPickID()}");
        }
    }


    public static List<T> Shuffle<T>(this IList<T> list)
    {
        System.Random rng = new System.Random();

        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list.ToList();
    }

    public static string ConvertToMonetaryString(this int number)
    {
        string character = "M";
        if (number < 1000000)
            character = "K";

        number = Mathf.RoundToInt(number / 1000000f);
        return $"{number.ToString("F1")}{character}";
    }
}
