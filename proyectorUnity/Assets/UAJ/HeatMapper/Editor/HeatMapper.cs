using System.Linq;
using UnityEditor;
using UnityEngine;

// Herramienta
// Ventana del editor, tracker, configuraciones, visualizador, generacion de mapas, guardado de sesiones, combinacion de heatmaps
// Usuario puede crear un tracker en la escena, seleccionar un area, añadir heatmaps, elegir objetos a trackear, elegir eventos, visualizar mapas generados

// HeatMapConfig; configuracion de cada heatmap
// HeatMapData: Dato guardados de un heatmap
// HeatMapCell: Informacion de una celda
// HeatMapVisualizer: Dibuja los heatmaps en la escena
public class HeatMapper : EditorWindow {

    private int _selectedMapType = 0;

    [Header("VISUALIZAR")]
    bool ver;

    [Header("CREAR")]
    private HeatMapperTracker selectedTracker;
    string mapBaseName = "";

    MapConfig[] _heatMapConfigs;
    //private List<MapConfig> _heatMapConfigs = new List<MapConfig> ();

    [MenuItem("Tools/HeatMapper")]
    public static void ShowWindow() {
        //GetWindow(typeof(HeatMapper));
        GetWindow<HeatMapper>("HeatMapper");
    }

    private void OnEnable()
    {
        _heatMapConfigs = new MapConfig[0];
    }

