using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AreaPlacerEditor : EditorWindow
{
    private static AreaPlacerEditor window;
    private List<GameObject> prefabsToPlace = new List<GameObject>();
    private int numberOfObjects = 50;
    private bool isChaotic = true;
    private float chaosMagnitude = 5f;

    [MenuItem("Tools/Area Placer")]
    public static void ShowWindow()
    {
        window = GetWindow<AreaPlacerEditor>("Area Placer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Place Prefabs in a Drawn Area", EditorStyles.boldLabel);

        // Prefabs list UI
        EditorGUILayout.LabelField("Prefabs to Choose From:");
        for (int i = 0; i < prefabsToPlace.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            prefabsToPlace[i] = (GameObject)EditorGUILayout.ObjectField($"Prefab {i + 1}", prefabsToPlace[i], typeof(GameObject), false);
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                prefabsToPlace.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("Add Prefab"))
        {
            prefabsToPlace.Add(null);
        }

        EditorGUILayout.Space(10);

        numberOfObjects = EditorGUILayout.IntField("Number of Objects", numberOfObjects);
        isChaotic = EditorGUILayout.Toggle("Chaotic Placement", isChaotic);

        if (isChaotic)
        {
            chaosMagnitude = EditorGUILayout.FloatField("Chaos Magnitude", chaosMagnitude);
        }

        if (GUILayout.Button("Start Placing"))
        {
            StartPlacementTool();
        }

        EditorGUILayout.Space(20);
        GUILayout.Label("Instructions:", EditorStyles.boldLabel);
        GUILayout.Label("1. Click 'Start Placing'.", EditorStyles.label);
        GUILayout.Label("2. In the Scene view, click and drag to define the area.", EditorStyles.label);
        GUILayout.Label("3. Release the mouse button to place the objects.", EditorStyles.label);
    }

    private void StartPlacementTool()
    {
        if (prefabsToPlace.Count == 0 || prefabsToPlace[0] == null)
        {
            Debug.LogError("No prefabs assigned. Please add prefabs to the list.");
            return;
        }

        // Create a temporary object to hold the scene view tool script
        GameObject toolObject = new GameObject("Area Placement Tool");
        toolObject.hideFlags = HideFlags.HideInHierarchy;
        // Correctly reference the AreaPlacerTool component
        AreaPlacerTool tool = toolObject.AddComponent<AreaPlacerTool>();

        tool.prefabsToPlace = prefabsToPlace;
        tool.numberOfObjects = numberOfObjects;
        tool.isChaotic = isChaotic;
        tool.chaosMagnitude = chaosMagnitude;

        Debug.Log("Area Placement Tool Activated. Click and drag in the Scene view.");
    }
}