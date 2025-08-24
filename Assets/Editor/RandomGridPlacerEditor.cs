using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RandomGridPlacerEditor : EditorWindow
{
    private List<GameObject> prefabsToPlace = new List<GameObject>();
    private int rows = 5;
    private int columns = 5;
    private float xSpacing = 10f;
    private float zSpacing = 10f;
    private bool autoAdjustSpacing = true;
    private bool isChaotic = false;
    private float chaosMagnitude = 2f;

    [MenuItem("Tools/Random Grid Placer")]
    public static void ShowWindow()
    {
        GetWindow<RandomGridPlacerEditor>("Random Grid Placer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Randomly Place Prefabs in a Grid", EditorStyles.boldLabel);

        // UI for adding/removing prefabs
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

        rows = EditorGUILayout.IntField("Rows", rows);
        columns = EditorGUILayout.IntField("Columns", columns);

        EditorGUILayout.Space(10);

        autoAdjustSpacing = EditorGUILayout.Toggle("Auto-Adjust Spacing", autoAdjustSpacing);
        if (!autoAdjustSpacing)
        {
            xSpacing = EditorGUILayout.FloatField("X Spacing", xSpacing);
            zSpacing = EditorGUILayout.FloatField("Z Spacing", zSpacing);
        }

        isChaotic = EditorGUILayout.Toggle("Chaotic Placement", isChaotic);
        if (isChaotic)
        {
            chaosMagnitude = EditorGUILayout.FloatField("Chaos Magnitude", chaosMagnitude);
        }

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

        GameObject parentObject = new GameObject("Randomly Placed Grid");
        Vector3 startPosition = parentObject.transform.position;

        float finalXSpacing = xSpacing;
        float finalZSpacing = zSpacing;

        // Auto-adjust spacing if enabled
        if (autoAdjustSpacing)
        {
            Vector3 maxBounds = GetMaxPrefabBounds();
            finalXSpacing = maxBounds.x + 2f;
            finalZSpacing = maxBounds.z + 2f;
        }

        // Loop through the grid dimensions
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                // Select a random prefab from the list
                GameObject prefabToSpawn = prefabsToPlace[Random.Range(0, prefabsToPlace.Count)];
                if (prefabToSpawn == null) continue;

                // Calculate the base position
                Vector3 newPosition = startPosition + new Vector3(j * finalXSpacing, 0, i * finalZSpacing);

                // Add chaos if enabled
                if (isChaotic)
                {
                    float randomXOffset = Random.Range(-chaosMagnitude, chaosMagnitude);
                    float randomZOffset = Random.Range(-chaosMagnitude, chaosMagnitude);
                    newPosition += new Vector3(randomXOffset, 0, randomZOffset);
                }

                // Instantiate the prefab at the new position
                GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn, parentObject.transform);
                newObject.transform.position = newPosition;
            }
        }

        Debug.Log($"Successfully placed {rows * columns} objects in a grid.");
    }

    private Vector3 GetMaxPrefabBounds()
    {
        Vector3 maxBounds = Vector3.zero;
        foreach (var prefab in prefabsToPlace)
        {
            if (prefab != null)
            {
                Bounds prefabBounds = GetPrefabBounds(prefab);
                if (prefabBounds.size.x > maxBounds.x) maxBounds.x = prefabBounds.size.x;
                if (prefabBounds.size.y > maxBounds.y) maxBounds.y = prefabBounds.size.y;
                if (prefabBounds.size.z > maxBounds.z) maxBounds.z = prefabBounds.size.z;
            }
        }
        return maxBounds;
    }

    private Bounds GetPrefabBounds(GameObject prefab)
    {
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }
        return new Bounds(Vector3.zero, Vector3.one);
    }
}