using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[Serializable]

//ESTRUCTURA INTERMEDIA xq no se puede pasar directamente a json desde heatmap
public struct HeatMapData
{
    public int width;
    public int height;
    public float tileSize;
    public Vector2 position;
    public List<int> heatValues;
}

public static class HeatMapSerializer
{
    public static string savingPath;
    public static string calculateSavingPath(string sesion)
    {
        string basePath = Path.GetFullPath(Path.Combine(Application.dataPath, "heatMaps"));

        string sessionPath = Path.Combine(basePath, $"session_{sesion}");

        Directory.CreateDirectory(sessionPath);
        savingPath = sessionPath;

        return sessionPath;
    }

    public static string ToJson(HeatMap heatMap)
    {
        HeatMapData data = new HeatMapData
        {
            width = heatMap.GetWidth(),
            height = heatMap.GetHeight(),
            tileSize = heatMap.GetTileSize(),
            position = heatMap.GetPosition(),
            heatValues = new List<int>()
        };

        for (int y = 0; y < data.height; y++)
        {
            for (int x = 0; x < data.width; x++)
            {
                data.heatValues.Add(heatMap.GetHeatMapValue(x, y));
            }
        }

        return JsonUtility.ToJson(data, true);
    }

  
    public static HeatMap FromJson(string json)
    {
        HeatMapData data = JsonUtility.FromJson<HeatMapData>(json);

        HeatMap heatMap = new HeatMap(data.width, data.height, data.tileSize, data.position);


        for (int y = 0; y < data.height; y++)
        {
            for (int x = 0; x < data.width; x++)
            {
                heatMap._heatMap[x, y] = data.heatValues[y*data.width + x];
            }
        }

        return heatMap;
    }


    public static void SaveToFile(HeatMap heatMap, string fileName)
    {
        Debug.Log(fileName);
        string json = ToJson(heatMap);
        string path = Path.Combine(savingPath, fileName + ".json");
        File.WriteAllText(path, json);

        Debug.Log($"Guardado en: {path}");
    }

    
    public static HeatMap LoadFromFile(string path)
    { 

        if (!File.Exists(path))
        {
            Debug.LogError("Archivo no encontrado: " + path);
            return null;
        }

        string json = File.ReadAllText(path);
        return FromJson(json);
    }
}
