using Unity.VisualScripting;
using UnityEngine;

public class HeatMapArea : MonoBehaviour {
    [SerializeField] private Vector2Int _areaSize = new Vector2Int(50, 50); // (width/height) en caso de necesitar menos k pantalla
    [SerializeField] private float _tileSize = 1.0f; // tamanio de cada tile del heatmap

    // vamos a trakear por ejemplo el transform de algun objeto d escena, luego imagino k cambiar
    [SerializeField] private Transform _testTransform;

    private HeatMap _heatMap; 

    private void Start() {
        Camera maincam = Camera.main;
        float screenHeight = maincam.orthographicSize * 2.0f; // alto camara
        float screenWidth = screenHeight * maincam.aspect; // ancho camara
        // esquina arriba izquierda
        Vector2 bottomleft = new Vector2(
            (maincam.transform.position.x - screenWidth / 2.0f),
            (maincam.transform.position.y + screenHeight / 2.0f) - _tileSize
        );
        // tamanio en base a las tilesizes se ajusta a pantalla
        int w = (int)(screenWidth / _tileSize);
        int h = (int)(screenHeight / _tileSize);

        // crea el objeto tilemap
        _heatMap = new HeatMap(w, h, _tileSize, bottomleft);
    }

    private void Update() {
        _heatMap.addHeatValue(_heatMap.worldToTile(_testTransform.position));
    }

    // para ver cuadricula y k se ponga el calor en rojo segun el alpha
    private void OnDrawGizmos() {
        drawTileMap();
        drawHeatMap();
    }

    private void drawTileMap() {

        for(int i = 0; i < _heatMap.getWidth(); ++i) {
            for (int j = 0; j < _heatMap.getHeight(); ++j) {
                Vector2 tilePos = new Vector2(
                    _heatMap.getPosition().x + i * _tileSize,
                    _heatMap.getPosition().y - j * _tileSize
                );

                // el origen lo pone rosa para saber cual es
                if (tilePos == _heatMap.getPosition()) Gizmos.color = Color.magenta;
                else Gizmos.color = Color.white;

                Gizmos.DrawWireCube(
                    new Vector3(tilePos.x, tilePos.y, 0) + Vector3.one * (_tileSize / 2.0f),
                    Vector3.one * _tileSize
                );
            }
        }
    }

    private void drawHeatMap() {

    }
}
