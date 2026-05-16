using System;
using System.Collections.Generic;
using UnityEngine;

// Tipos de eventos que puede registrar HeatMapper
public enum TrackEventType
{
    Transform,      // Registra periodicamente la posicion de un Transform
    InputKey,       // Registra la posicion de un Transform cuando se pulsa una tecla concreta
    InputMouse,     // Registra la posicion del raton en el mundo cuando se hace click
    Custom,         // Evento lanzado manualmente desde codigo
    ListTransform   // Registra periodicamente la posicion de varios Transforms
}

// Botones de raton disponibles para eventos InputMouse
public enum MouseButton
{
    Left = 0,
    Right = 1,
    Middle = 2
}

[Serializable]
public class MapConfig
{
    // Indica si este heatmap debe visualizarse o no
    public bool visible = true;

    // Nombre para identificar el heatmap
    // Se usa como clave para guardar y acceder a sus datos
    public string mapName = "New Heatmap";

    // Tipo de evento que se va a registrar en este heatmap
    public TrackEventType eventType = TrackEventType.Transform;

    // Color con el que se dibujara este heatmap
    public Color color = new Color(1.0f, 0f, 0f, 0.35f);

    [Header("Transform")]
    // Transform principal que se va a registrar
    // Se usa en Transform e Inputkey
    public Transform tr;
    
    [Header("Input Key")]
    // Tecla que se debe pulsar para registrar el evento
    public KeyCode inputKey = KeyCode.Space;

    [Header("Input Mouse")]
    // Boton de raton que debe pulsarse para registrar el evento
    public MouseButton mouseButton = MouseButton.Left;

    [Header("List Transform")]
    // Lista de objetos que se van a registrar
    public List<Transform> transformList = new List<Transform>();

    [Header("Sampling")]
    // Tiempo entre muestras consecutivas
    // Para tracking periodico de posicion
    public float sampleInterval = 0.25f;
}