    private void OnGUI() {

        GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
        {
            normal = { textColor = new Color(0.95f, 0.7f, 0.84f) }
        };
        EditorGUILayout.LabelField("HEATMAPPER", style);

        EditorGUILayout.Space();

        if (GUILayout.Button("Add Tracker To Scene"))
            AddTracker();

        EditorGUILayout.Space();

        selectedTracker = EditorGUILayout.ObjectField(
            "Tracker",
            selectedTracker,
            typeof(HeatMapperTracker),
            true
            ) as HeatMapperTracker;

        if (selectedTracker == null)
        {
            EditorGUILayout.HelpBox("Selecciona o crea un HeatMapperTracker para configurar la herramienta.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();

        DrawTrackerSettings();

        EditorGUILayout.Space();

        DrawHeatMapConfigs();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(selectedTracker);
            SceneView.RepaintAll();
        }

        //mapBaseName = EditorGUILayout.TextField("Base Name", mapBaseName);

        //EditorGUILayout.Space();
        //EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        //EditorGUILayout.BeginHorizontal();

        //if (GUILayout.Button("Add Map"))
        //    AddMap();
        //_selectedMapType = EditorGUILayout.Popup(
        //    (int) _selectedMapType,
        //    System.Enum.GetNames(typeof(TrackEventType))
        //);

        //EditorGUILayout.EndHorizontal();

        //for (int i = _heatMapConfigs.Length - 1; i >= 0; i--)
        //{
        //    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        //    MapConfig m = _heatMapConfigs[i];

        //    EditorGUILayout.Space();
        //    EditorGUILayout.BeginHorizontal();
        //    EditorGUILayout.LabelField(m.GetType().Name, EditorStyles.boldLabel);
        //    bool deleted = GUILayout.Button("✕");
        //    EditorGUILayout.EndHorizontal();
            
        //    if (deleted)
        //    {
        //        ArrayUtility.RemoveAt(ref _heatMapConfigs, i);
        //        EditorGUILayout.EndVertical();
        //        continue;
        //    }

        //    m.visible = EditorGUILayout.Toggle("Visible", m.visible);
        //    m.mapName = EditorGUILayout.TextField("Map Name", m.mapName);
        //    m.tr = EditorGUILayout.ObjectField("Transform to Register", m.tr, typeof(Transform), true) as Transform;
        
        //    EditorGUILayout.EndVertical();
        //}

        //EditorGUILayout.EndVertical();
        //if (GUILayout.Button("AddMap")) {
        //    _heatMapConfigs.Append<MapConfig>(new MapConfig());
        //    //_heatMapConfigs = _heatMapConfigs.Append(new MapConfig()).ToArray();
        //   // _heatMapConfigs.Add(new MapConfig());
        //}
    }

    private void AddTracker()
    {
        //if (GameObject.Find("HeatMapper") == null) {
        //    GameObject hm = new GameObject("HeatMapper");
        //    hm.AddComponent<HeatMapArea>();
        //}

        GameObject hm = GameObject.Find("HeatMapper");

        if (hm != null)
        {
            selectedTracker = hm.GetComponent<HeatMapperTracker>();

            if (selectedTracker == null)
            {
                selectedTracker = hm.AddComponent<HeatMapperTracker>();
            }

            Selection.activeGameObject = hm;
            return;
        }

        GameObject gameObject = new GameObject("HeatMapper");
        selectedTracker = gameObject.AddComponent<HeatMapperTracker>();

        Selection.activeGameObject = gameObject;
    }

    private void DrawTrackerSettings()
    {

        EditorGUILayout.LabelField("Tracking Area", EditorStyles.boldLabel);

        Undo.RecordObject(selectedTracker, "Modify HeatMapper Tracker");

        selectedTracker.areaSize = EditorGUILayout.Vector2Field("Area Size", selectedTracker.areaSize);
        selectedTracker.cellSize = EditorGUILayout.FloatField("Cell Size", selectedTracker.cellSize);

        if (selectedTracker.cellSize <= 0f)
        {
            selectedTracker.cellSize = 0.1f;
        }

       // EditorUtility.SetDirty(selectedTracker);
    }

    private void DrawHeatMapConfigs()
    {
        EditorGUILayout.LabelField("Heatmaps", EditorStyles.boldLabel);

        mapBaseName = EditorGUILayout.TextField("Base Name", mapBaseName);

        for (int i = 0; i < selectedTracker.heatMapConfigs.Count; i++)
        {
            MapConfig config = selectedTracker.heatMapConfigs[i];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(config.mapName, EditorStyles.boldLabel);


            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                Undo.RecordObject(selectedTracker, "Remove Heatmap");
                selectedTracker.heatMapConfigs.RemoveAt(i);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }

            EditorGUILayout.EndHorizontal();

            config.visible = EditorGUILayout.Toggle("Visible", config.visible);
            config.mapName = EditorGUILayout.TextField("Map Name", config.mapName);
            config.eventType =(TrackEventType)EditorGUILayout.EnumPopup("Event Type", config.eventType);

            if (config.eventType == TrackEventType.Transform || config.eventType == TrackEventType.InputKey)
            {
                config.tr = EditorGUILayout.ObjectField("Transform to Register", config.tr, typeof(Transform), true) as Transform;
            }
            if (config.eventType == TrackEventType.InputKey)
            {
                config.inputKey = (KeyCode)EditorGUILayout.EnumPopup("Input Key", config.inputKey);
            }

            config.color = EditorGUILayout.ColorField("Color", config.color);
            config.sampleInterval = EditorGUILayout.FloatField("Sample Interval", config.sampleInterval);

            if (config.sampleInterval <= 0f)
            {
                config.sampleInterval = 0.01f;
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Add Heatmap"))
        {
            Undo.RecordObject(selectedTracker, "Add Heatmap");

            MapConfig newConfig = new MapConfig()
            {
                mapName = mapBaseName + "_" + selectedTracker.heatMapConfigs.Count
            };

            selectedTracker.heatMapConfigs.Add(newConfig);
        }
    }

    //private void AddMap()
    //{
    //    MapConfig newConfig;
    //    switch ((TrackEventType)_selectedMapType)
    //    { // assign chosen type
    //        case TrackEventType.Transform:
    //            newConfig = new TransformConfig();
    //            break;
    //        case TrackEventType.InputKey:
    //            newConfig = new InputKeyConfig();
    //            break;
    //        case TrackEventType.InputMouse:
    //            newConfig = new InputMouseConfig();
    //            break;
    //        default:
    //            newConfig = new MapConfig();
    //            break;
    //    }
    //    ArrayUtility.Add(ref _heatMapConfigs, newConfig);
    //}
}
