using SpinMotion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class AiCarSpawner : MonoBehaviour
{
    public GameObject aiWaypointTrackerPrefab;

    [SerializeField] private bool spawnOnAwake = true;

    [Header("SpawnSettings")]
    [SerializeField] private float minTimeBetweenSpawn = 0f;
    [SerializeField] private float maxTimeBetweenSpawn = 0f;

    [SerializeField] private bool duplicateFirstCarData = false;
    [Tooltip("It will take the first ai car data and use that data to spawn to amount of car wanted, ignoring the other car data")]
    [SerializeField] private int numberOfCarsToSpawn = 1;
    [SerializeField] private List<SpawnPointData> spawnPoints = new();

    [SerializeField] public List<AICarData> aiCarDataList = new();

    public static List<AICarData> AiCarDataList = new();

    private readonly List<(GameObject aiCar, Vector3 spawnPos, Quaternion spawnRot)> spawnedPlayers = new();

    private void Awake()
    {
        if (aiCarDataList.Count == 0)
        {
            Debug.LogError("No car assigned");
        }

        if (duplicateFirstCarData)
        {
            AICarData firstCarData = aiCarDataList[0];
            aiCarDataList.RemoveAll(item => item != firstCarData); // Keep only the first car data

            for (int i = 1; i < numberOfCarsToSpawn; i++)
            {
                SpawnPointData spawnPoint = spawnPoints[i % spawnPoints.Count];
                AICarData temp = new AICarData
                {
                    aiCarPrefab = firstCarData.aiCarPrefab,
                    WaypointsParent = firstCarData.WaypointsParent,
                    carTag = firstCarData.carTag,
                    onCollisionEvent = firstCarData.onCollisionEvent,
                    spawnTransform = spawnPoint.spawnTransform,
                    startingWaypointTransform = spawnPoint.startingWaypointTransform
                };
                aiCarDataList.Add(temp);
                
            }
        }

        foreach (AICarData data in aiCarDataList)
        {
            // Initialize the list for each AICarSpawnData instance if it's not already
            if (data.aiWaypoints == null)
            {
                data.aiWaypoints = new List<AIWaypoint>();
            }
            else
            {
                data.aiWaypoints.Clear(); // Clear existing waypoints if you're re-populating
            }

            if (data.WaypointsParent != null)
            {
                // Iterate over each child Transform of the AiWaypoints_parent
                foreach (Transform aiWaypoint in data.WaypointsParent.transform)
                {
                    // Attempt to get the MeshRenderer component from the child
                    MeshRenderer meshRendererComponent = aiWaypoint.GetComponent<MeshRenderer>();

                    data.aiWaypoints.Add(new AIWaypoint
                    {
                        aiWaypointTransform = aiWaypoint, // childTransform is already a Transform
                        aiWaypointMeshRenderer = meshRendererComponent
                    });
                }
            }
            else
            {
                Debug.LogWarning($"AICarSpawnData for '{data.aiCarPrefab.name}' has no 'AiWaypoints_parent' assigned. Skipping waypoint collection for this car.", this);
            }
        }

        foreach (AICarData aiCarSpawnData in aiCarDataList)
            if (aiCarSpawnData.spawnTransform.TryGetComponent<Renderer>(out var renderer)) { renderer.enabled = false; }

        if (spawnOnAwake)
            SpawnAllPlayers();

        AiCarDataList = aiCarDataList; // Store the list of AICarData for global access
    }

    private void Start()
    {
        foreach (AICarData aiCarSpawnData in aiCarDataList)
        {
            if (aiCarSpawnData.spawnTransform.TryGetComponent<Renderer>(out var renderer)) { renderer.enabled = false; }
            if (aiCarSpawnData.WaypointsParent != null)
                foreach (Transform waypoint in aiCarSpawnData.WaypointsParent.transform)
                    if (waypoint.TryGetComponent<MeshRenderer>(out var meshRenderer)) { meshRenderer.enabled = false; }
        }
    }

    public void SpawnAllPlayers()
    {
        StartCoroutine(SpawnPlayersWithDelay());
        return;

    }


    IEnumerator SpawnPlayersWithDelay()
    {
        for (int i = 0; i < aiCarDataList.Count; i++)
        {
            SpawnPlayer(i);
            yield return new WaitForSeconds(GetSpawnDelay());
        }
    }

    public void ResetAiCarPosition()
    {
        // reallocate players position and rotation to initial spawn points
        foreach (var (aiCar, spawnPos, spawnRot) in spawnedPlayers)
        {
            aiCar.transform.position = spawnPos;

            // check for main and child Rigidbodies
            Rigidbody[] rigidbodies = aiCar.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in rigidbodies)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.rotation = spawnRot;
            }

            if (rigidbodies.Length == 0)
            {
                aiCar.transform.rotation = spawnRot;
            }
        }
    }

    public static void IgnoreAllAiPlayerCollision(float duration)
    {
        foreach (AICarData aiCarData in AiCarDataList)
        {
            if (aiCarData.carAIControl != null)
                aiCarData.carAIControl.IgnoreAiPlayerCollision(duration);
        }
    }

    public void SpawnPlayer(int index)
    {
        AICarData aICarSpawnData = aiCarDataList[index];

        GameObject aiCar = Instantiate(aICarSpawnData.aiCarPrefab, aICarSpawnData.spawnTransform.position, aICarSpawnData.spawnTransform.rotation);
        spawnedPlayers.Add((aiCar, aiCar.transform.position, aiCar.transform.rotation));

        Custom_AIWaypointTracker aiTracker = Instantiate(aiWaypointTrackerPrefab).GetComponent<Custom_AIWaypointTracker>();
        aiTracker.SetTracker(aICarSpawnData.aiWaypoints);
        aiTracker.SetupAICarCollider(aiCar.GetComponentInChildren<AICarWaypointTrackerColliderTrigger>().GetColliderTrigger());
        if(aICarSpawnData.startingWaypointTransform != null)
            aiTracker.SetWaypointToReach(aICarSpawnData.startingWaypointTransform); // set the first waypoint to reach as the spawn point of the car

        CarAIControl carAiControl = aiCar.GetComponent<CarAIControl>();
        carAiControl.SetTarget(aiTracker.transform); // replace with your car controller ai target to aim/follow

        BoundaryTrigger bTrig = aiCar.GetComponentInChildren<BoundaryTrigger>();
        bTrig.onTriggerEnter.AddListener(aICarSpawnData.onCollisionEvent.Invoke);

        GameObject colBottomObj = aiCar.transform.Find("Colliders").Find("ColliderBottom").gameObject;
        colBottomObj.tag = aICarSpawnData.carTag.ToString(); // Assign the tag to the bottom collider object

        aICarSpawnData.aiCar = aiCar; // Assign the instantiated car to the AICarData for reference
        aICarSpawnData.carAIControl = carAiControl; // Assign the CarAIControl component to the AICarData for reference

        aiCar.GetComponent<CarFreeze>().OnToggleCarFreeze(false);

    }

    private float GetSpawnDelay()
    {
        // If min and max times are equal, return the fixed time.
        if (Mathf.Approximately(minTimeBetweenSpawn, maxTimeBetweenSpawn))
        {
            return minTimeBetweenSpawn;
        }
        // Otherwise, return a random time between the min and max values.
        else
        {
            return Random.Range(minTimeBetweenSpawn, maxTimeBetweenSpawn);
        }
    }


    [ContextMenu("Spawn Players")]
    public void OnSpawnPlayers()
    {
        SpawnAllPlayers();
        foreach (var (aiCar, _, _) in spawnedPlayers)
        {
            aiCar.GetComponent<CarFreeze>().OnToggleCarFreeze(false);
        }
    }
}


[Serializable]
public class AICarData
{
    [Tooltip("The tag to assing at the car when spawning. Use this to find which car is used for which purpose. " +
        "Example : For 'RightOfWay' signal, you can assing the tag 'RightOfWay' so that you can search for the car that occupies of the logic for that signal")]
    public AiCarType carTag;

    [Header("AI Car Spawn Settings")]
    public GameObject aiCarPrefab;
    public Transform spawnTransform;
    public Transform startingWaypointTransform;
    [Tooltip("The parent object that contains all the relative AI Waypoints of this car")]
    public GameObject WaypointsParent;

    public UnityEvent onCollisionEvent;

    [Header("Automatic References (Leave Empty)")]
    [Tooltip("The AI car GameObject that will be spawned at runtime.")]
    public GameObject aiCar;
    [Tooltip("List of AI waypoints for this car to follow. They are finded automatically, leave this field empty.")]
    public List<AIWaypoint> aiWaypoints; // List of waypoints for the AI car to follow
    public CarAIControl carAIControl; // Reference to the CarAIControl component

}

[Serializable]
public class SpawnPointData
{
    public Transform spawnTransform;
    public Transform startingWaypointTransform;
}