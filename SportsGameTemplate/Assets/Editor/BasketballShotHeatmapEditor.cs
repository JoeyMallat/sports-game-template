using System;
using UnityEditor;
using UnityEngine;

public sealed class BasketballShotHeatmapPainter : EditorWindow
{
    private BasketballShotHeatmap _heatmap;
    private int _positionIndex;
    private BasketballOffensiveTactic _tactic;
    private float _brushValue = 1f;
    private float _brushStrength = 0.35f;
    private int _brushRadius = 1;

    [MenuItem("Tools/Basketball/Shot Heatmap Painter")]
    public static void Open()
    {
        GetWindow<BasketballShotHeatmapPainter>("Shot Heatmap");
    }

    public static void Open(BasketballShotHeatmap heatmap)
    {
        BasketballShotHeatmapPainter window = GetWindow<BasketballShotHeatmapPainter>("Shot Heatmap");
        window._heatmap = heatmap;
        window.Repaint();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Basketball Shot Location Heatmap", EditorStyles.boldLabel);
        _heatmap = (BasketballShotHeatmap)EditorGUILayout.ObjectField("Heatmap Asset", _heatmap, typeof(BasketballShotHeatmap), false);
        if (_heatmap == null)
        {
            EditorGUILayout.HelpBox("Assign or create a Basketball Shot Heatmap asset to paint position and tactic preferences.", MessageType.Info);
            if (GUILayout.Button("Create Heatmap Asset"))
                CreateAsset();
            return;
        }

        _positionIndex = EditorGUILayout.Popup("Player Position", _positionIndex, BasketballShotHeatmap.BasketballPositions);
        _tactic = (BasketballOffensiveTactic)EditorGUILayout.EnumPopup("Offensive Tactic", _tactic);
        _brushValue = EditorGUILayout.Slider("Paint Preference", _brushValue, 0f, 1f);
        _brushStrength = EditorGUILayout.Slider("Brush Strength", _brushStrength, 0.05f, 1f);
        _brushRadius = EditorGUILayout.IntSlider("Brush Radius", _brushRadius, 0, 5);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset Neutral"))
            ResetLayer(0.5f);
        if (GUILayout.Button("Clear"))
            ResetLayer(0f);
        if (GUILayout.Button("Fill Preferred"))
            ResetLayer(1f);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("Drag on the half court to paint. Bright red areas are preferred; dark areas remain possible because runtime sampling always includes a baseline weight.", MessageType.None);
        float width = Mathf.Min(position.width - 24f, 650f);
        Rect court = GUILayoutUtility.GetRect(width, width * 47f / 50f, GUILayout.ExpandWidth(false));
        court.x = (position.width - court.width) * 0.5f;
        DrawCourt(court);
        HandlePainting(court);
    }

    private void DrawCourt(Rect court)
    {
        EditorGUI.DrawRect(court, new Color(0.72f, 0.49f, 0.25f));
        float cellWidth = court.width / _heatmap.Columns;
        float cellHeight = court.height / _heatmap.Rows;
        string playerPosition = BasketballShotHeatmap.BasketballPositions[_positionIndex];
        for (int row = 0; row < _heatmap.Rows; row++)
        {
            for (int column = 0; column < _heatmap.Columns; column++)
            {
                float weight = _heatmap.GetWeight(playerPosition, _tactic, column, row);
                Rect cell = new Rect(court.x + column * cellWidth, court.yMax - (row + 1) * cellHeight, cellWidth, cellHeight);
                EditorGUI.DrawRect(cell, new Color(weight, 0.08f, 1f - weight, 0.48f));
            }
        }

        Handles.BeginGUI();
        Handles.color = Color.white;
        Handles.DrawAAPolyLine(2f,
            new Vector3(court.xMin, court.yMin), new Vector3(court.xMax, court.yMin),
            new Vector3(court.xMax, court.yMax), new Vector3(court.xMin, court.yMax),
            new Vector3(court.xMin, court.yMin));
        Vector2 hoop = CourtToGui(court, new Vector2(0f, 5.25f));
        float scale = court.width / 50f;
        Handles.DrawWireDisc(hoop, Vector3.forward, 0.75f * scale);
        Handles.DrawLine(new Vector2(court.center.x - 3f * scale, court.yMax - 4f * scale), new Vector2(court.center.x + 3f * scale, court.yMax - 4f * scale));
        Rect key = new Rect(court.center.x - 8f * scale, court.yMax - 19f * scale, 16f * scale, 19f * scale);
        Handles.DrawAAPolyLine(2f,
            new Vector3(key.xMin, key.yMin), new Vector3(key.xMax, key.yMin),
            new Vector3(key.xMax, key.yMax), new Vector3(key.xMin, key.yMax));
        Handles.DrawWireArc(hoop, Vector3.forward, Vector3.left, 180f, 23.75f * scale);
        Handles.EndGUI();
    }

