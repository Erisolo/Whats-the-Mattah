using System.Collections.Generic;
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

    private Vector2 scrollPosition;
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
        HeatMapper window = GetWindow<HeatMapper>();
        window.titleContent = new GUIContent("HeatMapper");
    }

    private void OnEnable()
    {
        FindTrackerInScene();
        _heatMapConfigs = new MapConfig[0];
    }

    private void OnGUI() {
        FindTrackerInScene();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

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

        // Boton de recuperacion del tracker manual.
        if(GUILayout.Button("Find Tracker In Scene"))
        {
            selectedTracker = null;
            FindTrackerInScene();
        }

        if (selectedTracker == null)
        {
            EditorGUILayout.HelpBox("Selecciona o crea un HeatMapperTracker para configurar la herramienta.", MessageType.Info);
            return;
        }


        EditorGUILayout.Space();

        HeatMapConfigDrawer.DrawTrackerSettings(selectedTracker);

        EditorGUILayout.Space();

        HeatMapConfigDrawer.DrawHeatMapConfigs(selectedTracker);

        // Si se ha modificado algo en la ventana, se marca el tracker como modificado
        // y se fuerza el repintado en la Scene View
        if (GUI.changed)
        {
            EditorUtility.SetDirty(selectedTracker);
            SceneView.RepaintAll();
        }

        EditorGUILayout.EndScrollView();
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

    // Crea o recupera un objeto HeatMapperTracker en la escena 
    private void AddTracker()
    {
        //if (GameObject.Find("HeatMapper") == null) {
        //    GameObject hm = new GameObject("HeatMapper");
        //    hm.AddComponent<HeatMapArea>();
        //}

        // Buscar si ya existe algun HeatMapperTracker en la escena
        // Se evita crear trackers duplicados
#if UNITY_2023_1_OR_NEWER
        HeatMapperTracker existingTracker = Object.FindFirstObjectByType<HeatMapperTracker>();
#else
        HeatMapperTracker existingTracker = Object.FindObjectOfType<HeatMapperTracker>();
#endif

        if (existingTracker != null)
        {
            selectedTracker = existingTracker;
            Selection.activeGameObject = existingTracker.gameObject;

            // Si el tracker ya existia, asegurarse de que tenga visualizer asociado
            AddVisualizer();
            return;
        }

        // Si no existe ningun tracker, se crea un nuevo objeto en la escena
        GameObject trackerObject = new GameObject("HeatMapperTracker");

        selectedTracker = trackerObject.AddComponent<HeatMapperTracker>();

        Selection.activeGameObject = trackerObject;

        // Crea el visualizer junto al tracker
        AddVisualizer();
    }

    // Crea o recupera un HeatmapVisualizer y lo conecta con el tracker seleccionado
    // Si existe, lo seleccione y se asegura que tenga un grid
    private void AddVisualizer() {

        if (selectedTracker == null) return;

        // Si el tracker ya tiene visualizer asignado, se reutiliza
        HeatmapVisualizer hmVisualizer = selectedTracker.heatMapVisualizer;

        // Se busca en la escena si no tiene uno asignado
#if UNITY_2023_1_OR_NEWER
        if (hmVisualizer == null) {
            hmVisualizer = Object.FindFirstObjectByType<HeatmapVisualizer>();
        }
#else
        if(hmVisualizer == null)
        {
            hmVisualizer = Object.FindObjectOfType<HeatmapVisualizer>();
        }
#endif

        // Si no existe ningun visualizer, se crea uno nuevo
        if(hmVisualizer == null)
        {
            GameObject hmvGO = new GameObject("HeatmapVisualizer");
            hmVisualizer = hmvGO.AddComponent<HeatmapVisualizer>();
        }
        //// mira si ya existe y si no lo crea
        //GameObject hmvGO= GameObject.Find("HeatmapVisualizer");
        //if (hmvGO == null) hmvGO = new GameObject("HeatmapVisualizer");

        //// mira si tiene el componente y si no se lo pone
        //HeatmapVisualizer hmVisualizer = hmvGO.GetComponent<HeatmapVisualizer>();
        //if(hmVisualizer == null) hmVisualizer = hmvGO.AddComponent<HeatmapVisualizer>();

        // Buscar si tiene grid dentro del visualizer
        Grid grid = hmVisualizer.GetComponentInChildren<Grid>();

        // Si no existe, se crea como hijo del visualizer
        if (grid == null) {
            GameObject gGO = new GameObject("Grid");
            gGO.transform.SetParent(hmVisualizer.transform);
            grid = gGO.AddComponent<Grid>();
        }
        // Conectar el grid con el visualizer
        hmVisualizer.setGrid(grid);

        // Conectar el tracker con el visualizer
        selectedTracker.heatMapVisualizer = hmVisualizer;
    }

    // Busca automaticamente un HeatMapperTracker existente en la escena
    // Util para recuperar la referencia cuando la ventana de editor la pierde
    private void FindTrackerInScene()
    {
        if (selectedTracker != null) return;

#if UNITY_2023_1_OR_NEWER
            HeatMapperTracker[] trackers = Object.FindObjectsByType<HeatMapperTracker>(
                FindObjectsSortMode.None
            );
#else
        HeatMapperTracker[] trackers = Object.FindObjectsOfType<HeatMapperTracker>();
#endif
        // Si encuentra un tracker, lo asigna como el seleccionado y obtiene su gameObject en la jerarquia
        if (trackers.Length > 0)
        {
            selectedTracker = trackers[0];
            Selection.activeGameObject = selectedTracker.gameObject;
        }
    }
}
