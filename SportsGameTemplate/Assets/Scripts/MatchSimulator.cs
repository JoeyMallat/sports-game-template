using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface MatchSimulator
{
    public void SimulateMatch(Match match, int teamToForceWin);
}
