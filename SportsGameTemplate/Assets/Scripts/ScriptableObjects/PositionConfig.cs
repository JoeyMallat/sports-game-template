using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Position Config", menuName = "Config/Position Config")]
public class PositionConfig : ScriptableObject
{
    [SerializeField] SportsType _sportsType;
    [SerializeField] List<Position> _positions;

    public List<Position> GetPositions()
    {
        return _positions;
    }
}
