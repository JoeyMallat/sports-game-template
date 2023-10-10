using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DraftClass
{
    [SerializeField] List<Player> _playersInDraftClass;

    public DraftClass(int rating)
    {
        SquadCreator squadCreator = new SquadCreator();

        _playersInDraftClass = squadCreator.CreateDraftClass(rating);
        _playersInDraftClass = _playersInDraftClass.OrderByDescending(x => x.CalculateTradeValue() * UnityEngine.Random.Range(0.8f, 1.2f)).ToList();
    }

    public List<Player> GetPlayers()
    {
        return _playersInDraftClass;
    }

    public Player PickPlayerAtID(int id, Team team, int pickNumber)
    {
        Player chosenPlayer = _playersInDraftClass[id];
        _playersInDraftClass.RemoveAt(id);
        team.AddPlayer(chosenPlayer, pickNumber);
        return chosenPlayer;
    }
}
