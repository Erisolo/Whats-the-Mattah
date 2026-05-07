using System;
using System.Collections.Generic;
using UnityEngine;

public enum TrackEventType
{
    Transform,
    InputKey,
    InputMouse
}

public class MapConfig
{
    public bool visible;
    public string mapName;
    public Transform tr;
}

public class TransformConfig : MapConfig{}
public class InputKeyConfig : MapConfig
{
    char key;
}
public class InputMouseConfig : MapConfig { }