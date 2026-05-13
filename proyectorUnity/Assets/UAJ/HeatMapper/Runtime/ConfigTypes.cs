using System;
using System.Collections.Generic;
using UnityEngine;

// Tipos de eventos que puede registrar HeatMapper
public enum TrackEventType
{
    Transform,  // Registra periodicamente la posicion de un Transform
    InputKey,   // Registra la posicion del Transform cuando se pulsa una tecla concreta
    InputMouse, // Registra la posicion del raton en el mundo cuando se hace click
    Custom
}

// Configuracion de un heatmap: evento a trackear, objeto asociado, configuracion visual
[Serializable]
public class MapConfig
{
    // Indica si este heatmap debe visualizarse o no
    public bool visible = true;

    // Nombre para identificar el heatmap
    // Se usa como clave para guardar y acceder a sus datos
    public string mapName = "New Heatmap";

    // Transform que se va a registrar
    public Transform tr;
    
    // Tipo de evento que se va a registrar en este heatmap
    public TrackEventType eventType = TrackEventType.Transform;

    // Tecla que se debe pulsar para registrar el evento (eventype = inputkey)
    public KeyCode inputKey = KeyCode.Space;

    // Color con el que se dibujara este heatmap
    public Color color = new Color(1.0f, 0f, 0f, 0.35f);

    // Tiempo entre muestras consecutivas
    // Para tracking periodico de posicion
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
