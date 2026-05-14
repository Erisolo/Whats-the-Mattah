using UnityEngine;

// IMPORTANTE TENER EN CUENTA K LO ESTOY HACIENDO EL ORIGEN EN TOPLEFY Y LA X VA HACIA LA DERECHA Y LA Y VA HACIA ABAJO GRACIAS

public class HeatMapArea : MonoBehaviour {
    [SerializeField] private float _tileSize = 1.0f; // tamanio de cada tile del heatmap

    #region TODO BORRAR LUEGO TESTEO
    // vamos a trakear por ejemplo el transform de algun objeto d escena, luego imagino k cambiar
    [SerializeField] private Transform _testTransform;
    #endregion

    private HeatMap _heatMap; 

    private void Start() {
        Camera maincam = Camera.main;
        float screenHeight = maincam.orthographicSize * 2.0f; // alto camara
        float screenWidth = screenHeight * maincam.aspect; // ancho camara
        // esquina arriba izquierda
        Vector2 topleft = new Vector2(
            (maincam.transform.position.x - screenWidth / 2.0f),
            (maincam.transform.position.y + screenHeight / 2.0f) - _tileSize
        );
        // tamanio en base a las tilesizes se ajusta a pantalla
        int w = (int)(screenWidth / _tileSize);
        int h = (int)(screenHeight / _tileSize);

        // crea el objeto tilemap
        _heatMap = new HeatMap(w, h, _tileSize, topleft);
    }

    private void Update() {
        if(_testTransform != null) {
            _heatMap.AddHeatMapValue(_heatMap.WorldToTile(_testTransform.position));
        }
    }

    // para ver cuadricula y k se ponga el calor en rojo segun el alpha
    private void OnDrawGizmos() {
        DrawTileMap();
        DrawHeatMap();
    }

    private void DrawTileMap() {
        for(int i = 0; i < _heatMap.GetWidth(); ++i) {
            for (int j = 0; j < _heatMap.GetHeight(); ++j) {
                Vector2 tilePos = new Vector2(
                    _heatMap.GetPosition().x + i * _tileSize,
                    _heatMap.GetPosition().y - j * _tileSize
                );

                // el origen lo pone rosa para saber cual es
                if (tilePos == _heatMap.GetPosition()) Gizmos.color = Color.magenta;
                else Gizmos.color = Color.white;

                Gizmos.DrawWireCube(
                    new Vector3(tilePos.x, tilePos.y, 0) + Vector3.one * (_tileSize / 2.0f),
                    Vector3.one * _tileSize
                );
            }
        }
    }

    private void DrawHeatMap() {
        for (int i = 0; i < _heatMap.GetWidth(); ++i) {
            for (int j = 0; j < _heatMap.GetHeight(); ++j) {
                int heatvalue = _heatMap.GetHeatMapValue(i, j);

                // si hay valor le va ajustando el alfa y si hay mas calor mas rojo se pone
                if (heatvalue > 0) {
                    float alphavalue = heatvalue * 0.01f; // TODO ajustar si hace falta

                    // cada vez mas alpha
                    Gizmos.color = new Color(1.0f, 0.0f, 0.0f, alphavalue);

                    // copiado del metodo d antes.
                    Vector2 tilePos = new Vector2(
                        _heatMap.GetPosition().x + i * _tileSize,
                        _heatMap.GetPosition().y - j * _tileSize
                    );

                    Gizmos.DrawCube(
                        new Vector3(tilePos.x, tilePos.y, 0) + Vector3.one * (_tileSize / 2.0f),
                        Vector3.one * _tileSize
                    );
                }
            }
        }
    }
}
