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
        _playersInDraftClass = _playersInDraftClass.OrderByDescending(x => x.CalculateTradeValue()).ToList();
    }   
}
