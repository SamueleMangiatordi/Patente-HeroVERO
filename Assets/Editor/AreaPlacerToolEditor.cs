using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(AreaPlacerTool))]
public class AreaPlacerToolEditor : Editor
{
    private AreaPlacerTool placerTool;

    private bool isDrawing = false;
    private Vector3 startPoint;
    private Vector3 endPoint;

    private void OnEnable()
    {
        placerTool = (AreaPlacerTool)target;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        // Don't draw if the editor window isn't open or the user isn't holding the tool
        if (!isDrawing)
        {
            return;
        }

        Event currentEvent = Event.current;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        HandleUtility.AddDefaultControl(controlID);

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 mousePosition = ray.GetPoint(distance);

            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
            {
                GUIUtility.hotControl = controlID;
                startPoint = mousePosition;
                currentEvent.Use();
            }
            else if (currentEvent.type == EventType.MouseDrag && currentEvent.button == 0 && GUIUtility.hotControl == controlID)
            {
                endPoint = mousePosition;
                currentEvent.Use();
            }
            else if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0 && GUIUtility.hotControl == controlID)
            {
                GUIUtility.hotControl = 0;
                endPoint = mousePosition;
                isDrawing = false;
                PlaceObjectsInArea();
                DestroyImmediate(placerTool.gameObject);
                currentEvent.Use();
            }
        }

        if (isDrawing)
        {
            Handles.color = Color.yellow;
            Vector3 p1 = new Vector3(startPoint.x, 0, startPoint.z);
            Vector3 p2 = new Vector3(endPoint.x, 0, startPoint.z);
            Vector3 p3 = new Vector3(endPoint.x, 0, endPoint.z);
            Vector3 p4 = new Vector3(startPoint.x, 0, endPoint.z);
            Handles.DrawLines(new Vector3[] { p1, p2, p2, p3, p3, p4, p4, p1 });
            SceneView.RepaintAll();
        }
    }

    private void PlaceObjectsInArea()
    {
        // Place your objects here using the data from placerTool
        // Use placerTool.prefabsToPlace, placerTool.numberOfObjects, etc.
        Debug.Log("Objects placed!");
    }
}