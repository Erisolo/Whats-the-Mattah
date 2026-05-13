using UnityEngine;


// IMPORTANTE TENER EN CUENTA K LO ESTOY HACIENDO EL ORIGEN EN TOPLEFY Y LA X VA HACIA LA DERECHA Y LA Y VA HACIA ABAJO GRACIAS

// Representa los datos de un heatmap como una matriz de enteros
// Cada posicion de la matriz equivale a una celda del area trackeada
// El valor de cada celda indica cuantas veces se ha registrado une vento en esa zona
public class HeatMap {
    private int _width;        // Anchura en casillas del heatmap
    private int _height;       // Altura en casillas del heatmap
    private float _tileSize;   // Tamanio de cada tile del heatmap
    private Vector2 _position; // Posicion del heatmap (desde alguna esquina)

    public int[,] _heatMap;    // Matriz que almacena la intensidad de cada casilla

    // Getters
    public int GetWidth() { return _width; }
    public int GetHeight() { return _height; }
    public float GetTileSize() { return _tileSize; }
    public Vector2 GetPosition() { return _position; }

    // Crea un heatmap vacio con una anchura, altura, tamanio de celda y posicion inicial
    public HeatMap(int w, int h, float tileSize, Vector2 pos) {
        _width = w; _height = h; _tileSize = tileSize; _position = pos;

        _heatMap = new int[_width, _height];
    }

    // aniadimos puntos de calor al mapa en la posicion especificada.
    // x : [0, _width-1] // y : [0, _height-1]

    // Devuelve el valor de calor de una casilla
    // Si la celda esta fuera del mapa, devuelve 0 para evitar errores
    public int GetHeatMapValue(int x, int y)
    {
        if(!IsInside(x,y)) return 0;
        return _heatMap[x, y];
    }

    // Aniade calor a una casilla concreta
    // Por defecto suma 1, pero se puede pasar otro valor
    public void AddHeatMapValue(Vector2Int tile, int value = 1) {
        if (!IsInside(tile.x, tile.y)) return;

        _heatMap[tile.x, tile.y] += value;
    }

    // Convierte una posicion del mundo a una celda del heatmap
    // El origen del mapa esta en la esquina superior izquierda:
    // - X crece hacia la derecha
    // - Y crece hacia abajo
    public Vector2Int WorldToTile(Vector2 worldPoint) {

        int x = Mathf.FloorToInt((worldPoint.x - _position.x) / _tileSize);
        int y = Mathf.FloorToInt((_position.y - worldPoint.y) / _tileSize);
       
        return new Vector2Int(x, y);
    }

    // Convierte una celda del heatmap a la posicion central de esa celda en el mundo
    // Sirve para dibujar el tile en la escena
    public Vector3 TileToWorldCenter(int x, int y, float z = 0f)
    {

        float worldX = _position.x + x * _tileSize + _tileSize / 2f;

        // Como Y crece hacia abajo desde la esquina superior izquierda,
        // se resta el desplazamiento vertical
        float worldY = _position.y - y * _tileSize - _tileSize / 2f;

        return new Vector3(worldX, worldY, z);
    }

    // Comprueba si una casilla esta dentro de los limites del tilemap
    // Evita acceder a posiciones fuera de la matriz
    public bool IsInside(int x, int y)
    {
        return x >= 0 && x < _width && y >= 0 && y < _height;
    }
}
