using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatObjectWrapper
{
    [SerializeField] string _title;
    [SerializeField] List<float> _stats;
    [SerializeField] Player _playerLink;

    public StatObjectWrapper(string title, List<float> stats, Player player = null)
    {
        _title = title;
        _stats = stats;
        _playerLink = player;
    }

    public (string, List<float>) ReadStatObject()
    {
        return (_title, _stats);
    }

    public Player GetPlayerLink()
    {
        return _playerLink;
    }
}
