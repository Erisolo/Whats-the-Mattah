using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

// Componente principal que se coloca en la escena. Se encarga de:
// - Definir el area de tracking
// - Crear los heatmaps configurados
// - Registrar eventos durante Play Mode
// - Dibujar la cuadricula y los heatmaps con Gizmos
public class HeatMapperTracker : MonoBehaviour
{
    [Header("Tracking area")]

    // Tamanio total del area que cubrira el tracker, X (ancho) e Y (alto)
    public Vector2 areaSize = new Vector2(20f, 10f);
    
    // Tamanio de cada casilla del heatmap.
    public float cellSize = 1f;

    [Header("Heatmaps")]

    // Lista de configuraciones de heatmaps.
    // Cada configuracion define que evento se trackea, que objeto se registra y como se visualiza
    public List<MapConfig> heatMapConfigs = new List<MapConfig>();
    
    // Diccionario que guarda los heatmaps generados. 
    // La clave es el nombre del mapa y el valor es su matriz de calor
    private Dictionary<string, HeatMap> _heatMaps = new Dictionary<string, HeatMap>();

    // Temporizadores individuales para cada heatmap
    // Se usan para controlar cada cuanto se registra la posicion
    private Dictionary<string, float> _timers = new Dictionary<string,float>();

    [Header("Visualization")]

    public HeatmapVisualizer heatMapVisualizer;

    private void Start()
    {
        // Generar los mapas segun configuraciones
        GenerateHeatMaps();
        
        // si hay visualizer asignado crea los tilemaps k haya
        if (heatMapVisualizer != null) {
            Grid grid = heatMapVisualizer.GetComponentInChildren<Grid>();
            if(grid != null ) {
                heatMapVisualizer.setGrid(grid);
                // Calcular la esquina superior izquierda del area de tracking
                Vector2 topLeft = new Vector2(
                    transform.position.x - areaSize.x / 2f,
                    transform.position.y + areaSize.y / 2f);
                grid.transform.position = topLeft;
                grid.cellSize = new Vector3(cellSize, cellSize, 1.0f);
            }

            // crea los tilemaps por cada config
            foreach(MapConfig config in heatMapConfigs) {
                heatMapVisualizer.createTileMap(config);
            }
        }
    }

    private void Update()
    {
        // Se revisa cada config y se ejecuta el tipo de tracking correspondiente
        foreach (MapConfig config in heatMapConfigs)
        {
            if (string.IsNullOrEmpty(config.mapName)) continue;

            switch(config.eventType)
            {
                case TrackEventType.Transform:
                    TrackTransform(config);
                    break;

                case TrackEventType.InputKey:
                    TrackInputKey(config); 
                    break;

                case TrackEventType.InputMouse:
                    TrackMouse(config);
                    break;

                case TrackEventType.ListTransform:
                    TrackListTransform(config); 
                    break;

                // custom
            }

            // actualiza el visualizer (lo del tilemap)
            if (heatMapVisualizer != null) {
                if (_heatMaps.ContainsKey(config.mapName)) { // si tiene el heatmap con el nombre en el dictionary
                    // updatea los tilemaps segun el heatmap del dictionarty y el config
                    heatMapVisualizer.updateTileMap(_heatMaps[config.mapName], config);
                }
                
            }
        }
    }

    // Crea un heatmap por cada configuracion definida en la tool
    // Cada heatmao tiene el mismo area y granularidad, pero registra datos distintos
    private void GenerateHeatMaps()
    {
        _heatMaps.Clear();
        _timers.Clear();

        // Calcular cuantas casillas caben en el area segun el tamanio de casilla
        int width = Mathf.CeilToInt(areaSize.x / cellSize);
        int height = Mathf.CeilToInt(areaSize.y / cellSize);

        // Calcular la esquina superior izquierda del area de tracking
        Vector2 topLeft = new Vector2(
            transform.position.x - areaSize.x / 2f,
            transform.position.y + areaSize.y / 2f);

        foreach (MapConfig config in heatMapConfigs)
        {
            if (string.IsNullOrEmpty (config.mapName)) continue;    

            // Solo se crea el mapa si no existe ya otro con el mismo nombre
            if (!_heatMaps.ContainsKey(config.mapName))
            {
                _heatMaps.Add(config.mapName, new HeatMap(width, height, cellSize, topLeft));
                _timers.Add(config.mapName, 0f);
            }
        }
    }

    // Registra periodicamente la posicion del Transform indicando en la configuracion
    private void TrackTransform(MapConfig config)
    {
        if (config.tr == null) return;

        _timers[config.mapName] += Time.deltaTime;

        if (_timers[config.mapName] < config.sampleInterval) return;

        _timers[config.mapName] = 0f;

        RegisterPosition(config.mapName, config.tr.position);
    }

    // Registra la posicion del Transform cuando se pulsa una tecla concreta
    private void TrackInputKey(MapConfig config)
    {
        if (config.tr == null) return;

        if (Input.GetKeyDown(config.inputKey))
        {
            RegisterPosition(config.mapName, config.tr.position);
        }
    }

