using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatMapVisual : MonoBehaviour
{
    private Grid grid;
    private Mesh mesh;

    private void Awake()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh; 
    }
    public void SetGrid(Grid grid)
    {
        this.grid = grid;
    }

    private void UpdateHeatMapVisual()
    {

    }
}
