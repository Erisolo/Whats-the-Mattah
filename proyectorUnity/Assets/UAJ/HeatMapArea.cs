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
        Vector2 topleft = new Vector2(
            maincam.transform.position.x - screenWidth / 2.0f,
            maincam.transform.position.y - screenHeight / 2.0f);
        // tamanio en base a las tilesizes se ajusta a pantalla
        int w = (int)(screenWidth / _tileSize);
        int h = (int)(screenHeight / _tileSize);

        // crea el objeto tilemap
        _heatMap = new HeatMap(w, h, _tileSize, topleft);
    }
}
