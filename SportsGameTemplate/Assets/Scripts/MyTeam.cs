using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyTeam : MonoBehaviour
{
    [SerializeField] string teamName;
    [SerializeField] List<Player> _players;

    private void Start()
    {
        _players = new SquadCreator().CreateSquad(80);
    }
}
