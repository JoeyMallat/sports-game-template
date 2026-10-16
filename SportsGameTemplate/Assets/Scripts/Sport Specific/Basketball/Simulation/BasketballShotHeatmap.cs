using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Basketball Shot Heatmap", menuName = "Basketball/Shot Location Heatmap")]
public sealed class BasketballShotHeatmap : ScriptableObject
{
    [Serializable]
    public sealed class Layer
    {
        [SerializeField] string _position = "Point Guard";
        [SerializeField] BasketballOffensiveTactic _tactic;
        [SerializeField] List<float> _weights = new List<float>();

        public string Position => _position;
        public BasketballOffensiveTactic Tactic => _tactic;
        public IReadOnlyList<float> Weights => _weights;

        public Layer(string position, BasketballOffensiveTactic tactic, int cellCount)
        {
            _position = position;
            _tactic = tactic;
            _weights = Enumerable.Repeat(0.5f, cellCount).ToList();
        }

        internal void EnsureSize(int cellCount)
        {
            while (_weights.Count < cellCount)
                _weights.Add(0.5f);
            if (_weights.Count > cellCount)
                _weights.RemoveRange(cellCount, _weights.Count - cellCount);
        }

        internal float Get(int index) => index >= 0 && index < _weights.Count ? _weights[index] : 0.5f;
        internal void Set(int index, float value) => _weights[index] = Mathf.Clamp01(value);
    }

    public static readonly string[] BasketballPositions = { "Point Guard", "Shooting Guard", "Small Forward", "Power Forward", "Center" };

    [SerializeField][Range(8, 40)] int _columns = 25;
    [SerializeField][Range(8, 40)] int _rows = 24;
    [SerializeField] List<Layer> _layers = new List<Layer>();

    public int Columns => _columns;
    public int Rows => _rows;
    public IReadOnlyList<Layer> Layers => _layers.AsReadOnly();

    public Vector2 Sample(string position, BasketballOffensiveTactic tactic, BasketballShotType shotType, BasketballRandom random)
    {
        if (random == null)
            throw new ArgumentNullException(nameof(random));
        Layer layer = GetOrCreateLayer(position, tactic);
        List<int> compatibleCells = new List<int>();
        for (int index = 0; index < _columns * _rows; index++)
        {
            Vector2 center = CellCenter(index);
            if (IsCompatible(center, shotType))
                compatibleCells.Add(index);
        }
        if (compatibleCells.Count == 0)
            return Vector2.zero;

        int selected = random.Weighted(compatibleCells, index => 0.2f + layer.Get(index) * 1.8f);
        int column = selected % _columns;
        int row = selected / _columns;
        float cellWidth = 50f / _columns;
        float cellHeight = 47f / _rows;
        return new Vector2(
            -25f + column * cellWidth + random.Range(0.05f, 0.95f) * cellWidth,
            row * cellHeight + random.Range(0.05f, 0.95f) * cellHeight);
    }

    public Layer GetOrCreateLayer(string position, BasketballOffensiveTactic tactic)
    {
        string normalizedPosition = BasketballPositions.Contains(position) ? position : "Small Forward";
        Layer layer = _layers.FirstOrDefault(candidate => candidate.Position == normalizedPosition && candidate.Tactic == tactic);
        if (layer == null)
        {
            layer = new Layer(normalizedPosition, tactic, _columns * _rows);
            _layers.Add(layer);
        }
        layer.EnsureSize(_columns * _rows);
        return layer;
    }

    public float GetWeight(string position, BasketballOffensiveTactic tactic, int column, int row)
    {
        ValidateCell(column, row);
        return GetOrCreateLayer(position, tactic).Get(row * _columns + column);
    }

    public void Paint(string position, BasketballOffensiveTactic tactic, int column, int row, float value, float strength)
    {
        ValidateCell(column, row);
        Layer layer = GetOrCreateLayer(position, tactic);
        int index = row * _columns + column;
        layer.Set(index, Mathf.Lerp(layer.Get(index), Mathf.Clamp01(value), Mathf.Clamp01(strength)));
    }

    public void ResetLayer(string position, BasketballOffensiveTactic tactic, float value = 0.5f)
    {
        Layer layer = GetOrCreateLayer(position, tactic);
        for (int i = 0; i < _columns * _rows; i++)
            layer.Set(i, value);
    }

    private Vector2 CellCenter(int index)
    {
        int column = index % _columns;
        int row = index / _columns;
        return new Vector2(-25f + (column + 0.5f) * 50f / _columns, (row + 0.5f) * 47f / _rows);
    }

    private static bool IsCompatible(Vector2 point, BasketballShotType shotType)
    {
        float distance = point.magnitude;
        switch (shotType)
        {
            case BasketballShotType.Dunk:
                return distance <= 5.5f;
            case BasketballShotType.Layup:
                return distance <= 9f;
            case BasketballShotType.ThreePointer:
                return distance >= 22f;
            case BasketballShotType.TwoPointJumpShot:
                return distance > 7f && distance < 23.5f;
            default:
                return true;
        }
    }

    private void ValidateCell(int column, int row)
    {
        if (column < 0 || column >= _columns || row < 0 || row >= _rows)
            throw new ArgumentOutOfRangeException($"Cell ({column}, {row}) is outside {_columns}x{_rows} heatmap.");
    }

    private void OnValidate()
    {
        _columns = Mathf.Clamp(_columns, 8, 40);
        _rows = Mathf.Clamp(_rows, 8, 40);
        foreach (Layer layer in _layers)
            layer.EnsureSize(_columns * _rows);
    }
}
