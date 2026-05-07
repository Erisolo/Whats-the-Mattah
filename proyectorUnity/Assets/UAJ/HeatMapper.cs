using System.Linq;
using UnityEditor;
using UnityEngine;

public class HeatMapper : EditorWindow {

    private int _selectedMapType = 0;

    [Header("VISUALIZAR")]
    bool ver;

    [Header("CREAR")]
    string mapBaseName = "";

    MapConfig[] _heatMapConfigs;


    [MenuItem("Tools/Basic Object Spawner")]
    public static void ShowWindow() {
        GetWindow(typeof(HeatMapper));
    }

    private void OnEnable()
    {
        _heatMapConfigs = new MapConfig[0];
    }

    private void OnGUI() {

        var style = new GUIStyle(EditorStyles.boldLabel)
        {
            normal = { textColor = new Color(0.95f, 0.7f, 0.84f) }
        };
        EditorGUILayout.LabelField("HEATMAPPER", style);

        if (GUILayout.Button("Add Tracker To Scene"))
            AddTracker();

        mapBaseName = EditorGUILayout.TextField("Base Name", mapBaseName);

        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Add Map"))
            AddMap();
        _selectedMapType = EditorGUILayout.Popup(
            (int) _selectedMapType,
            System.Enum.GetNames(typeof(TrackEventType))
        );

        EditorGUILayout.EndHorizontal();

        for (int i = _heatMapConfigs.Length - 1; i >= 0; i--)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            MapConfig m = _heatMapConfigs[i];

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m.GetType().Name, EditorStyles.boldLabel);
            bool deleted = GUILayout.Button("✕");
            EditorGUILayout.EndHorizontal();
            
            if (deleted)
            {
                ArrayUtility.RemoveAt(ref _heatMapConfigs, i);
                EditorGUILayout.EndVertical();
                continue;
            }

            m.visible = EditorGUILayout.Toggle("Visible", m.visible);
            m.mapName = EditorGUILayout.TextField("Map Name", m.mapName);
            m.tr = EditorGUILayout.ObjectField("Transform to Register", m.tr, typeof(Transform), true) as Transform;
        
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndVertical();
    }

    private void AddTracker()
    {
        if (GameObject.Find("HeatMapper") == null)
        {
            new GameObject("HeatMapper");
        }
    }

    private void AddMap()
    {
        MapConfig newConfig;
        switch ((TrackEventType)_selectedMapType)
        { // assign chosen type
            case TrackEventType.Transform:
                newConfig = new TransformConfig();
                break;
            case TrackEventType.InputKey:
                newConfig = new InputKeyConfig();
                break;
            case TrackEventType.InputMouse:
                newConfig = new InputMouseConfig();
                break;
            default:
                newConfig = new MapConfig();
                break;
        }
        ArrayUtility.Add(ref _heatMapConfigs, newConfig);
    }
}
