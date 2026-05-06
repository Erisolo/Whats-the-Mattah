using System.Linq;
using UnityEditor;
using UnityEngine;

public class HeatMapper : EditorWindow {

    [Header("VISUALIZAR")]
    bool ver;

    [Header("CREAR")]
    string mapBaseName = "";

    MapConfig[] _heatMapConfigs;


    [MenuItem("Tools/Basic Object Spawner")]
    public static void ShowWindow() {
        GetWindow(typeof(HeatMapper));
    }

    private void OnGUI() {
        _heatMapConfigs = new MapConfig[0];

        var style = new GUIStyle(EditorStyles.boldLabel)
        {
            normal = { textColor = new Color(0.95f, 0.7f, 0.84f) }
        };
        EditorGUILayout.LabelField("HEATMAPPER", style);

        if (GUILayout.Button("Add Tracker To Scene"))
        {
            AddTracker();
        }

        mapBaseName = EditorGUILayout.TextField("Base Name", mapBaseName);

        foreach (MapConfig m in _heatMapConfigs)
        {
            m.visible = EditorGUILayout.Toggle(m.visible);
            m.mapName = EditorGUILayout.TextField("Map Name", m.mapName);
            m.tr = EditorGUILayout.ObjectField("Transform to Register", m.tr, typeof(Transform), false) as Transform;
        }

        if (GUILayout.Button("AddMap")) {
            _heatMapConfigs.Append<MapConfig>(new MapConfig());
        }
    }

    private void AddTracker()
    {
        if (GameObject.Find("HeatMapper") == null)
        {
            new GameObject("HeatMapper");
        }
    }
}
