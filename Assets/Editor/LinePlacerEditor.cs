using UnityEngine;
using UnityEditor;

public class LinePlacerEditor : EditorWindow
{
    private GameObject objectToPlace;
    private int numberOfObjects = 10;
    private float distanceBetween = 2f;
    private Vector3 direction = Vector3.forward;

    [MenuItem("Tools/Line Placer")]
    public static void ShowWindow()
    {
        GetWindow<LinePlacerEditor>("Line Placer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Place Objects in a Line", EditorStyles.boldLabel);

        objectToPlace = (GameObject)EditorGUILayout.ObjectField("Object to Place", objectToPlace, typeof(GameObject), true);
        numberOfObjects = EditorGUILayout.IntField("Number of Objects", numberOfObjects);
        distanceBetween = EditorGUILayout.FloatField("Distance Between", distanceBetween);
        direction = EditorGUILayout.Vector3Field("Direction", direction);

        if (GUILayout.Button("Place Objects"))
        {
            PlaceObjects();
        }
    }

    private void PlaceObjects()
    {
        if (objectToPlace == null)
        {
            Debug.LogError("Object to Place is not assigned.");
            return;
        }

        GameObject parentObject = new GameObject(objectToPlace.name + "s");
        Vector3 startPosition = parentObject.transform.position;

        for (int i = 0; i < numberOfObjects; i++)
        {
            Vector3 newPosition = startPosition + (direction.normalized * distanceBetween * i);
            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(objectToPlace, parentObject.transform);
            newObject.transform.position = newPosition;
        }

        Debug.Log("Objects placed successfully!");
    }
}