    private void HandlePainting(Rect court)
    {
        Event current = Event.current;
        if ((current.type != EventType.MouseDown && current.type != EventType.MouseDrag) || current.button != 0 || !court.Contains(current.mousePosition))
            return;

        float normalizedX = Mathf.InverseLerp(court.xMin, court.xMax, current.mousePosition.x);
        float normalizedY = 1f - Mathf.InverseLerp(court.yMin, court.yMax, current.mousePosition.y);
        int centerColumn = Mathf.Clamp(Mathf.FloorToInt(normalizedX * _heatmap.Columns), 0, _heatmap.Columns - 1);
        int centerRow = Mathf.Clamp(Mathf.FloorToInt(normalizedY * _heatmap.Rows), 0, _heatmap.Rows - 1);
        Undo.RecordObject(_heatmap, "Paint Basketball Shot Heatmap");
        string playerPosition = BasketballShotHeatmap.BasketballPositions[_positionIndex];
        for (int row = centerRow - _brushRadius; row <= centerRow + _brushRadius; row++)
        {
            for (int column = centerColumn - _brushRadius; column <= centerColumn + _brushRadius; column++)
            {
                if (column < 0 || column >= _heatmap.Columns || row < 0 || row >= _heatmap.Rows)
                    continue;
                float distance = Vector2.Distance(new Vector2(column, row), new Vector2(centerColumn, centerRow));
                if (distance > _brushRadius + 0.25f)
                    continue;
                float falloff = _brushRadius == 0 ? 1f : 1f - distance / (_brushRadius + 1f);
                _heatmap.Paint(playerPosition, _tactic, column, row, _brushValue, _brushStrength * falloff);
            }
        }
        EditorUtility.SetDirty(_heatmap);
        current.Use();
        Repaint();
    }

    private void ResetLayer(float value)
    {
        Undo.RecordObject(_heatmap, "Reset Basketball Shot Heatmap");
        _heatmap.ResetLayer(BasketballShotHeatmap.BasketballPositions[_positionIndex], _tactic, value);
        EditorUtility.SetDirty(_heatmap);
    }

    private void CreateAsset()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create Basketball Shot Heatmap", "BasketballShotHeatmap", "asset", "Choose a location for the heatmap asset.");
        if (string.IsNullOrEmpty(path))
            return;
        _heatmap = CreateInstance<BasketballShotHeatmap>();
        AssetDatabase.CreateAsset(_heatmap, path);
        AssetDatabase.SaveAssets();
        Selection.activeObject = _heatmap;
    }

    private static Vector2 CourtToGui(Rect court, Vector2 courtPosition)
    {
        return new Vector2(
            Mathf.Lerp(court.xMin, court.xMax, (courtPosition.x + 25f) / 50f),
            Mathf.Lerp(court.yMax, court.yMin, courtPosition.y / 47f));
    }
}

[CustomEditor(typeof(BasketballShotHeatmap))]
public sealed class BasketballShotHeatmapInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Open Shot Heatmap Painter"))
            BasketballShotHeatmapPainter.Open((BasketballShotHeatmap)target);
    }
}
