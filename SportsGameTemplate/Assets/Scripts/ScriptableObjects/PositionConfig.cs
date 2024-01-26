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

    public Position GetPosition(string position)
    {
        foreach (Position pos in _positions)
        {
            if (pos.GetPositionName() == position)
                return pos;
        }
        return null;
    }
}
