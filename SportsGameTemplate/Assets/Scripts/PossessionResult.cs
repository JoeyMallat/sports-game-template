using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossessionResult
{
    [SerializeField] Player _player;
    [SerializeField] ResultAction _resultAction;

    public PossessionResult(Player player, ResultAction resultAction)
    {
        _player = player;
        _resultAction = resultAction;
    }

    public Player GetPlayer()
    {
        return _player;
    }

    public ResultAction GetPossessionResult()
    {
        return _resultAction;
    }
}
