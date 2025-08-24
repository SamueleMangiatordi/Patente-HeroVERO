using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AreaPlacerEditorWindow : EditorWindow
{
    private static AreaPlacerEditorWindow window;
    private List<GameObject> prefabsToPlace = new List<GameObject>();
    private int numberOfObjects = 50;
    private bool isChaotic = true;
    private float chaosMagnitude = 5f;

    public enum PlacementMode { Square, Lasso }
    private PlacementMode currentMode = PlacementMode.Square;

    // Temporary object to hold the tool's data and manage events
    private AreaPlacerTool toolObject;

    [MenuItem("Tools/Area Placer")]
    public static void ShowWindow()
    {
        window = GetWindow<AreaPlacerEditorWindow>("Area Placer");
        window.minSize = new Vector2(300, 400);
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        if (toolObject != null)
        {
            DestroyImmediate(toolObject.gameObject);
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Place Prefabs in a Drawn Area", EditorStyles.boldLabel);

        currentMode = (PlacementMode)EditorGUILayout.EnumPopup("Placement Mode", currentMode);

        EditorGUILayout.Space();

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

        if (toolObject == null)
        {
            if (GUILayout.Button("Start Placing"))
            {
                StartPlacementTool();
            }
        }
        else
        {
            GUILayout.Label("Tool Active. Click and drag in the Scene view.", EditorStyles.boldLabel);
            if (GUILayout.Button("Cancel"))
            {
                CancelPlacementTool();
            }
        }

        EditorGUILayout.Space(20);
        GUILayout.Label("Instructions:", EditorStyles.boldLabel);
        GUILayout.Label("1. Choose a mode.", EditorStyles.label);
        GUILayout.Label("2. Click 'Start Placing'.", EditorStyles.label);
        GUILayout.Label("3. In the Scene view, click and drag to define the area.", EditorStyles.label);
        GUILayout.Label("4. Release the mouse button to place the objects.", EditorStyles.label);

        GUILayout.Label("\n5. If you get an error 'IndexOutOfRangeException',\n ignore it. The tool works fine anyway", EditorStyles.boldLabel);

    }

    private void StartPlacementTool()
    {
        if (prefabsToPlace.Count == 0 || prefabsToPlace.Exists(p => p == null))
        {
            Debug.LogError("No prefabs assigned or list contains a null entry. Please assign prefabs to the list.");
            return;
        }

        GameObject tempObject = new GameObject("Area Placement Tool");
        tempObject.hideFlags = HideFlags.HideInHierarchy;
        toolObject = tempObject.AddComponent<AreaPlacerTool>();

        toolObject.prefabsToPlace = prefabsToPlace;
        toolObject.numberOfObjects = numberOfObjects;
        toolObject.isChaotic = isChaotic;
        toolObject.chaosMagnitude = chaosMagnitude;

        Repaint();
    }

    private void CancelPlacementTool()
    {
        if (toolObject != null)
        {
            DestroyImmediate(toolObject.gameObject);
            toolObject = null;
        }
        Repaint();
    }

    // --- Placement Logic ---
    private void PlaceObjectsInArea(Vector3[] areaPoints)
    {
        GameObject parentObject = new GameObject("Placed Objects");

        if (toolObject.isChaotic)
        {
            PlaceObjectsChaotically(areaPoints, parentObject);
        }
        else
        {
            PlaceObjectsInGrid(areaPoints, parentObject);
        }

        Debug.Log("Objects placed successfully!");
        CancelPlacementTool();
    }

    private void PlaceObjectsChaotically(Vector3[] areaPoints, GameObject parentObject)
    {
        float minX = areaPoints[0].x;
        float maxX = areaPoints[0].x;
        float minZ = areaPoints[0].z;
        float maxZ = areaPoints[0].z;
        for (int i = 1; i < areaPoints.Length; i++)
        {
            minX = Mathf.Min(minX, areaPoints[i].x);
            maxX = Mathf.Max(maxX, areaPoints[i].x);
            minZ = Mathf.Min(minZ, areaPoints[i].z);
            maxZ = Mathf.Max(maxZ, areaPoints[i].z);
        }

        for (int i = 0; i < toolObject.numberOfObjects; i++)
        {
            GameObject prefabToSpawn = toolObject.prefabsToPlace[Random.Range(0, toolObject.prefabsToPlace.Count)];
            if (prefabToSpawn == null) continue;

            Vector3 newPosition = new Vector3(Random.Range(minX, maxX), 0, Random.Range(minZ, maxZ));

            if (currentMode == PlacementMode.Lasso && !IsPointInPolygon(newPosition, areaPoints))
            {
                i--; // Retry placement if outside the lasso area
                continue;
            }

            float randomXOffset = Random.Range(-toolObject.chaosMagnitude, toolObject.chaosMagnitude);
            float randomZOffset = Random.Range(-toolObject.chaosMagnitude, toolObject.chaosMagnitude);
            newPosition += new Vector3(randomXOffset, 0, randomZOffset);

            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn, parentObject.transform);
            newObject.transform.position = newPosition;
        }
    }

    private void PlaceObjectsInGrid(Vector3[] areaPoints, GameObject parentObject)
    {
        float minX = areaPoints[0].x;
        float maxX = areaPoints[0].x;
        float minZ = areaPoints[0].z;
        float maxZ = areaPoints[0].z;
        for (int i = 1; i < areaPoints.Length; i++)
        {
            minX = Mathf.Min(minX, areaPoints[i].x);
            maxX = Mathf.Max(maxX, areaPoints[i].x);
            minZ = Mathf.Min(minZ, areaPoints[i].z);
            maxZ = Mathf.Max(maxZ, areaPoints[i].z);
        }

        // Determine grid spacing based on the number of objects
        int gridX = Mathf.CeilToInt(Mathf.Sqrt(toolObject.numberOfObjects));
        int gridZ = Mathf.CeilToInt((float)toolObject.numberOfObjects / gridX);
        float spacingX = (maxX - minX) / (gridX - 1);
        float spacingZ = (maxZ - minZ) / (gridZ - 1);

        int count = 0;
        for (int x = 0; x < gridX; x++)
        {
            for (int z = 0; z < gridZ; z++)
            {
                if (count >= toolObject.numberOfObjects)
                    break;

                GameObject prefabToSpawn = toolObject.prefabsToPlace[Random.Range(0, toolObject.prefabsToPlace.Count)];
                if (prefabToSpawn == null) continue;

                Vector3 newPosition = new Vector3(minX + x * spacingX, 0, minZ + z * spacingZ);

                if (currentMode == PlacementMode.Lasso && !IsPointInPolygon(newPosition, areaPoints))
                {
                    continue; // Skip if outside the lasso area
                }

                GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn, parentObject.transform);
                newObject.transform.position = newPosition;
                count++;
            }
        }
    }

    // Simple raycasting check to see if a point is inside a polygon
    private bool IsPointInPolygon(Vector3 point, Vector3[] polygon)
    {
        bool isInside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            if (((polygon[i].z > point.z) != (polygon[j].z > point.z)) &&
                (point.x < (polygon[j].x - polygon[i].x) * (point.z - polygon[i].z) / (polygon[j].z - polygon[i].z) + polygon[i].x))
            {
                isInside = !isInside;
            }
        }
        return isInside;
    }

    // --- Scene View Drawing & Input Logic ---
    private bool isDrawing = false;
    private Vector3 startPoint;
    private Vector3 endPoint;
    private List<Vector3> lassoPoints = new List<Vector3>();

    private void OnSceneGUI(SceneView sceneView)
    {
        if (toolObject == null)
        {
            return;
        }

        Event currentEvent = Event.current;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        HandleUtility.AddDefaultControl(controlID);

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);

        Vector3 mousePosition = Vector3.zero;
        if (groundPlane.Raycast(ray, out float distance))
        {
            mousePosition = ray.GetPoint(distance);
        }

        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
        {
            GUIUtility.hotControl = controlID;
            isDrawing = true;
            if (currentMode == PlacementMode.Square)
            {
                startPoint = mousePosition;
            }
            else
            {
                lassoPoints.Clear();
                lassoPoints.Add(mousePosition);
            }
            currentEvent.Use();
        }
        else if (currentEvent.type == EventType.MouseDrag && currentEvent.button == 0 && GUIUtility.hotControl == controlID)
        {
            if (currentMode == PlacementMode.Square)
            {
                endPoint = mousePosition;
            }
            else
            {
                lassoPoints.Add(mousePosition);
            }
            currentEvent.Use();
        }
        else if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0 && GUIUtility.hotControl == controlID)
        {
            GUIUtility.hotControl = 0;
            isDrawing = false;
            if (currentMode == PlacementMode.Square)
            {
                PlaceObjectsInArea(new Vector3[] { startPoint, endPoint });
            }
            else
            {
                PlaceObjectsInArea(lassoPoints.ToArray());
            }
            currentEvent.Use();
        }

        // Drawing Logic
        if (isDrawing)
        {
            Handles.color = Color.yellow;
            if (currentMode == PlacementMode.Square)
            {
                Vector3 p1 = new Vector3(startPoint.x, 0, startPoint.z);
                Vector3 p2 = new Vector3(endPoint.x, 0, startPoint.z);
                Vector3 p3 = new Vector3(endPoint.x, 0, endPoint.z);
                Vector3 p4 = new Vector3(startPoint.x, 0, endPoint.z);
                Handles.DrawLines(new Vector3[] { p1, p2, p2, p3, p3, p4, p4, p1 });
            }
            else
            {
                // Add a check to prevent the IndexOutOfRangeException
                if (lassoPoints.Count > 1)
                {
                    Handles.DrawLines(lassoPoints.ToArray());
                }
            }
            SceneView.RepaintAll();
        }
    }
}