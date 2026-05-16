using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Clase auxiliar de editor para dibujar la configuracion de heatmaps
// Se usa tanto en la ventana HeatMapper como en el inspector del HeatMapperTracker
// Interfaz comun editable del tracker
public static class HeatMapConfigDrawer
{
    // Dibuja los campos generales del area de tracking que el usuario puede modificar
    public static void DrawTrackerSettings(HeatMapperTracker tracker)
    {
        if (tracker == null) return;

        Undo.RecordObject(tracker, "Modify HeatMapperTracker");

        EditorGUILayout.LabelField("Tracking Area", EditorStyles.boldLabel);

        // Tamanio total del area que cubre el atracker
        tracker.areaSize = EditorGUILayout.Vector2Field("Area Size", tracker.areaSize);
        // Tamanio de celda
        tracker.cellSize = EditorGUILayout.FloatField("Cell Size", tracker.cellSize);
        // Evitar que el tamanio de casilla sea 0 o negativo
        if (tracker.cellSize <= 0f)
        {
            tracker.cellSize = 0.1f;
        }
        // Ajsute del area a la cuadricula
        if (GUILayout.Button("Snap Area To Grid"))
        {
            tracker.SnapAreaToGrid();
            EditorUtility.SetDirty(tracker);
            SceneView.RepaintAll();
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
        // Visibilidad del area de debug
        tracker.showTrackingArea = EditorGUILayout.Toggle("Show Tracking Area", tracker.showTrackingArea);
        // Visibilidad de la grid de debug
        tracker.showGrid = EditorGUILayout.Toggle("Show Cell Grid", tracker.showGrid);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Editor Debug", EditorStyles.boldLabel);
        // Activacion de los handles de edicion del area
        tracker.showAreaEditor = EditorGUILayout.Toggle("Show Area Editor", tracker.showAreaEditor);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Cell Inspector", EditorStyles.boldLabel);
        // Permitir la snspeccion de datos de una celda
        tracker.enableCellInspector = EditorGUILayout.Toggle("Enable Cell Inspector", tracker.enableCellInspector);
        // Activar informar solo los heatmaps visibles
        tracker.showOnlyVisibleHeatMaps = EditorGUILayout.Toggle("Show Info Only Visible HeatMaps", tracker.showOnlyVisibleHeatMaps);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Visualization", EditorStyles.boldLabel);
        tracker.heatMapVisualizer = EditorGUILayout.ObjectField(
            "Heat Map Visualizer", tracker.heatMapVisualizer, typeof(HeatmapVisualizer), true) as HeatmapVisualizer;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(tracker);
            SceneView.RepaintAll();
        }
    }
    // Dibuja la lista de heatmaps configurados en el tracker.
    // Se pueden aniadir, eliminar y modificar mapas
    public static void DrawHeatMapConfigs(HeatMapperTracker tracker)
    {
        EditorGUILayout.LabelField("HeatMaps", EditorStyles.boldLabel);

        if (tracker.heatMapConfigs == null)
        {
            tracker.heatMapConfigs = new List<MapConfig>();
        }

        for (int i = 0; i < tracker.heatMapConfigs.Count; i++)
        {
            MapConfig config = tracker.heatMapConfigs[i];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(config.mapName, EditorStyles.boldLabel);

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                Undo.RecordObject(tracker, "Remove HeatMap");
                tracker.heatMapConfigs.RemoveAt(i);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();

            // Campos comunes a todos los tipos de heatmap
            DrawCommonFields(config);
            // Campos especificos segun el tipo de evento seleccionado
            DrawEventFields(config);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Add HeatMap"))
        {
            MapConfig newConfig = new MapConfig()
            {
                mapName = "HeatMap_" + tracker.heatMapConfigs.Count
            };

            tracker.heatMapConfigs.Add(newConfig);
        }
    }

    private static void DrawCommonFields(MapConfig config)
    {
        config.visible = EditorGUILayout.Toggle("Visible", config.visible);
        config.mapName = EditorGUILayout.TextField("Map Name", config.mapName);
        config.color = EditorGUILayout.ColorField("Color", config.color);
        config.eventType = (TrackEventType)EditorGUILayout.EnumPopup("Event Type", config.eventType);
    }

    // Dibuja los campos concretos segun el tipo de evento elegido
    private static void DrawEventFields(MapConfig config)
    {
        switch (config.eventType)
        {
            case TrackEventType.Transform:
                DrawTransformFields(config);
                break;

            case TrackEventType.InputKey:
                DrawInputKeyFields(config);
                break;

            case TrackEventType.InputMouse:
                DrawInputMouseFields(config);
                break;

            case TrackEventType.ListTransform:
                DrawListTransformFields(config);
                break;

            case TrackEventType.Custom:
                DrawCustomFields(config);
                break;
        }
    }

    // Campos para registrar periodicamente la posicion de un unico Transform
    private static void DrawTransformFields(MapConfig config)
    {
        config.sampleInterval = EditorGUILayout.FloatField("Sample Interval", config.sampleInterval);

        if (config.sampleInterval <= 0f)
        {
            config.sampleInterval = 0.01f;
        }

        config.tr = EditorGUILayout.ObjectField("Transform to Register", config.tr, typeof(Transform), true) as Transform;
    }

    // Campos para registrar la posicion de una entidad al pulsar una tecla
    private static void DrawInputKeyFields(MapConfig config)
    {
        config.inputKey = (KeyCode)EditorGUILayout.EnumPopup("Input Key", config.inputKey);

        config.tr = EditorGUILayout.ObjectField("Entity Transform", config.tr, typeof(Transform), true) as Transform;
    }

    // Campos para registrar la posicion del raton al pulsar un boton
    private static void DrawInputMouseFields(MapConfig config)
    {
        config.mouseButton = (MouseButton)EditorGUILayout.EnumPopup("Mouse Button", config.mouseButton);
    }

    // Campos para registrar periodicamente la posicion de varios Transforms
    private static void DrawListTransformFields(MapConfig config)
    {
        config.sampleInterval = EditorGUILayout.FloatField("Sample Interval", config.sampleInterval);

        if (config.sampleInterval <= 0f)
        {
            config.sampleInterval = 0.01f;
        }

        DrawTransformList(config);
    }

    // Dibuja la lista de transforms asociados a un heatmap de tipo ListTransform
    private static void DrawTransformList(MapConfig config)
    {
        EditorGUILayout.LabelField("Transforms", EditorStyles.miniBoldLabel);

        if (config.transformList == null)
        {
            config.transformList = new List<Transform>();
        }

        for (int t = 0; t < config.transformList.Count; t++)
        {
            EditorGUILayout.BeginHorizontal();

            config.transformList[t] = EditorGUILayout.ObjectField("Element " + t, config.transformList[t], typeof(Transform), true) as Transform;

            if (GUILayout.Button("-", GUILayout.Width(22)))
            {
                config.transformList.RemoveAt(t);
                EditorGUILayout.EndHorizontal();
                break;
            }

            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("+ Add Transform"))
        {
            config.transformList.Add(null);
        }
    }
    // Informacion para eventos customs
    private static void DrawCustomFields(MapConfig config)
    {
        EditorGUILayout.HelpBox("Este heatmap se actualiza desde codigo usando RegisterCustomEvent(mapName, position).", MessageType.Info);
    }
}