    // Registra la posicion del raton en el mundo cuando se hace click izquierdo
    private void TrackMouse (MapConfig config)
    {
        if (!Input.GetMouseButtonDown((int)config.mouseButton)) return;   

        Camera camera = Camera.main;
        if (camera == null) return;

        Vector3 mouseWorld = camera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        RegisterPosition(config.mapName, mouseWorld);
    }
    
    private void TrackListTransform(MapConfig config)
    {
        if (config.transformList == null || config.transformList.Count == 0) return;

        _timers[config.mapName] += Time.deltaTime;
        if (_timers[config.mapName] < config.sampleInterval) return;
        _timers[config.mapName] = 0f;
        foreach (Transform t in config.transformList) {
            if (t != null) RegisterPosition(config.mapName, t.position);
        }
    }
    
    // Permite registrar un evento manualmente desde otro script
    public void RegisterCustomEvent(string mapName, Vector3 worldPosition)
    {
        RegisterPosition(mapName, worldPosition);
    }

    // Registra una posicion del mundo dentro del heatmap correspondiente
    // Convierte la posicion a casilla y aumenta la intensidad de esa casilla
    private void RegisterPosition(string mapName, Vector3 worldPosition)
    {
        if (!_heatMaps.ContainsKey(mapName)) return;

        HeatMap heatMap = _heatMaps[mapName];

        Vector2Int tile = heatMap.WorldToTile(worldPosition);

        heatMap.AddHeatMapValue(tile);

        //Vector2Int cell = WorldToCell(worldPosition);

        //if (!IsInsideArea(cell)) return;

        //if (!_heatMaps[mapName].ContainsKey(cell))
        //    _heatMaps[mapName][cell] = 0;

        //_heatMaps[mapName][cell]++;
    }

    // Dibuja en la Scene View el area, la cuadricula y los heatmaps
    private void OnDrawGizmos()
    {
        DrawTrackingArea();
        DrawGrid();

        // Solo se dibuja los datos mientras el juego esta ejecutandose,
        // porque los datos se guardan durante Play Mode
        if (Application.isPlaying)
        {
            DrawHeatmaps();
            //DrawRuntimeHeatmaps();
        }
    }

    // Dibuja el rectangulo que delimita el area cubierta por el tracker
    private void DrawTrackingArea()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, areaSize.y, 0.1f));
    }

    // Dibuja la cuadricula completa del area de tracking
    // Cada casilla representa una posible posicion del heatmap
    private void DrawGrid()
    {
        if (cellSize <= 0f) return;

        int width = Mathf.CeilToInt(areaSize.x / cellSize);
        int height = Mathf.CeilToInt(areaSize.y / cellSize);

        Vector2 topLeft = new Vector2(
            transform.position.x - areaSize.x / 2f,
            transform.position.y + areaSize.y / 2f);

        Gizmos.color = new Color(1f, 1f, 1f, 0.25f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 center = new Vector3(
                    topLeft.x + x * cellSize + cellSize / 2f,
                    topLeft.y - y * cellSize - cellSize / 2f,
                    transform.position.z);

                Gizmos.DrawWireCube(center, new Vector3(cellSize, cellSize, 0.05f));
            }
        }
    }

    // Dibuja las casillas con calor acumulado
    // Cuanto mayor sea el valor de una celda, mayor sera su intensidad visual
    private void DrawHeatmaps()
    {
        foreach (MapConfig config in heatMapConfigs)
        {
            if (!config.visible) continue;
            if (!_heatMaps.ContainsKey(config.mapName)) continue;

            HeatMap heatMap = _heatMaps[config.mapName];

            for(int x = 0; x < heatMap.GetWidth(); x++)
            {
                for(int y=0; y < heatMap.GetHeight(); y++)
                {
                    int value = heatMap.GetHeatMapValue(x, y);

                    if (value <= 0) continue;

                    // Normalizacion simple para convertir el valor de la celda en una intensidad entre 0 y 1
                    float intensity = Mathf.Clamp01(value / 20f);

                    Color c = config.color;
                    c.a = intensity;

                    Gizmos.color = c;

                    Vector3 center = heatMap.TileToWorldCenter(x, y, transform.position.z);

                    Gizmos.DrawCube(center, new Vector3(cellSize, cellSize, 0.05f));

                }
            }
        }
    }
    //private void DrawRuntimeHeatmaps()
    //{
    //    if (_heatMaps == null) return;

    //    foreach (MapConfig config in heatMapConfigs)
    //    {
    //        if (!config.visible) continue;
    //        if (!_heatMaps.ContainsKey(config.mapName)) continue;

    //        Dictionary<Vector2Int, int> cells = _heatMaps[config.mapName];

    //        foreach (KeyValuePair<Vector2Int, int> cell in cells)
    //        {
    //            Vector3 cellCenter = CellToWorld(cell.Key);

    //            float intensity = Mathf.Clamp01(cell.Value / 10f);

    //            Color c = config.color;
    //            c.a = intensity * config.color.a;

    //            Gizmos.color = c;
    //            Gizmos.DrawCube(cellCenter, new Vector3(cellSize, cellSize, 0.1f));
    //        }
    //    }
    //}
}




