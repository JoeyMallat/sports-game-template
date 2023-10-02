using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public interface ITradeable
{
    public int CalculateTradeValue();
    public string GetTradeableID();

    public void AddToTrade(int teamID = 0);
}
