using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class BoundaryTrigger : MonoBehaviour
{
    [SerializeField] private string[] validTags = { "Player" };
    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerExit;
    public UnityEvent onTriggerStay;

    public bool showDebugMessages = false;

    void OnTriggerEnter(Collider other)
    {
        // Check if the collider that entered the trigger is your player car
        if (validTags.Contains(other.tag))
        {
            onTriggerEnter?.Invoke();
        }
        else if(showDebugMessages)
        {
            Debug.LogWarning($"Collider with tag '{other.tag}' entered the boundary trigger, but it is not a valid tag. Valid tags are: {string.Join(", ", validTags)}", this);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (validTags.Contains(other.tag))
        {
            onTriggerStay?.Invoke();
        }
        else if (showDebugMessages)
        {
            Debug.LogWarning($"Collider with tag '{other.tag}' is STAYING in the boundary trigger, but it is not a valid tag. Valid tags are: {string.Join(", ", validTags)}", this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the collider that entered the trigger is your player car
        if (validTags.Contains(other.tag))
        {
            onTriggerExit?.Invoke();
        }
        else if(showDebugMessages)
        {
            Debug.LogWarning($"Collider with tag '{other.tag}' EXITED the boundary trigger, but it is not a valid tag. Valid tags are: {string.Join(", ", validTags)}", this);
        }
    }
}
