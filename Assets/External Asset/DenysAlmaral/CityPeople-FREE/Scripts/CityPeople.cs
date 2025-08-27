using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CityPeople
{
    public class CityPeople : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField]
        [Tooltip("Overrides palette materials, skips other objects")]
        private Material PaletteOverride;
        public string CurrentPaletteName { get; private set; }

        // Use string parameters for Animator, directly mapping to states
        [SerializeField] private string walkAnimStateName = "locom_f_basicWalk_30f"; // Name of your Walk state in Animator
        [SerializeField] private string idleAnimStateName = "idle_f_1_150f"; // Name of your Idle state in Animator
        [SerializeField] private string isWalkingParamName = "IsWalking"; // Name of your IsWalking boolean parameter

        private Animator animator;
        public const string people_pal_prefix = "people_pal";
        private List<Renderer> _paletteMeshes;

        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 1.5f; // Speed of walking
        [SerializeField] private float rotationSpeed = 5f; // Speed of rotation towards waypoint
        [SerializeField] private float waypointReachedThreshold = 0.5f; // How close to a waypoint to consider it reached
        [SerializeField] private float idleDuration = 3f; // How long to idle at a waypoint

        [Header("Pathfinding")]
        [Tooltip("Assign an empty GameObject with Transform children as waypoints.")]
        [SerializeField] private Transform waypointParent; // Parent object holding all waypoint Transforms
        [SerializeField]
        [Tooltip("The tag assigned to the waypoint where you want to start the idle animation")]
        private string idleWaypointTag = "IdleWaypoint";

        private List<Transform> waypoints;
        private int currentWaypointIndex = 0;

        private Rigidbody rb;

        private enum CharacterState
        {
            Walking,
            Idling,
            // You can add more states like Running, Talking, etc.
        }
        private CharacterState currentState = CharacterState.Idling; // Start in Idle state

        private void Awake()
        {
            var AllRenderers = gameObject.GetComponentsInChildren<Renderer>();
            _paletteMeshes = new List<Renderer>();
            foreach (Renderer r in AllRenderers)
            {
                var matName = r.sharedMaterial.name;
                var len = Math.Min(people_pal_prefix.Length, matName.Length);
                if (matName[0..len] == CityPeople.people_pal_prefix)
                {
                    _paletteMeshes.Add(r);
                }
            }
            if (_paletteMeshes.Count > 0)
            {
                CurrentPaletteName = _paletteMeshes[0].sharedMaterial.name;
            }

            if (PaletteOverride != null)
            {
                SetPalette(PaletteOverride);
            }
        }

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator component not found on " + gameObject.name);
                return;
            }

            // Setup Waypoints
            waypoints = new List<Transform>();
            if (waypointParent != null)
            {
                foreach (Transform child in waypointParent)
                {
                    waypoints.Add(child);
                }
            }
            else
            {
                Debug.LogWarning("Waypoint Parent is not assigned. Character will remain idle.", this);
            }

            foreach (Transform child in waypoints)
            {
                child.GetComponent<MeshRenderer> ().enabled = false;
            }

            // Start the state machine
            StartCoroutine(CharacterStateMachine());

            //collider for detect clicks near the character
            CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
            //average character dimentions
            collider.center = new Vector3(0f, 0.8f, 0f);
            collider.radius = 0.3f;
            collider.height = 1.77f;
            collider.direction = 1;

        }

        private IEnumerator CharacterStateMachine()
        {
            while (true) // Infinite loop for continuous behavior
            {
                switch (currentState)
                {
                    case CharacterState.Walking:
                        yield return StartCoroutine(HandleWalkingState());
                        break;
                    case CharacterState.Idling:
                        yield return StartCoroutine(HandleIdlingState());
                        break;
                }
                yield return null; // Small delay to prevent infinite loop issues if no state changes
            }
        }

        private IEnumerator HandleWalkingState()
        {
            //Debug.Log("Entering Walking State");
            animator.SetBool(isWalkingParamName, true); // Tell Animator to play walk animation

            if (waypoints.Count == 0)
            {
                Debug.LogWarning("No waypoints defined. Staying idle.");
                currentState = CharacterState.Idling;
                yield break; // Exit coroutine
            }

            // Ensure currentWaypointIndex is valid
            if (currentWaypointIndex >= waypoints.Count)
            {
                currentWaypointIndex = 0; // Loop back to the start of the path
            }

            Vector3 targetWaypoint = waypoints[currentWaypointIndex].position;

            while (Vector3.Distance(rb.position, targetWaypoint) > waypointReachedThreshold)
            {
                // Move towards waypoint
                Vector3 directionToWaypoint = (targetWaypoint - rb.position).normalized;
                rb.position += Time.deltaTime * walkSpeed * directionToWaypoint;

                // --- MODIFIED PART START ---
                // Rotate towards waypoint, but only on the Y axis (yaw)
                // We flatten the direction vector to ignore vertical difference
                Vector3 flatDirection = new Vector3(directionToWaypoint.x, 0, directionToWaypoint.z).normalized;


                // Only calculate targetRotation if there's a valid direction
                if (flatDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
                    rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
                // --- MODIFIED PART END ---

                yield return null; // Wait for the next frame
            }

            //Debug.Log($"Reached waypoint {currentWaypointIndex}");

            if(waypoints[currentWaypointIndex].CompareTag(idleWaypointTag))
                currentState = CharacterState.Idling; // Transition to Idling state

            currentWaypointIndex++; // Move to the next waypoint for the next walk cycle
        }

        private IEnumerator HandleIdlingState()
        {
            //Debug.Log("Entering Idling State");
            animator.SetBool(isWalkingParamName, false); // Tell Animator to play idle animation

            yield return new WaitForSeconds(idleDuration); // Wait for the specified idle time

            //Debug.Log("Idling done. Returning to Walking state.");
            currentState = CharacterState.Walking; // Transition back to Walking state
        }



        public void SetPalette(Material mat)
        {
            if (mat != null)
            {
                if (mat.name[0..people_pal_prefix.Length] == CityPeople.people_pal_prefix)
                {
                    CurrentPaletteName = mat.name;
                    foreach (Renderer r in _paletteMeshes)
                    {
                        r.material = mat;
                    }
                }
                else
                {
                    Debug.Log("Material name should start with 'palete_pal...' by convention.");
                }
            }
        }

        // We will no longer directly call PlayClip or PlayAnyClip for walk/idle
        // because the Animator Controller manages these transitions.
        // You can keep these for other specific animations if needed, but they won't be used for walk/idle logic.
        public void PlayClip(AnimationClip clip)
        {
            if (animator != null && clip != null)
            {
                // Consider using SetTrigger or SetBool if you integrate other animations into the Animator Controller
                animator.CrossFadeInFixedTime(clip.name, 0.2f); // Shorter blend for direct plays
            }
            else
            {
                Debug.LogWarning("Animator or clip is null, cannot play animation.");
            }
        }



    }
}
