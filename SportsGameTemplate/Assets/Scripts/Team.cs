using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Team
{
    [SerializeField] int _teamID;
    [SerializeField] string _teamName;
    [SerializeField] int _teamRating;
    [SerializeField] List<Player> _players;

    public Team(int id, string name, int rating, List<Player> players)
    {
        _teamID = id;
        _teamName = name;
        _teamRating = rating;
        _players = players;
    }
}
