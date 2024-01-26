using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AutoFillAgeRange : MonoBehaviour
{
    public int MinNumber;
    public int MaxNumber;

    private void Awake()
    {
        TMP_Dropdown[] dropdowns = GetComponentsInChildren<TMP_Dropdown>();

        foreach (var dropdown in dropdowns)
        {
            dropdown.AddOptions(GetOptions());
        }
    }

    private List<string> GetOptions()
    {
        List<string> options = new List<string>();

        for (int i = MinNumber; i < MaxNumber; i++)
        {
            options.Add(i.ToString());
        }

        return options;
    }
}
