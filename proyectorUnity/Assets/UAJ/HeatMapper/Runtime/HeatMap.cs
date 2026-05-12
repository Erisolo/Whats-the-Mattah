using UnityEngine;
using UnityEngine.UIElements;

// IMPORTANTE TENER EN CUENTA K LO ESTOY HACIENDO EL ORIGEN EN TOPLEFY Y LA X VA HACIA LA DERECHA Y LA Y VA HACIA ABAJO GRACIAS
public class HeatMap {
    private int _width;        // anchura en casillas del heatmap
    private int _height;       // altura en casillas del hatmap
    private float _tileSize;   // tmanio de cada tile del heatmap
    private Vector2 _position; // posicion del heatmap (desde alguna esquina)

    // valores de cada casilla, supongo k a mas valor mas calor?
    public int[,] _heatMap;

    public int GetWidth() { return _width; }
    public int GetHeight() { return _height; }
    public float GetTileSize() { return _tileSize; }
    public Vector2 GetPosition() { return _position; }

    public HeatMap(int w, int h, float tileSize, Vector2 pos) {
        _width = w; _height = h; _tileSize = tileSize; _position = pos;

        _heatMap = new int[_width, _height];
    }

    // aniadimos puntos de calor al mapa en la posicion especificada.
    // x : [0, _width-1] // y : [0, _height-1]

    public int GetHeatMapValue(int x, int y)
    {
        if(!IsInside(x,y)) return 0;
        return _heatMap[x, y];
    }
    public void AddHeatMapValue(Vector2Int tile, int value = 1) {
        if (!IsInside(tile.x, tile.y)) return;

        _heatMap[tile.x, tile.y] += value;
    }

    public Vector2Int WorldToTile(Vector2 worldPoint) {

        int x = Mathf.FloorToInt((worldPoint.x - _position.x) / _tileSize);
        int y = Mathf.FloorToInt((_position.y - worldPoint.y) / _tileSize);
       
        return new Vector2Int(x, y);
    }

    public Vector3 TileToWorldCenter(int x, int y, float z = 0f)
    {

        float worldX = _position.x + x * _tileSize + _tileSize / 2f;
        float worldY = _position.y - y * _tileSize + _tileSize / 2f;

        return new Vector3(worldX, worldY, z);
    }

    public bool IsInside(int x, int y)
    {
        return x >= 0 && x < _width && y >= 0 && y < _height;
    }
}
