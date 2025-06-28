using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class BoundaryTrigger : MonoBehaviour
{
    [SerializeField] private string[] validTags = { "Player" };
    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerExit;

    void OnTriggerEnter(Collider other)
    {
        // Check if the collider that entered the trigger is your player car
        if (validTags.Contains(other.tag))
        {
            onTriggerEnter?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the collider that entered the trigger is your player car
        if (validTags.Contains(other.tag))
        {
            onTriggerExit?.Invoke();
        }
    }
}
