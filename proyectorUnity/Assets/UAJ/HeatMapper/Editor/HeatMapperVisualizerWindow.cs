using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class HeatMapperVisualizerWindow : EditorWindow
{
    private Vector2 scroll;

    private HeatMapperTracker _tracker;

    private List<string> _sessions = new List<string>();

    private string _basePath;

    // Estado visual por INSTANCIA
    private Dictionary<string, bool> _mapVisibility = new Dictionary<string, bool>();
    private Dictionary<string, float> _mapOpacity = new Dictionary<string, float>();
    private Dictionary<string, Color> _mapColor = new Dictionary<string, Color>();

    // tracking de múltiples instancias del mismo heatmap
    private Dictionary<string, HeatMap> _activeHeatmaps = new Dictionary<string, HeatMap>();

    [MenuItem("Tools/HeatMapper/Visualizer", false, 1)]
    public static void OpenWindow()
    {
        HeatMapperVisualizerWindow window = GetWindow<HeatMapperVisualizerWindow>();
        window.titleContent = new GUIContent("HeatMapper Visualizer");
    }

    private void OnEnable()
    {
        RefreshTracker();
        RefreshSessions();
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void OnGUI()
    {
        RefreshTracker();

        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("HEATMAP VISUALIZER", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (_tracker != null)
        {
            DrawTrackerBlock(_tracker);
            EditorGUILayout.Space();
        }
        else
        {
            EditorGUILayout.HelpBox("No tracker in scene.", MessageType.Info);
            EditorGUILayout.Space();
        }

        DrawSessionsBlock();

        EditorGUILayout.EndScrollView();
    }

    private void DrawSessionsBlock()
    {
        EditorGUILayout.LabelField("Sessions", EditorStyles.boldLabel);

        if (GUILayout.Button("Refresh Sessions"))
        {
            RefreshSessions();
        }

        if (_sessions.Count == 0)
        {
            EditorGUILayout.HelpBox("No saved sessions.", MessageType.Info);
            return;
        }

        foreach (string session in _sessions)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField(session, EditorStyles.boldLabel);

            string sessionPath = Path.Combine(_basePath, session);

            if (Directory.Exists(sessionPath))
            {
                string[] files = Directory.GetFiles(sessionPath, "*.json");

                foreach (string file in files)
                {
                    string mapName = Path.GetFileNameWithoutExtension(file);

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(mapName);

                    if (GUILayout.Button("Add Heatmap", GUILayout.Width(120)))
                    {
                        AddHeatmapFromFile(file);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
        }
    }

    private void AddHeatmapFromFile(string file)
    {
        if (_tracker == null || _tracker.heatMapVisualizer == null)
            return;

        HeatMap heatmap = HeatMapSerializer.LoadFromFile(file);
        if (heatmap == null)
            return;

        string mapName = Path.GetFileNameWithoutExtension(file);

        var configs = _tracker.heatMapConfigs;
        if (configs == null)
            return;

        foreach (MapConfig config in configs)
        {
            if (config == null)
                continue;

            if (config.mapName != mapName)
                continue;

            string instanceKey =
                _tracker.name + "_" +
                config.mapName + "_" +
                System.Guid.NewGuid().ToString();

            _activeHeatmaps[instanceKey] = heatmap;

            _tracker.heatMapVisualizer.createTileMap(config);
            _tracker.heatMapVisualizer.updateTileMap(heatmap, config);

            break;
        }
    }

    private void DrawTrackerBlock(HeatMapperTracker tracker)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField(tracker.name, EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (tracker.heatMapConfigs == null)
        {
            EditorGUILayout.LabelField("No heatmap configs");
            EditorGUILayout.EndVertical();
            return;
        }

        foreach (MapConfig config in tracker.heatMapConfigs)
        {
            if (config == null)
                continue;

            DrawHeatmapBlock(tracker, config);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawHeatmapBlock(HeatMapperTracker tracker, MapConfig config)
    {
        string key = tracker.name + "_" + config.mapName;

        if (!_mapVisibility.ContainsKey(key))
            _mapVisibility[key] = config.visible;

        if (!_mapOpacity.ContainsKey(key))
            _mapOpacity[key] = 1f;

        if (!_mapColor.ContainsKey(key))
            _mapColor[key] = config.color;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();

        _mapVisibility[key] = EditorGUILayout.Toggle(_mapVisibility[key], GUILayout.Width(20));
        EditorGUILayout.LabelField(config.mapName, EditorStyles.boldLabel);

        EditorGUILayout.EndHorizontal();

        _mapColor[key] = EditorGUILayout.ColorField("Color", _mapColor[key]);
        _mapOpacity[key] = EditorGUILayout.Slider("Opacity", _mapOpacity[key], 0f, 1f);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Summary", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            BuildHeatmapSummary(tracker, config),
            MessageType.None
        );

        EditorGUILayout.EndVertical();

        ApplyVisualSettings(tracker, config, key);
    }

    private void ApplyVisualSettings(HeatMapperTracker tracker, MapConfig config, string key)
    {
        config.visible = _mapVisibility[key];

        Color c = _mapColor[key];
        c.a = _mapOpacity[key];
        config.color = c;

        if (tracker.heatMapVisualizer == null)
            return;

        var heatmaps = tracker.GetHeatMaps();
        if (heatmaps == null || !heatmaps.ContainsKey(config.mapName))
            return;

        HeatMap heatmap = heatmaps[config.mapName];

        tracker.heatMapVisualizer.updateTileMap(heatmap, config);
    }

    private string BuildHeatmapSummary(HeatMapperTracker tracker, MapConfig config)
    {
        if (tracker == null) return "No tracker";

        var heatmaps = tracker.GetHeatMaps();
        if (heatmaps == null || !heatmaps.ContainsKey(config.mapName))
            return "No live data";

        HeatMap map = heatmaps[config.mapName];

        int total = 0;
        int max = 0;
        int occupied = 0;

        for (int x = 0; x < map.GetWidth(); x++)
        {
            for (int y = 0; y < map.GetHeight(); y++)
            {
                int v = map.GetHeatMapValue(x, y);

                total += v;

                if (v > 0) occupied++;
                if (v > max) max = v;
            }
        }

        return
            $"Type: {config.eventType}\n" +
            $"Visible: {config.visible}\n" +
            $"Occupied cells: {occupied}\n" +
            $"Max value: {max}\n" +
            $"Total value: {total}";
    }

    private void RefreshSessions()
    {
        _sessions.Clear();

        _basePath = Path.Combine(Application.dataPath, "heatMaps");

        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
            return;
        }

        foreach (string dir in Directory.GetDirectories(_basePath))
        {
            _sessions.Add(Path.GetFileName(dir));
        }
    }

    private void RefreshTracker()
    {
#if UNITY_2023_1_OR_NEWER
        _tracker = Object.FindFirstObjectByType<HeatMapperTracker>();
#else
        _tracker = Object.FindObjectOfType<HeatMapperTracker>();
#endif
    }
}