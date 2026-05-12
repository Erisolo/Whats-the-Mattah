using System.Collections.Generic;
using UnityEngine;

// Crear un tracker
// Definir area
// Definir granularidad
// Asignar un objeto
// Ver celdas pintadas mientras se juega

// Objeto que se coloca en la escena y registra eventos
// Registra datos durante el playmode
public class HeatMapperTracker : MonoBehaviour
{
    [Header("Tracking area")]
    public Vector2 areaSize = new Vector2(20f, 10f);
    public float cellSize = 1f;

    [Header("Heatmaps")]
    public List<MapConfig> heatMapConfigs = new List<MapConfig>();

    private Dictionary<string, HeatMap> _heatMaps = new Dictionary<string, HeatMap>();

    private Dictionary<string, float> _timers = new Dictionary<string,float>();

    private void Start()
    {
        GenerateHeatMaps();
        //foreach (MapConfig config in heatMapConfigs) {
        //    if (string.IsNullOrEmpty(config.mapName))
        //        continue;

        //    _heatMaps[config.mapName] = new Dictionary<Vector2Int, int>();
        //    _timers[config.mapName] = 0f;
        //}
    }

    private void Update()
    {
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
            }
        }
    }

    private void GenerateHeatMaps()
    {
        _heatMaps.Clear();
        _timers.Clear();

        int width = Mathf.CeilToInt(areaSize.x / cellSize);
        int height = Mathf.CeilToInt(areaSize.y / cellSize);

        Vector2 topLeft = new Vector2(
            transform.position.x - areaSize.x / 2f,
            transform.position.y + areaSize.y / 2f);

        foreach (MapConfig config in heatMapConfigs)
        {
            if (string.IsNullOrEmpty (config.mapName)) continue;    

            if (!_heatMaps.ContainsKey(config.mapName))
            {
                _heatMaps.Add(config.mapName, new HeatMap(width, height, cellSize, topLeft));
                _timers.Add(config.mapName, 0f);
            }
        }
    }
    private void TrackTransform(MapConfig config)
    {
        if (config.tr == null) return;

        _timers[config.mapName] += Time.deltaTime;

        if (_timers[config.mapName] < config.sampleInterval) return;

        _timers[config.mapName] = 0f;

        RegisterPosition(config.mapName, config.tr.position);
    }

    private void TrackInputKey(MapConfig config)
    {
        if (config.tr == null) return;

        if (Input.GetKeyDown(config.inputKey))
        {
            RegisterPosition(config.mapName, config.tr.position);
        }
    }

    private void TrackMouse (MapConfig config)
    {
        if (!Input.GetMouseButtonDown(0)) return;   

        Camera camera = Camera.main;
        if (camera == null) return;

        Vector3 mouseWorld = camera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        RegisterPosition(config.mapName, mouseWorld);
    }

    public void RegisterCustomEvent(string mapName, Vector3 worldPosition)
    {
        RegisterPosition(mapName, worldPosition);
    }

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

    private Vector2Int WorldToCell(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - transform.position;

        int x = Mathf.FloorToInt((localPosition.x + areaSize.x / 2f) / cellSize);
        int y = Mathf.FloorToInt((localPosition.y + areaSize.y / 2f) / cellSize);

        return new Vector2Int(x, y);
    }

    private Vector3 CellToWorld(Vector2Int cell)
    {
        float x = transform.position.x - areaSize.x / 2f + cell.x * cellSize + cellSize / 2f;
        float y = transform.position.y - areaSize.y / 2f + cell.y * cellSize + cellSize / 2f;

        return new Vector3(x, y, transform.position.z);
    }

    private bool IsInsideArea(Vector2Int cell)
    {
        int width = Mathf.CeilToInt(areaSize.x / cellSize);
        int height = Mathf.CeilToInt(areaSize.y / cellSize);

        return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
    }

    private void OnDrawGizmos()
    {
        DrawTrackingArea();
        DrawGrid();

        if (Application.isPlaying)
        {
            DrawHeatmaps();
            //DrawRuntimeHeatmaps();
        }
    }

    private void DrawTrackingArea()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, areaSize.y, 0.1f));
    }

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
                    topLeft.y - y * cellSize + cellSize / 2f,
                    transform.position.z);

                Gizmos.DrawWireCube(center, new Vector3(cellSize, cellSize, 0.05f));
            }
        }
    }

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




