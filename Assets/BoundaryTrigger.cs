using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class BoundaryTrigger : MonoBehaviour
{
    [SerializeField] private string[] validTags = { "Player" };
    [SerializeField] public UnityEvent onTriggerEnter;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Valid tags : " + validTags.ToString() + " / Object tag : " + other.tag);
        // Check if the collider that entered the trigger is your player car
        if (validTags.Contains(other.tag))
        {
            onTriggerEnter?.Invoke();
        }
    }
}
