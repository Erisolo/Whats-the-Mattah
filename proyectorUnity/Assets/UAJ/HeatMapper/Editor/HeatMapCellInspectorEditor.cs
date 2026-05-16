using Codice.Client.BaseCommands.Merge;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

// Herramienta de editor para inspeccionar celdas del heatmap en tiempo real
// Permite hacer click en la Scene View y mostrar informacion de la celda seleccionada
[InitializeOnLoad]
public static class HeatMapCellInspectorEditor
{
    private static HeatMapperTracker _selectedTracker;

    private static bool _hasSelectedCell;
    private static Vector2Int _selectedCell;
    private static Vector3 _selectedCellWorldCenter;
    private static List<string> _selectedCellInfo = new List<string>();

    static HeatMapCellInspectorEditor()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        FindSelectedTracker();

        if(_selectedTracker == null)
        {
            return;
        }

        if(!_selectedTracker.enableCellInspector)
        {
            return;
        }

        HandleMouseClick();

        DrawSelectedCellInfo();
    }

    private static void FindSelectedTracker()
    {
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject != null)
        {
            HeatMapperTracker tracker = selectedObject.GetComponent<HeatMapperTracker>();
            if (tracker != null)
            {
                _selectedTracker = tracker;
                return;
            }
        }

        if(_selectedTracker != null)
        {
            return;
        }

    #if UNITY_2023_1_OR_NEWER
        _selectedTracker = Object.FindFirstObjectByType<HeatMapperTracker>();
    #else
        _selectedTracker = Object.FindObjectOfType<HeatMapperTracker>();
    #endif
    }

    private static void HandleMouseClick()
    {
        Event currenEvent = Event.current;

        if (currenEvent == null)
        {
            return;
        }

        if (currenEvent.type != EventType.MouseDown || currenEvent.button != 0)
        {
            return;
        }

        Vector3 worldPosition;

        if (!GetMouseWorldPosition(currenEvent.mousePosition, out worldPosition))
        {
            return;
        }

        Vector2Int cell;

        if (!_selectedTracker.GetCellAtWorldPosition(worldPosition, out cell))
        {
            _hasSelectedCell = false;
            return;
        }

        Vector3 cellCenter;

        if (!_selectedTracker.GetCellWorldCenter(cell, out cellCenter))
        {
            _hasSelectedCell = false;
            return;
        }

        _selectedCell = cell;
        _selectedCellWorldCenter = cellCenter;
        _selectedCellInfo = _selectedTracker.GetCellInfo(cell);
        _hasSelectedCell = true;

        currenEvent.Use();
        SceneView.RepaintAll();
    }

    private static bool GetMouseWorldPosition(Vector2 mousePosition, out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;

        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, _selectedTracker.transform.position.z));

        if(!plane.Raycast(ray, out float distance))
        {
            return false;
        }

        worldPosition = ray.GetPoint(distance);
        worldPosition.z = _selectedTracker.transform.position.z;

        return true;
    }

    private static void DrawSelectedCellInfo()
    {
        if(!_hasSelectedCell)
        {
            return;
        }

        DrawSelectedCellHightlight();

        string text = BuildCellInfoText();

        Handles.Label(_selectedCellWorldCenter + Vector3.up * 0.5f, text);
    }

    private static string BuildCellInfoText()
    {
        string text =
            $"Cell: ({_selectedCell.x}, {_selectedCell.y})\n" +
            $"World position: {_selectedCellWorldCenter}\n\n";

        if(_selectedCellInfo == null || _selectedCellInfo.Count == 0)
        {
            text += "No hay datos en esta celda.";
            return text;
        }

        for (int i = 0; i < _selectedCellInfo.Count; i++)
        {
            text += _selectedCellInfo[i];

            if(i < _selectedCellInfo.Count - 1)
            {
                text += "\n\n";
            }
        }

        return text;
    }

    private static void DrawSelectedCellHightlight()
    {
        float size = _selectedTracker.cellSize;

        Handles.color = Color.white;

        Vector3 topLeft = _selectedCellWorldCenter + new Vector3(-size / 2f, size / 2f, 0f);
        Vector3 topRight = _selectedCellWorldCenter + new Vector3(size / 2f, size / 2f, 0f);
        Vector3 bottomRight = _selectedCellWorldCenter + new Vector3(size / 2f, -size / 2f, 0f);
        Vector3 bottomLeft= _selectedCellWorldCenter + new Vector3(-size / 2f, -size / 2f, 0f);

        Handles.DrawLine(topLeft, topRight);
        Handles.DrawLine(topRight, bottomRight);
        Handles.DrawLine(bottomRight, bottomLeft);
        Handles.DrawLine(bottomLeft, topLeft);
    }
}
