using System;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Debug")]
    // Para mostrar u ocultar el rectangulo del area de tracking en la Scene View
    // Sirve para mostrar visualmente que zona del nivel esta cubriendo HeatMapper
    public bool showTrackingArea = true;
    // Para mostrar u ocultar la cuadricula de celdas en la Scene View
    // TODO comprobar: Es util para comprobar la granularidad configurada mediante cellSize
    public bool showGrid = false;

    [Header("Editor Debug")]
    // Para activar o desactivar los handles de edicion del area en la Scene View
    // Cuando esta activo, el usuario puede redimensionar visualmente el area del tracker
    public bool showAreaEditor = true;

    [Header("Cell Inspector")]
    // Permite activar o desactivar la inpeccion de celdas
    public bool enableCellInspector = false;
    // Para mostrar solo en los heatmpas que esten activos
    public bool showOnlyVisibleHeatMaps = true;

    [Header("Visualization")]

    public HeatmapVisualizer heatMapVisualizer;

    [Header("Heatmaps")]
    // Lista de configuraciones de heatmaps.
    // Cada configuracion define que evento se trackea, que objeto se registra y como se visualiza
    public List<MapConfig> heatMapConfigs = new List<MapConfig>();
    
    // Diccionario que guarda los heatmaps generados. 
    // La clave es el nombre del mapa y el valor es su matriz de calor
    private Dictionary<string, HeatMap> _heatMaps = new Dictionary<string, HeatMap>();

    // Lista interna para las configuraciones validas, descartando incorrectas como mapas sin nombre
    // o mapas con nombres duplicados. Util para evitar errores por claves repetidas en los diccionarios.
    private List<MapConfig> _validHeatMapConfigs = new List<MapConfig>();

    // Temporizadores individuales para cada heatmap
    // Se usan para controlar cada cuanto se registra la posicion
    private Dictionary<string, float> _timers = new Dictionary<string,float>();


    private void Start() {
        string CurrentSessionId = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        HeatMapSerializer.calculateSavingPath(CurrentSessionId);


        // Generar los mapas segun configuraciones
        GenerateHeatMaps();
        
        // Si hay visualizer asignado crea los tilemaps que haya
        if (heatMapVisualizer != null) {
            Grid grid = heatMapVisualizer.GetComponentInChildren<Grid>();
            if(grid != null ) {
                heatMapVisualizer.setGrid(grid);
                //grid.cellSize = new Vector3(cellSize, cellSize, 1.0f);
            }

            // Crea los tilemaps por cada config
            foreach(MapConfig config in _validHeatMapConfigs) {
                heatMapVisualizer.createTileMap(config);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (KeyValuePair<string, HeatMap> entry in _heatMaps)
        {
            HeatMapSerializer.SaveToFile(entry.Value, entry.Key);
        }
            
    }

    //private void OnApplicationQuit()
    //{
    //    foreach (KeyValuePair<string, HeatMap> entry in _heatMaps)
    //    {
    //        HeatMapSerializer.SaveToFile(entry.Value, entry.Key);
    //        Debug.Log("saving");
    //    }

    //}

    private void Update()
    {
        // Se revisa cada config y se ejecuta el tipo de tracking correspondiente
        foreach (MapConfig config in _validHeatMapConfigs)
        {
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

            // Actualiza el visualizer (lo del tilemap)

            //if (heatMapVisualizer != null) {
            //    if (_heatMaps.ContainsKey(config.mapName)) { // Si tiene el heatmap con el nombre en el dictionary
            //        // updatea los tilemaps segun el heatmap del dictionarty y el config
            //        heatMapVisualizer.updateTileMap(_heatMaps[config.mapName], config);
            //    }
            //}
        }
    }

    // Crea un heatmap por cada configuracion valida definida en la tool
    // Cada heatmap tiene el mismo area y granularidad, pero registra datos distintos
    // Las configuraciones invalidas se ignoran para evitar errores durante el tracking
    // o durante la creacion de tilemaps en el visualizador.
    private void GenerateHeatMaps()
    {
        if(cellSize <= 0f)
        {
            cellSize = 0.1f;
        }

        // Limpiar datos anteriores antes de generar una nueva sesion
        _heatMaps.Clear();
        _timers.Clear();
        _validHeatMapConfigs.Clear();

        // Calcular cuantas casillas caben en el area segun el tamanio de casilla
        int width = Mathf.CeilToInt(areaSize.x / cellSize);
        int height = Mathf.CeilToInt(areaSize.y / cellSize);

        // Calcular la esquina superior izquierda del area de tracking que se usa como origen logico del heatmap
        Vector2 topLeft = new Vector2(
            transform.position.x - areaSize.x / 2f,
            transform.position.y + areaSize.y / 2f);

        foreach (MapConfig config in heatMapConfigs)
        {
            if (config == null) continue;

            // Comprobar que el nombre del mapa no este vacio
            if (string.IsNullOrEmpty(config.mapName))
            {
                Debug.LogWarning("Heatmap sin nombre");
                continue;
            }

            // Comprobar que no exista ya otro heatmap con el mismo nombre
            if (_heatMaps.ContainsKey(config.mapName))
            {
                Debug.LogWarning("Ya existe un heatmap con el nombre: " +  config.mapName);
                continue;
            }


            // Solo se crea el mapa si no existe ya otro con el mismo nombre
            HeatMap heatMap = new HeatMap(width, height, cellSize, topLeft);

            _heatMaps.Add(config.mapName, heatMap);
            // Temporizador propio
            _timers.Add(config.mapName, 0f);
            // Aniadir configuracion a la lista de configuraciones validas
            _validHeatMapConfigs.Add(config);

            //Debug.Log($"HeatMapper: creado heatmap '{config.mapName}' con tamaño {width}x{height} y cellSize {cellSize}");
        }
    }

    // Devuelve los heatmaps generados actualmente. Unicamente permite consultar, no modificar
    public IReadOnlyDictionary<string, HeatMap> GetHeatMaps() {
        return _heatMaps;
    }

    // Devuelve la lista de configuraciones validas. Unicamente permite consultar, no modificar
    public IReadOnlyList<MapConfig> GetValidHeatMapConfigs()
    {
        return _validHeatMapConfigs;
    }
    // Registra periodicamente la posicion del Transform indicando en la configuracion
    private void TrackTransform(MapConfig config)
    {
        if (config.tr == null) return;
        if(!_timers.ContainsKey(config.mapName)) return; 

        _timers[config.mapName] += Time.deltaTime;

        if (_timers[config.mapName] < config.sampleInterval) return;

        _timers[config.mapName] = 0f;

        RegisterPosition(config.mapName, config.tr.position);
    }

    // Registra la posicion del Transform cuando se pulsa una tecla concreta
    private void TrackInputKey(MapConfig config)
    {
        if (config.tr == null) return;
        if(!_heatMaps.ContainsKey(config.mapName)) return;

        if (Input.GetKeyDown(config.inputKey))
        {
            RegisterPosition(config.mapName, config.tr.position);
        }
    }

    // Registra la posicion del raton en el mundo cuando se hace click izquierdo
    private void TrackMouse (MapConfig config)
    {
        if(!_heatMaps.ContainsKey(config.mapName)) return;

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
        if(!_timers.ContainsKey(config.mapName)) return;
     
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
        if (!_heatMaps.ContainsKey(mapName))
        {
            Debug.LogWarning($"HeatMapper: no existe el heatmap '{mapName}'");
            return;
        }

        HeatMap heatMap = _heatMaps[mapName];

        Vector2Int tile = heatMap.WorldToTile(worldPosition);

        if (!heatMap.IsInside(tile.x, tile.y))
        {
            Debug.Log($"HeatMapper: posicion fuera del area en mapa '{mapName}'. Posicion: {worldPosition}, celda: {tile}");
            return;
        }

        heatMap.AddHeatMapValue(tile);

        //Vector2Int cell = WorldToCell(worldPosition);

        //if (!IsInsideArea(cell)) return;

        //if (!_heatMaps[mapName].ContainsKey(cell))
        //    _heatMaps[mapName][cell] = 0;

        //_heatMaps[mapName][cell]++;
    }

    // Ajusta el tamanio del area de tracking para que sea multiplo exacto del tamanio de celda
    // Evita que la cuadricula sobresalga parcialmente del area cuando areaSize no es divisible entre cellSize
    public void SnapAreaToGrid()
    {
        if (cellSize <= 0f)
        {
            cellSize = 0.1f;
        }

        // Para asegurar que el area original quede completamente cubierta
        int width = Mathf.CeilToInt(areaSize.x / cellSize);
        int height = Mathf.CeilToInt(areaSize.y / cellSize);    

        areaSize = new Vector2(width * cellSize, height * cellSize);
    }

    // Convertir posicion del mundo a celda
    public bool GetCellAtWorldPosition(Vector3 worldPosition, out Vector2Int cell)
    {
        cell = Vector2Int.zero;

        if (_heatMaps == null || _heatMaps.Count == 0)
        {
            return false;
        }

        foreach (HeatMap heatMap in _heatMaps.Values)
        {
            cell = heatMap.WorldToTile(worldPosition);

            return heatMap.IsInside(cell.x, cell.y);
        }

        return false;
    }

    // Obtener el centro de una celda, Sirve para mostrar el popup justo en la celda seleccionada
    public bool GetCellWorldCenter(Vector2Int cell, out Vector3 worldCenter)
    {
        worldCenter = Vector3.zero;

        if(_heatMaps == null || _heatMaps.Count == 0)
        {
            return false;
        }

        foreach (HeatMap heatMap in _heatMaps.Values)
        {
            if(!heatMap.IsInside(cell.x, cell.y))
            {
                return false;
            }

            worldCenter = heatMap.TileToWorldCenter(cell.x, cell.y, transform.position.z);

            return true;
        }
        return false;
    }

    // Obtener informacion de la celda
    public List<string> GetCellInfo(Vector2Int cell)
    {
        List<string> info = new List<string>();

        if (_heatMaps == null || _heatMaps.Count == 0)
        {
            return info;
        }

        foreach (MapConfig config in _validHeatMapConfigs)
        {
            if (config == null) continue;

            if (showOnlyVisibleHeatMaps && !config.visible)
            {
                continue;
            }

            if (!_heatMaps.ContainsKey(config.mapName))
            {
                continue;
            }

            HeatMap heatMap = _heatMaps[config.mapName];
            if(!heatMap.IsInside(cell.x, cell.y))
            {
                continue;
            }

            int value = heatMap.GetHeatMapValue(cell.x, cell.y);

            // Si el valor de la celda es 0, no se muestra informacion
            if (value <= 0)
            {
                continue;
            }

            // Valor maximo del heatmap completo
            int maxValue = heatMap.GetMaxValue();

            // Porcentaje de calor de esta celda respecto a la celda con mas calor del heatmap
            float normalizedValue = 0f;

            if (maxValue > 0)
            {
                normalizedValue = (float)value / maxValue;
            }

            float percentage = normalizedValue * 100f;

            string targetName = "None";

            if (config.tr != null)
            {
                targetName = config.tr.name;
            }

            string line =
                $"{config.mapName}\n" +
                $"Value: {value}\n" +
                $"Heat: {percentage:F1}%\n" +
                $"Event: {config.eventType}\n" +
                $"Target: {targetName}";
                
            info.Add(line);
        }

        return info;
    }
    // TODO borrar en el futuro, sirve para debug con GIZMOS
    // Dibuja en la Scene View el area, la cuadricula y los heatmaps
    private void OnDrawGizmos()
    {
        // TODO lo he comentado para poder ver bien el tilemap
        if(showTrackingArea)
        {
            DrawTrackingArea();
        }
        if (showGrid)
        {
            DrawGrid();
        }


        //// Solo se dibuja los datos mientras el juego esta ejecutandose,
        //// porque los datos se guardan durante Play Mode
        //if (Application.isPlaying)
        //{
        //    DrawHeatmaps();
        //    //DrawRuntimeHeatmaps();
        //}
    }

    // TODO borrar en el futuro, sirve para debug con GIZMOS
    // Dibuja el rectangulo que delimita el area cubierta por el tracker
    private void DrawTrackingArea()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, areaSize.y, 0.1f));
    }

    // TODO borrar en el futuro, sirve para debug con GIZMOS
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
}




