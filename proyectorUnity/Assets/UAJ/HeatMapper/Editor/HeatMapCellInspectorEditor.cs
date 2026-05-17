using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Herramienta de editor para inspeccionar celdas del heatmap en tiempo real
// Permite hacer click en una celda en la Scene View y mostrar un popup
// con la informacion de la celda seleccionada
[InitializeOnLoad]
public static class HeatMapCellInspectorEditor
{
    // Tracker actualmente seleccionado o encontrado en la escena
    private static HeatMapperTracker _selectedTracker;

    // Datos de la celda seleccionada
    private static bool _hasSelectedCell;
    private static Vector2Int _selectedCell;
    private static Vector3 _selectedCellWorldCenter;
    private static List<string> _selectedCellInfo = new List<string>();

    // Se ejecuta al cargar Unity y registrar el metodo OnSceneGUI
    // para poder dibujar y detectar clicks en la Scene View
    static HeatMapCellInspectorEditor()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        // Busca el tracker seleccionado o, si no hay ninguno, uno existente en la escena
        FindSelectedTracker();

        if(_selectedTracker == null)
        {
            return;
        }

        if(!_selectedTracker.enableCellInspector)
        {
            return;
        }

        // Detecta clicks del raton sobre la Scene View
        HandleMouseClick();

        // Si hay una celda seleccionada, dibuja su info
        DrawSelectedCellInfo();
    }

    // Busca el tracker seleccionado o, si no hay ninguno, uno existente en la escena
    private static void FindSelectedTracker()
    {
        // Intenta usar el objeto seleccionado en la jerarquia
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

        // Si ya habia un tracker guardado, se mantiene
        if(_selectedTracker != null)
        {
            return;
        }

        // Si no lo hay, busca uno en la escena
    #if UNITY_2023_1_OR_NEWER
        _selectedTracker = Object.FindFirstObjectByType<HeatMapperTracker>();
    #else
        _selectedTracker = Object.FindObjectOfType<HeatMapperTracker>();
    #endif
    }

    // Detecta clicks del raton sobre la Scene View
    private static void HandleMouseClick()
    {
        Event currentEvent = Event.current;

        if (currentEvent == null)
        {
            return;
        }

        // Solo se procesa click izquierdo
        if (currentEvent.type != EventType.MouseDown || currentEvent.button != 0)
        {
            return;
        }

        Vector3 worldPosition;

        // Convierte la posicion del raton en pantalla a una posicion del mundo
        if (!GetMouseWorldPosition(currentEvent.mousePosition, out worldPosition))
        {
            return;
        }

        Vector2Int cell;

        // Convierte la posicion de mundo a celda del heatmap
        if (!_selectedTracker.GetCellAtWorldPosition(worldPosition, out cell))
        {
            _hasSelectedCell = false;
            return;
        }

        Vector3 cellCenter;

        // Obtiene el centro de la celda para dibujar el popup
        if (!_selectedTracker.GetCellWorldCenter(cell, out cellCenter))
        {
            _hasSelectedCell = false;
            return;
        }

        // Obtiene la informacion de los heatmpas con valor en esa celda
        _selectedCellInfo = _selectedTracker.GetCellInfo(cell);

        // Si la celda no tiene valores en ningun heatmap, no se muestra el popup
        if (_selectedCellInfo == null || _selectedCellInfo.Count == 0)
        {
            _hasSelectedCell = false;
            currentEvent.Use();
            SceneView.RepaintAll();
            return;
        }

        // Guarda la celda seleccionada
        _selectedCell = cell;
        _selectedCellWorldCenter = cellCenter;
        _hasSelectedCell = true;

        currentEvent.Use();
        SceneView.RepaintAll();
    }

    // Convierte la posicion del raton en pantalla a una posicion del mundo
    private static bool GetMouseWorldPosition(Vector2 mousePosition, out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;

        // Crea un rayo desde la posicion del raton en la Scene View
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

        // Plano 2D sobre el que se proyecta el click
        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, _selectedTracker.transform.position.z));

        if(!plane.Raycast(ray, out float distance))
        {
            return false;
        }

        // Punto del mundo donde el rayo corta el plano
        worldPosition = ray.GetPoint(distance);
        worldPosition.z = _selectedTracker.transform.position.z;

        return true;
    }

    // Dibuja la info de la celda seleccionada
    private static void DrawSelectedCellInfo()
    {
        if(!_hasSelectedCell)
        {
            return;
        }

        // Recalcula la informacion cada vez que se repinta la Scene View
        _selectedCellInfo = _selectedTracker.GetCellInfo(_selectedCell);

        if (_selectedCellInfo == null || _selectedCellInfo.Count == 0) {  return; }
        string text = BuildCellInfoText();

        if(string.IsNullOrEmpty(text)) {  return; }

        DrawSelectedCellHightlight();
        DrawInfoPopup(text);
    }

    // Construye el texto de info general de la celda seleccionada
    private static string BuildCellInfoText()
    {
        if(_selectedCellInfo == null || _selectedCellInfo.Count == 0)
        {
            return "";
        }

        // Celda y las posiciones X e Y del mundo
        string text =
        $"Cell: ({_selectedCell.x}, {_selectedCell.y})\n" +
        $"World pos: ({_selectedCellWorldCenter.x:F2}, {_selectedCellWorldCenter.y:F2})\n\n";

        // Aniade la informacion de cada heatmap con valor en esta celda
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

    // Resalta la celda seleccionada
    private static void DrawSelectedCellHightlight()
    {
        // Usar el cellSize del tracker para dibujar el borde de la celda seleccionada
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

    // Dibuja el popup
    private static void DrawInfoPopup(string text)
    {
        // Convierte la posicion del mundo a posicion de pantalla en la Scene View
        Vector2 guiPoint = HandleUtility.WorldToGUIPoint(_selectedCellWorldCenter);

        // Desplaza el popup un poco a la derecha y ligeramente hacia arriba
        float offsetX = 20f;
        float offsetY = -30f;
        float width = 130f;

        // Estilo del texto
        GUIStyle textStyle = new GUIStyle(GUI.skin.label);
        textStyle.normal.textColor = Color.black;
        textStyle.fontSize = 9;
        textStyle.wordWrap = true;

        float height = textStyle.CalcHeight(new GUIContent(text), width);

        Rect rect = new Rect(guiPoint.x + offsetX, guiPoint.y + offsetY, width, height);

        Handles.BeginGUI();

        // Fondo blanco semitransparente
        Color oldColor = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.85f);
        GUI.Box(rect, GUIContent.none);
        GUI.color = oldColor;

        // Boton X para cerrar popup
        Rect closeRect = new Rect(rect.xMax - 12f, rect.y, 12f, 12f);

        GUIStyle closeStyle = new GUIStyle(GUI.skin.button);
        closeStyle.fontSize = 8;
        closeStyle.padding = new RectOffset(0,0,0,0);

        if(GUI.Button(closeRect, "X", closeStyle))
        {
            ClearSelection();
            Handles.EndGUI();
            return;
        }

        // Texto del popup
        Rect textRect = new Rect(rect.x, rect.y, rect.width, rect.height);
        GUI.Label(textRect, text, textStyle);

        Handles.EndGUI();
    }

    // Limpia la celda seleccionada y ocultar el popup de info
    public static void ClearSelection()
    {
        _hasSelectedCell = false;
        _selectedCellInfo.Clear();
        SceneView.RepaintAll();
    }
}
