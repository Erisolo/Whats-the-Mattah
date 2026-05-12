using System;
using System.Collections.Generic;
using UnityEngine;

public enum TrackEventType
{
    Transform,
    InputKey,
    InputMouse,
    Custom
}


[Serializable]
public class MapConfig
{
    public bool visible = true;

    public string mapName = "New Heatmap";

    public Transform tr;
    
    public TrackEventType eventType = TrackEventType.Transform;

    public KeyCode inputKey = KeyCode.Space;

    public Color color = new Color(1.0f, 0f, 0f, 0.35f);

    public float sampleInterval = 0.25f;
}

//public class TransformConfig : MapConfig{}
//public class InputKeyConfig : MapConfig
//{
//    char key;
//}
//[System.Serializable]
//public class InputKeyConfig_: MapConfig
//{
//    public KeyCode key;
//}
//public class InputMouseConfig : MapConfig{}
