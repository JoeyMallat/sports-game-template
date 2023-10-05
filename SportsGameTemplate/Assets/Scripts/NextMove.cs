using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NextMove
{
    [SerializeField] Move _move;
    [SerializeField] AnimationCurve _occurencePerSecond;

    public float GetOddsOfMove(int secondsPlayedInPossesion)
    {
        return _occurencePerSecond.Evaluate(secondsPlayedInPossesion);
    }

    public Move GetMove()
    {
        return _move;
    }
}
