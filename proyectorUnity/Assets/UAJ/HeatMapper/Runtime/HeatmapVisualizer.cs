using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class HeatmapVisualizer : MonoBehaviour {
    [SerializeField] private TileBase _tilebase;
    [SerializeField] private Grid _grid;

    private Dictionary<string, Tilemap> _tilemaps = new Dictionary<string, Tilemap>();

    public void setGrid(Grid grid) { _grid = grid; }

    [SerializeField]
    // Lista de combinaciones de heatmaps.
    public List<HeatMapCombineType> heatMapsToVisualize = new List<HeatMapCombineType>();

    //llamado cuando cambia algo en el editor (posiblemente los heatmaps)
#if UNITY_EDITOR
    private void OnValidate()
    {
        // Esto se ejecuta cuando cambias algo en el inspector (como el checkbox visible)
        // Usa EditorApplication.delayCall para evitar problemas de ejecución
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null && _grid != null)
            {
                ClearTilemaps();
                for (int i = 0; i < heatMapsToVisualize.Count; i++)
                {
                    heatMapsToVisualize[i].RefreshHeatmaps();
                    heatMapsToVisualize[i].logicOperation();
                }

                createTileMaps();
            }
        };
    }
#endif
    

    public void createTileMaps()
    {
        for (int i = 0; i < heatMapsToVisualize.Count; i++)
        {
            if (_grid != null)
            {
               
                // crea un nuevo gameobject contenedor de un tilemap con su nombre con identificador y lo hace hijo de grid.
                GameObject tmGO = new GameObject($"Tilemap_{heatMapsToVisualize[i].name}");
                tmGO.transform.SetParent(_grid.transform);

                // pone los componentes
                Tilemap tilemap = tmGO.AddComponent<Tilemap>();
                TilemapRenderer tmRenderer = tmGO.AddComponent<TilemapRenderer>();
                tmRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));

                // para k se pinte por encima de todo
                tmRenderer.sortingLayerName = "Heatmap";
                tmRenderer.sortingOrder = 9999; // por ejemplo

                // aniade el tilemap al dictionary
                _tilemaps.Add(heatMapsToVisualize[i].name, tilemap);

                // Sincronizar el Grid con el HeatMap, para que usen el mismo tamanio de celda
                _grid.cellSize = new Vector3(heatMapsToVisualize[i].heatMapToRender.GetTileSize(), heatMapsToVisualize[i].heatMapToRender.GetTileSize(), 1f);

                // El origen del Grid se coloca en la esquina superior izquierda del area
                //heatmap.GetPosition es la esquina superior izquierda del area
                _grid.transform.position = new Vector3(heatMapsToVisualize[i].heatMapToRender.GetPosition().x, heatMapsToVisualize[i].heatMapToRender.GetPosition().y, 0f);



                //y ahora pintamos sus casillas
                for (int x = 0; x < heatMapsToVisualize[i].heatMapToRender.GetWidth(); ++x)
                {
                    for (int y = 0; y < heatMapsToVisualize[i].heatMapToRender.GetHeight(); ++y)
                    {
                        int heatvalue = heatMapsToVisualize[i].heatMapToRender.GetHeatMapValue(x, y);

                        // si hay valor le va ajustando el alfa y si hay mas calor mas rojo se pone
                        if (heatvalue > 0)
                        {
                           
                            Vector3Int tilePos = new Vector3Int(x, -y - 1, 0);
                            tilemap.SetTile(tilePos, _tilebase);

                            // unlockea transform de las tiles y color para k pinte 
                            tilemap.SetTileFlags(tilePos, TileFlags.None);


                            // Escalar visualmente la tile para que ocupe el tamanio de celda del heatmap
                            float tileScale = heatMapsToVisualize[i].heatMapToRender.GetTileSize();
                            tilemap.SetTransformMatrix(
                                tilePos,
                                Matrix4x4.TRS(
                                    Vector3.zero,
                                    Quaternion.identity,
                                    new Vector3(tileScale, tileScale, 1f)
                                    )
                                );
                            //float alphavalue = heatvalue * 0.01f; // TODO ajustar si hace falta
                            float alphavalue = Mathf.Clamp(heatvalue * 0.05f, 0.10f, 0.95f);

                            // a mas calor mas alpha
                            Color color = heatMapsToVisualize[i].color;
                            color.a = alphavalue;

                            tilemap.SetColor(tilePos, color);
                        }
                    }
                }
                // lo pone visible en caso de estar en el config
                tilemap.gameObject.SetActive(heatMapsToVisualize[i].visible);
            }
        
        }

    }





    // crea un tilemap nuevo y lo mete al dictionary
    public void createTileMap(MapConfig config) {
        // di hay grid
        if(_grid != null) {
            // crea un nuevo gameobject contenedor de un tilemap con su nombre con identificador y lo hace hijo de grid.
            GameObject tmGO = new GameObject($"Tilemap_{config.mapName}");
            tmGO.transform.SetParent(_grid.transform);

            // pone los componentes
            Tilemap tilemap = tmGO.AddComponent<Tilemap>();
            TilemapRenderer tmRenderer = tmGO.AddComponent<TilemapRenderer>();
            tmRenderer.material = new Material(Shader.Find("Sprites/Default"));

            // para k se pinte por encima de todo
            tmRenderer.sortingLayerName = "Heatmap";
            tmRenderer.sortingOrder = 9999; // por ejemplo

            // aniade el tilemap al dictionary
            _tilemaps.Add(config.mapName, tilemap);
        }
    }

    // updatea el tilemap seleccionado del dictionary
    public void updateTileMap(HeatMap heatmap, MapConfig config) {

        if (_grid == null) return;

        // primero tiene que tener el nombre en el dictionary
        if (_tilemaps.ContainsKey(config.mapName))
        {
            // selecciona el tilemap que kiere updatear segun el config pasado
            Tilemap tilemap = _tilemaps[config.mapName];

            // Sincronizar el Grid con el HeatMap, para que usen el mismo tamanio de celda
            _grid.cellSize = new Vector3(heatmap.GetTileSize(),heatmap.GetTileSize(),1f);

            // El origen del Grid se coloca en la esquina superior izquierda del area
            //heatmap.GetPosition es la esquina superior izquierda del area
            _grid.transform.position = new Vector3(heatmap.GetPosition().x, heatmap.GetPosition().y,0f);

            tilemap.ClearAllTiles(); // hace clear cada frame del update

            for (int i = 0; i < heatmap.GetWidth(); ++i){
                for (int j = 0; j < heatmap.GetHeight(); ++j) {
                    int heatvalue = heatmap.GetHeatMapValue(i, j);

                    // si hay valor le va ajustando el alfa y si hay mas calor mas rojo se pone
                    if (heatvalue > 0) {
                        //Debug.Log("tile x: " + i + "y: " + j);
                        //Vector3Int tilePos = new Vector3Int(i - heatmap.GetWidth()/2, -(j-heatmap.GetHeight()/2), 0); // ajuste pa k se vea bn
                        
                        // Heatmap usa origen arriba-izquierda
                        // Unity Tilemap crece hacia arriba en Y
                        Vector3Int tilePos = new Vector3Int(i, -j - 1, 0); 
                        tilemap.SetTile(tilePos, _tilebase);

                        // unlockea transform de las tiles y color para k pinte 
                        tilemap.SetTileFlags(tilePos, TileFlags.None);


                        // Escalar visualmente la tile para que ocupe el tamanio de celda del heatmap
                        float tileScale = heatmap.GetTileSize();
                        tilemap.SetTransformMatrix(
                            tilePos,
                            Matrix4x4.TRS(
                                Vector3.zero,
                                Quaternion.identity,
                                new Vector3(tileScale, tileScale, 1f)
                                )
                            );
                        //float alphavalue = heatvalue * 0.01f; // TODO ajustar si hace falta
                        float alphavalue = Mathf.Clamp(heatvalue * 0.05f, 0.10f, 0.95f);

                        // a mas calor mas alpha
                        Color color = config.color;
                        color.a = alphavalue;

                        tilemap.SetColor(tilePos, color);
                    }
                }
            }
            // lo pone visible en caso de estar en el config
            tilemap.gameObject.SetActive(config.visible);
        }
    }

    private void ClearTilemaps()
    {
        // Primero, limpiar las referencias del diccionario
        _tilemaps.Clear();

        // Eliminar todos los hijos del Grid
        for (int i = _grid.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = _grid.transform.GetChild(i);
            if (Application.isPlaying)
                GameObject.Destroy(child.gameObject);
            else
                GameObject.DestroyImmediate(child.gameObject); // Importante para editor
        }

        // Limpiar el Grid por si tiene referencias residuales
        if (_grid != null && _grid.gameObject != null)
        {
            // Resetear posición del Grid si es necesario
            _grid.transform.position = Vector3.zero;
        }
    }
}
