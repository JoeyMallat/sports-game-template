using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public static class Extensions
{
    public static void RemoveByPlayerID(this List<Player> players, string playerID)
    {
        List<Player> playersToRemove = players.Where(x => x.GetTradeableID() == playerID).ToList();

        if (playersToRemove.Any())
        {
            players.Remove(playersToRemove[0]);
        } else
        {
            Debug.LogWarning($"No player found with id {playersToRemove[0].GetTradeableID()}");
        }
    }

    public static void RemoveByPickID(this List<DraftPick> picks, string pickID)
    {
        List<DraftPick> picksToRemove = picks.Where(x => x.GetTradeableID() == pickID).ToList();

        if (picksToRemove.Any())
        {
            picks.Remove(picksToRemove[0]);
        }
        else
        {
            Debug.LogWarning($"No draft pick found with id {picksToRemove[0].GetTradeableID()}");
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

        float floatNumber = (float)Mathf.RoundToInt((float)number / 100000f) / 10f; 
        return $"{floatNumber.ToString("F1")}{character}";
    }

    public static string ConvertToMonetaryString(this float number)
    {
        string character = "M";
        if (number < 1000000)
            character = "K";

        number = (float)(Mathf.RoundToInt(number / 100000f) / 10f);
        return $"{number.ToString("F1")}{character}";
    }

    public static Vector2 GetSnapToPositionToBringChildIntoView(this ScrollRect instance, RectTransform child)
    {
        Canvas.ForceUpdateCanvases();
        Vector2 viewportLocalPosition = instance.viewport.localPosition;
        Vector2 childLocalPosition = child.localPosition;
        Vector2 result = new Vector2(
            0 - (viewportLocalPosition.x + childLocalPosition.x),
            childLocalPosition.y
        );
        return result;
    }

    public static string GetPotentialRange(this Potential potential, float accuracy, int seed)
    {
        if (accuracy == 0)
        {
            return "?";
        }

        if (accuracy < 100)
        {
            UnityEngine.Random.InitState(seed);
            potential -= UnityEngine.Random.Range(Mathf.RoundToInt(-1f / accuracy), (Mathf.RoundToInt(1 * accuracy)));
        }

        switch (potential)
        {
            case Potential.Elite:
                return "A+";
            case Potential.WorldClass:
                return "B-A";
            case Potential.Starter:
                return "C-B";
            case Potential.Bench:
                return "D-C";
            case Potential.Filler:
                return "F";
            default:
                return "?";
        }
    }

    public static (int, int) GetRatingRangeNumbers(this int rating, float accuracy, int seed)
    {
        UnityEngine.Random.InitState(seed);

        int minRating = rating + Mathf.RoundToInt(UnityEngine.Random.Range(-10 * (1f - accuracy), 0));
        int maxRating = rating + Mathf.RoundToInt(UnityEngine.Random.Range(1, 10 * (1f - accuracy)));
        return (minRating, maxRating);
    }

    public static string GetRatingRange(this int rating, float accuracy, int seed)
    {
        if (accuracy < 0.2f)
        {
            return "?";
        }
        else if (accuracy == 1)
        {
            return rating.ToString();
        }
        else
        {
            return $"{rating.GetRatingRangeNumbers(accuracy, seed).Item1} - " + $"{rating.GetRatingRangeNumbers(accuracy, seed).Item2}";
        }
    }

    public static string GetMediaExpectation(this int index)
    {
        switch (index)
        {
            case < 1:
                return "Championship favorite";
            case < 6:
                return "Championship contender";
            case < 16:
                return "Playoff team";
            case < 25:
                return "Mid-tier contender";
            default:
                return "Underdog challenger";
        }
    }
}
