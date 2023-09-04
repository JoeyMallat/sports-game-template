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
        Debug.Log(pickID);
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
}
