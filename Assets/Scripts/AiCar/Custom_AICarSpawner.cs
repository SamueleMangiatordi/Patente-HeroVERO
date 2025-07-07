using SpinMotion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AiCarSpawner : MonoBehaviour
{
    public GameObject aiWaypointTrackerPrefab;
    [SerializeField] private bool spawnOnAwake = true;
    [SerializeField] private List<AICarSpawnData> aiCarSpawnDataList = new();

    private readonly List<(GameObject aiCar, Vector3 spawnPos, Quaternion spawnRot)> spawnedPlayers = new();

    private void Awake()
    {
        if (aiCarSpawnDataList.Count == 0)
        {
            Debug.LogError("No spawn points assigned");
        }

        foreach (AICarSpawnData data in aiCarSpawnDataList)
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

        //gameEvents.SpawnPlayersEvent.AddListener(OnSpawnPlayers);
        //gameEvents.RestartRaceEvent.AddListener(OnRestartRace);

        foreach (AICarSpawnData aiCarSpawnData in aiCarSpawnDataList)
            if (aiCarSpawnData.spawnPoint.TryGetComponent<Renderer>(out var renderer)) { renderer.enabled = false; }

        if (spawnOnAwake)
            SpawnPlayers();
    }

    public void SpawnPlayers()
    {
        foreach (AICarSpawnData aICarSpawnData in aiCarSpawnDataList)
        {
            GameObject aiCar = Instantiate(aICarSpawnData.aiCarPrefab, aICarSpawnData.spawnPoint.position, aICarSpawnData.spawnPoint.rotation);
            spawnedPlayers.Add((aiCar, aiCar.transform.position, aiCar.transform.rotation));

            Custom_AIWaypointTracker aiTracker = Instantiate(aiWaypointTrackerPrefab).GetComponent<Custom_AIWaypointTracker>();
            aiTracker.SetTracker(aICarSpawnData.aiWaypoints);
            aiTracker.SetupAICarCollider(aiCar.GetComponentInChildren<AICarWaypointTrackerColliderTrigger>().GetColliderTrigger());

            aiCar.GetComponent<CarAIControl>().SetTarget(aiTracker.transform); // replace with your car controller ai target to aim/follow

            BoundaryTrigger bTrig = aiCar.GetComponentInChildren<BoundaryTrigger>();
            bTrig.onTriggerEnter.AddListener(aICarSpawnData.onCollisionEvent.Invoke);
        }

        foreach (var (aiCar, _, _) in spawnedPlayers)
        {
            aiCar.GetComponent<CarFreeze>().OnToggleCarFreeze(false);
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

    [ContextMenu("Spawn Players")]
    public void OnSpawnPlayers()
    {
        SpawnPlayers();
        foreach (var (aiCar, _, _) in spawnedPlayers)
        {
            aiCar.GetComponent<CarFreeze>().OnToggleCarFreeze(false);
        }
    }
}


[Serializable]
public class AICarSpawnData
{
    public GameObject aiCarPrefab;
    public Transform spawnPoint;
    [Tooltip("The parent object that contains all the relative AI Waypoints of this car")]
    public GameObject WaypointsParent;

    public UnityEvent onCollisionEvent;


    public List<AIWaypoint> aiWaypoints; // List of waypoints for the AI car to follow
}
