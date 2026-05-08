using UnityEngine;

public class HeatMapArea : MonoBehaviour {
    // cuantas tiles y como de grande es cada tile, ajustar despues segun lo vayamos neceistando
    [SerializeField] private Vector2 _areaSize = new Vector2(50.0f, 50.0f); // (width, height)
    [SerializeField] private float _tileSize = 1.0f; // tamanio de cada tile del heatmap
}
