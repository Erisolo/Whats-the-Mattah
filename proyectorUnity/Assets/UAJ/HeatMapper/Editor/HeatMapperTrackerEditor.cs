using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// Editor personalizado para HeatMapperTracker
// Permite al usuario editar el área del HeatMapperTracker visualmente desde 
// la Scene View mediante handles. Actualiza automaticamente areaSize.
// Complementa la configuracion manual de areaSize, facilitando ajustar el area al tamanio real del nivel.

[CustomEditor(typeof(HeatMapperTracker))]
public class HeatMapperTrackerEditor : Editor
{
    // Dibuja y gestiona los controles interactivos del area en la Scene View
    // Si esta activado, dibuja el rectangulo del area y se muestran handles
    // en los lados izquierdo, derecho, superior e inferior
    private void OnSceneGUI()
    {
        HeatMapperTracker tracker = (HeatMapperTracker)target;

        if (!tracker.showAreaEditor)
        {
            return;
        }

        Transform tracketTransform = tracker.transform;

        // Centro del area, basado en la posicion del objeto HeatMapperTracker
        Vector3 center = tracketTransform.position;

        // Mitad del tamanio del area para calcular los bordes
        float halfWidth = tracker.areaSize.x / 2f;
        float halfHeight = tracker.areaSize.y / 2f;

        // Posiciones de los handles en cada lado del rectangulo
        Vector3 left = center + Vector3.left * halfWidth;
        Vector3 right = center + Vector3.right * halfWidth;
        Vector3 top = center + Vector3.up * halfHeight;
        Vector3 bottom = center + Vector3.down * halfHeight;

        Handles.color = Color.yellow;

        // Dibujar el contorno del area actual
        DrawAreaRectangle(center, tracker.areaSize);

        // Detecta si el usuario mueve algun handle
        EditorGUI.BeginChangeCheck();

        Vector3 newLeft = Handles.FreeMoveHandle(
            left,
            Quaternion.identity,
            0.2f,
            Vector3.zero,
            Handles.SphereHandleCap
        );

        Vector3 newRight = Handles.FreeMoveHandle(
            right,
            Quaternion.identity,
            0.2f,
            Vector3.zero,
            Handles.SphereHandleCap
        );

        Vector3 newTop = Handles.FreeMoveHandle(
            top,
            Quaternion.identity,
            0.2f,
            Vector3.zero,
            Handles.SphereHandleCap
        );

        Vector3 newBottom = Handles.FreeMoveHandle(
            bottom,
            Quaternion.identity,
            0.2f,
            Vector3.zero,
            Handles.SphereHandleCap
        );

        // Si algun handle se ha movido, se recalcula el area
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(tracker, "Resize HeatMapper Area");

            // Mantiene la edicion en el plano 2D del tracker
            newLeft.z = center.z;
            newRight.z = center.z;
            newTop.z = center.z;
            newBottom.z = center.z;

            // Calcula el nuevo ancho y alto a partir de la distancia entre handles
            float newWidth = Mathf.Abs(newRight.x - newLeft.x);
            float newHeight = Mathf.Abs(newTop.y - newBottom.y);

            // Evitar que el area tenga tamanio cero o negativo 
            tracker.areaSize = new Vector2(
                Mathf.Max(0.1f, newWidth),
                Mathf.Max(0.1f, newHeight)
            );

            // Marca el objeto como modificado y repinta la Scene View
            EditorUtility.SetDirty(tracker);
            SceneView.RepaintAll();
        }
    }

    // Dibuja el rectangulo que representa el area de tracking
    private void DrawAreaRectangle(Vector3 center, Vector2 size)
    {
        // El rectangulo se dibuja centrado en la posicion del HeatMapperTracker
        // y utiliza areaSize como ancho y alto
        float halfWidth = size.x / 2f;
        float halfHeight = size.y / 2f;

        Vector3 topLeft = center + new Vector3(-halfWidth, halfHeight, 0f);
        Vector3 topRight = center + new Vector3(halfWidth, halfHeight, 0f);
        Vector3 bottomRight = center + new Vector3(halfWidth, -halfHeight, 0f);
        Vector3 bottomLeft = center + new Vector3(-halfWidth, -halfHeight, 0f);

        Handles.DrawLine(topLeft, topRight);
        Handles.DrawLine(topRight, bottomRight);
        Handles.DrawLine(bottomRight, bottomLeft);
        Handles.DrawLine(bottomLeft, topLeft);
    }
    public override void OnInspectorGUI()
    {
        HeatMapperTracker tracker = (HeatMapperTracker)target;

        // Reutiliza el mismo drawer que la ventana de la tool
        // Asi el inpector y la ventana HeatMapper muestran los mismos campos
        HeatMapConfigDrawer.DrawTrackerSettings(tracker);

        EditorGUILayout.Space();

        // Dibuja la configuracion de los heatmaps con campos especificos
        // segun el tipo de evento seleccionado
        HeatMapConfigDrawer.DrawHeatMapConfigs(tracker);
    }
}
