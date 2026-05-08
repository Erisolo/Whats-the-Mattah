public class HeatMap {
    // valore para inicializar el tilemap
    private int _width;
    private int _height;
    private float _tileSize;

    // aqui iran los valores de cada tile del tilemap de calor con mas fecuencia d un valor o no
    public int[,] heatMap;

    public int getWidth() { return _width; }
    public int getHeight() { return _height; }
    public float getTileSize() { return _tileSize; }

    public HeatMap(int w, int h, float tileSize) {
        _width = w; _height = h; _tileSize = tileSize;

        heatMap = new int[_width, _height];
    }

    // TODO hay k investigar como ir llenando el tilemap de cosas si desde aki o desde fuera y 
    // tb hay k saber como llenarlo y como representarlo.
}
