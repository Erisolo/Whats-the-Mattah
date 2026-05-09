using UnityEngine;
using UnityEngine.UIElements;

// IMPORTANTE TENER EN CUENTA K LO ESTOY HACIENDO EL ORIGEN EN TOPLEFY Y LA X VA HACIA LA DERECHA Y LA Y VA HACIA ABAJO GRACIAS
public class HeatMap {
    private int _width;        // anchura en casillas del heatmap
    private int _height;       // altura en casillas del hatmap
    private float _tileSize;   // tmanio de cada tile del heatmap
    private Vector2 _position; // posicion del heatmap (desde alguna esquina)

    // valores de cada casilla, supongo k a mas valor mas calor?
    public int[,] heatMap;

    public int getWidth() { return _width; }
    public int getHeight() { return _height; }
    public float getTileSize() { return _tileSize; }
    public Vector2 getPosition() { return _position; }
    public int getHeatMapValue(int x, int y) { return heatMap[x,y]; }

    public HeatMap(int w, int h, float tileSize, Vector2 pos) {
        _width = w; _height = h; _tileSize = tileSize; _position = pos;

        heatMap = new int[_width, _height];
    }

    // aniadimos puntos de calor al mapa en la posicion especificada.
    // x : [0, _width-1] // y : [0, _height-1]
    public void addHeatValue(Vector2Int tile, int value = 1) {
        heatMap[tile.x, tile.y] += value;
    }

    public Vector2Int worldToTile(Vector2 worldpoint) {
        return new Vector2Int(
            (int)((worldpoint.x - _position.x) / _tileSize),
            (int)((_position.y - worldpoint.y) / _tileSize)
        );
    }
}
