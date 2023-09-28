using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatObjectWrapper
{
    [SerializeField] string _title;
    [SerializeField] List<float> _stats;

    public StatObjectWrapper(string title, List<float> stats)
    {
        _title = title;
        _stats = stats;
    }

    public (string, List<float>) ReadStatObject()
    {
        return (_title, _stats);
    }
}
