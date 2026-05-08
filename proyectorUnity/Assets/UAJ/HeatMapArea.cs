using UnityEngine;

public class HeatMapArea : MonoBehaviour {
    // cuantas tiles y como de grande es cada tile, ajustar despues segun lo vayamos neceistando
    [SerializeField] private Vector2Int _areaSize = new Vector2Int(50, 50); // (width, height) con ints
    [SerializeField] private float _tileSize = 1.0f; // tamanio de cada tile del heatmap

    private HeatMap _heatMap; // clase heatmap para caluclar las casillas y todas las cosas

    private void Start() {
        // crea el objeto tilemap
        _heatMap = new HeatMap(_areaSize.x, _areaSize.y, _tileSize);
    }

    // TODO creo k seria mejor calcular aqui las cosas enplan heatmap es el objeto pero aqui
    // se calcculan las cosas o no no lo se
}
