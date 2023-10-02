using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MM_LeagueView : MonoBehaviour, ISettable
{


    public void SetDetails<T>(T item) where T : class
    {
        List<Team> league = item as List<Team>;

        
    }
}
