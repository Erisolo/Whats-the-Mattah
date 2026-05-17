using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Combine
{
    Union,   
    Intersect,
    Difference
}

[Serializable]
[ExecuteAlways]
public class HeatMapCombineType 
{
    public bool visible = true;
    public Color color = new Color(1.0f, 0f, 0f, 0.35f);
    public Combine combineType;

    public string name;
    [Header("Maps to combine")]
    public List<TextAsset> JSONHeatMaps = new List<TextAsset>();

    private List<HeatMap> deserialized_heatmaps = new List<HeatMap>();

    [HideInInspector]
    public HeatMap heatMapToRender;

    
    public void RefreshHeatmaps()
    {
        deserialized_heatmaps.Clear();

        if (JSONHeatMaps == null)
            return;

        foreach (var json in JSONHeatMaps)
        {
            if (json == null) continue;

            HeatMap hm = HeatMapSerializer.FromJson(json.text); 
            if (hm != null)
                deserialized_heatmaps.Add(hm);
        }
        Debug.Log(deserialized_heatmaps.Count); 

    }

    public void logicOperation()
    {
        heatMapToRender = deserialized_heatmaps[0];

    }

    public HeatMap Union()
    {
        heatMapToRender = deserialized_heatmaps[0];
        return heatMapToRender;
    }

    public HeatMap Intersect() 
    {
        heatMapToRender = deserialized_heatmaps[0];
        return heatMapToRender; 
    }
    public HeatMap Difference() 
    {
        heatMapToRender = deserialized_heatmaps[0];
        return heatMapToRender; 
    }
}
