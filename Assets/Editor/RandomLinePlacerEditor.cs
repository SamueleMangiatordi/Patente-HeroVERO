using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RandomLinePlacerEditor : EditorWindow
{
    private List<GameObject> prefabsToPlace = new List<GameObject>();
    private int numberOfObjects = 10;
    private float distanceBetween = 10f;
    private Vector3 direction = Vector3.forward;

    [MenuItem("Tools/Random Line Placer")]
    public static void ShowWindow()
    {
        GetWindow<RandomLinePlacerEditor>("Random Line Placer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Randomly Place Prefabs in a Line", EditorStyles.boldLabel);

        // UI for the list of prefabs
        EditorGUILayout.LabelField("Prefabs to Choose From:", EditorStyles.label);

        // Loop through the list to display each field and allow reordering
        for (int i = 0; i < prefabsToPlace.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            prefabsToPlace[i] = (GameObject)EditorGUILayout.ObjectField($"Prefab {i}", prefabsToPlace[i], typeof(GameObject), false);
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                prefabsToPlace.RemoveAt(i);
                break; // Exit the loop to prevent errors from modifying the list
            }
            EditorGUILayout.EndHorizontal();
        }

        // Button to add a new prefab field
        if (GUILayout.Button("Add Prefab"))
        {
            prefabsToPlace.Add(null);
        }

        EditorGUILayout.Space(10);

        numberOfObjects = EditorGUILayout.IntField("Number of Objects", numberOfObjects);
        distanceBetween = EditorGUILayout.FloatField("Distance Between", distanceBetween);
        direction = EditorGUILayout.Vector3Field("Direction", direction);

        if (GUILayout.Button("Place Objects"))
        {
            PlaceObjectsRandomly();
        }
    }

    private void PlaceObjectsRandomly()
    {
        if (prefabsToPlace.Count == 0)
        {
            Debug.LogError("No prefabs assigned. Please add prefabs to the list.");
            return;
        }

        GameObject parentObject = new GameObject("Randomly Placed Objects");
        Vector3 startPosition = parentObject.transform.position;

        for (int i = 0; i < numberOfObjects; i++)
        {
            // Select a random prefab from the list
            GameObject prefabToSpawn = prefabsToPlace[Random.Range(0, prefabsToPlace.Count)];

            // Calculate the position for the new object
            Vector3 newPosition = startPosition + (direction.normalized * distanceBetween * i);

            // Instantiate the prefab at the new position
            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn, parentObject.transform);
            newObject.transform.position = newPosition;
        }

        Debug.Log("Random objects placed successfully!");
    }
}