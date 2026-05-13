using UnityEditor;
using UnityEngine;

// HeatMapConfig; configuracion de cada heatmap
// HeatMapData: Dato guardados de un heatmap
// HeatMapCell: Informacion de una celda
// HeatMapVisualizer: Dibuja los heatmaps en la escena

// Ventana principal de la herramienta HeatMapper
// Permite crear un HeatMapperTracker en la escena y configurar:
// - Area de tracking
// - Tamanio de casilla
// - Heatmaps a generar
// - Tipo de evento de cada heatmap
// - Objeto o input asocaido
public class HeatMapper : EditorWindow {

    private int _selectedMapType = 0;

    [Header("VISUALIZAR")]
    bool ver;

    [Header("CREAR")]

    // Tracker actual seleccionado. 
    // Componente de la escena que realizara el tracking durante Play Mode
    private HeatMapperTracker selectedTracker;

    // Nombre base al crear nuevos heatmaps
    string mapBaseName = "";

    MapConfig[] _heatMapConfigs;
    //private List<MapConfig> _heatMapConfigs = new List<MapConfig> ();

    // Aniade la ventana al menu superior de Unity. Tools > HeatMapper
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

        // Campo para seleccionar manualmente un HeatMapperTracker existente en la escena
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

        // Si se ha modificado algo en la ventana, se marca el tracker como modificado
        // y se fuerza el repintado en la Scene View
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

    // Crea un objeto HeatMapper en la escena si no existe
    // Si existe, lo selecciona y se asegura de que tenga un HeatMapperTracker
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

    // Dibuja los campos generales del area de tracking:
    // tamanio total del area y tamanio de cada casilla
    private void DrawTrackerSettings()
    {

        EditorGUILayout.LabelField("Tracking Area", EditorStyles.boldLabel);

        Undo.RecordObject(selectedTracker, "Modify HeatMapper Tracker");

        selectedTracker.areaSize = EditorGUILayout.Vector2Field("Area Size", selectedTracker.areaSize);
        selectedTracker.cellSize = EditorGUILayout.FloatField("Cell Size", selectedTracker.cellSize);

        // Evitar que el tamanio de casilla sea 0 o negativo
        if (selectedTracker.cellSize <= 0f)
        {
            selectedTracker.cellSize = 0.1f;
        }

       // EditorUtility.SetDirty(selectedTracker);
    }

    // Dibuja la lista de heatmaps configurados en el tracker.
    // Se pueden aniadir, eliminar y modificar mapas
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
            config.color = EditorGUILayout.ColorField("Color",config.color);
            config.sampleInterval = EditorGUILayout.FloatField("Sample Interval", config.sampleInterval);
           
            if(config.sampleInterval <= 0f)
            {
                config.sampleInterval = 0.01f;
            }

            if (config.eventType == TrackEventType.Transform)
            {
                config.tr = EditorGUILayout.ObjectField("Transform to Register", config.tr, typeof(Transform), true) as Transform;
            }
            if (config.eventType == TrackEventType.InputKey)
            {
                config.tr = EditorGUILayout.ObjectField("Transform to Register", config.tr, typeof(Transform), true) as Transform;
                config.inputKey = (KeyCode)EditorGUILayout.EnumPopup("Input Key", config.inputKey);
            }
            if(config.eventType == TrackEventType.InputMouse)
            {
                config.mouseButton = (MouseButton)EditorGUILayout.EnumPopup("Mouse Button", config.mouseButton);
            }
            if (config.eventType == TrackEventType.ListTransform)
            {
                EditorGUILayout.LabelField("Transforms", EditorStyles.miniBoldLabel);
                for (int t = 0; t < config.transformList.Count; t++)
                {
                    EditorGUILayout.BeginHorizontal();
                    config.transformList[t] = EditorGUILayout.ObjectField(config.transformList[t], typeof(Transform), true) as Transform;
                    if (GUILayout.Button("-", GUILayout.Width(22)))
                    {
                        config.transformList.RemoveAt(t);
                        EditorGUILayout.EndHorizontal();
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if(GUILayout.Button("+ Add Transform"))
                {
                    config.transformList.Add(null);
                }
            }
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
