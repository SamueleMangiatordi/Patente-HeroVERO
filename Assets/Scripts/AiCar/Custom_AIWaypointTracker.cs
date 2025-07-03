using SpinMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Custom_AIWaypointTracker : MonoBehaviour
{
    public Vector3 trackerScale;

    private List<AIWaypoint> aiWaypoints = new();
    private Transform currentWaypoint;
    private int currentIndex;
    private Collider aiCarCollider;
    private Collider waypointTrackerCollider;

    private void Awake()
    {
        waypointTrackerCollider = GetComponent<BoxCollider>();
        transform.localScale = trackerScale;
        //ResetTracker();
    }

    private void ResetTracker()
    {
        currentIndex = 0;
        UpdateTrackerPosition();
    }

    public void SetTracker(List<AIWaypoint> aiWaypoints)
    {
        this.aiWaypoints = aiWaypoints;
        currentIndex = 0;
        UpdateTrackerPosition();
    }
    public void SetupAICarCollider(Collider aiCarCollider)
    {
        this.aiCarCollider = aiCarCollider;
    }

    IEnumerator OnTriggerEnter(Collider collider)
    {
        if (aiWaypoints.Count == 0)
        {
            Debug.LogWarning("No AI waypoints set for tracker. Please set waypoints before tracking.");
            yield break; // No waypoints to track
        }
        if (collider.GetInstanceID() == aiCarCollider.GetInstanceID())
        {
            waypointTrackerCollider.enabled = false;
            currentIndex++;
            if (currentIndex >= aiWaypoints.Count)
                currentIndex = 0;

            UpdateTrackerPosition();
            yield return new WaitForSeconds(0.1f);
            waypointTrackerCollider.enabled = true;
        }
    }

    private void UpdateTrackerPosition()
    {
        currentWaypoint = aiWaypoints[currentIndex].aiWaypointTransform;
        this.transform.position = currentWaypoint.position;
    }
}

