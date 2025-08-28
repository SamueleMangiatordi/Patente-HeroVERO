using NUnit.Framework;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [SerializeField] private GameObject automaticDisablePanel;
    [SerializeField] private string triggerTag = "objectiveCompleted";

    [Tooltip("Used as shortcut to assign checkpoints. Parent object containing all checkpoint transforms as children.")]
    [SerializeField] private Transform checkpointsParent = null;

    [Tooltip("List of checkpoint transforms in the order they should be reached.")]
    [SerializeField] private Transform[] checkpoints; // lista ordinata
    private int currentCheckpointIndex = 0;

    [SerializeField] private AudioSource checkpointReachedSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        if (checkpointsParent != null)
        {
            int childCount = checkpointsParent.childCount;
            checkpoints = new Transform[childCount];
            for (int i = 0; i < childCount; i++)
            {
                checkpoints[i] = checkpointsParent.GetChild(i);
            }
        }
    }

    private void Start()
    {
        checkpointReachedSound = checkpointReachedSound ?? GameObject.Find("audio e video").transform.Find("CheckpointReachedSound").GetComponent<AudioSource>();


        ActivateCheckpoint(currentCheckpointIndex);
    }

    private void ActivateCheckpoint(int index)
    {
        for (int i = 0; i < checkpoints.Length; i++)
        {
            checkpoints[i].gameObject.SetActive(i == index);
        }

        int oldCheckpointIndex = index - 1;

        if (oldCheckpointIndex < 1)
            return; // Do not play sound for the first checkpoint activation at the start of the

        checkpointReachedSound.Play();
        if (automaticDisablePanel != null && checkpoints[oldCheckpointIndex].CompareTag(triggerTag))
        {
            automaticDisablePanel.SetActive(true);
        }
    }

    public void GoToNextCheckpoint(Transform checkpointReached)
    {
        if (currentCheckpointIndex >= checkpoints.Length)
        {
            Debug.Log("All checkpoint reached, cannot go to the next one");
            return;
        }
        if (checkpoints[currentCheckpointIndex] != checkpointReached)
        {
            Debug.LogWarning("Checkpoint raggiunto non corrisponde al checkpoint attivo. Ignorando.");
            return;
        }

        currentCheckpointIndex++;
        if (currentCheckpointIndex < checkpoints.Length)
        {
            ActivateCheckpoint(currentCheckpointIndex);
        }
        else
        {
            Debug.Log("🚩 Tutti i checkpoint completati!");
        }
    }

    /// <summary>
    /// Return the checkpoint that the player needs to reach next, not the one already reached.
    /// </summary>
    /// <returns></returns>

    public Transform GetCheckpointToReach()
    {
        return checkpoints[currentCheckpointIndex];
    }

    /// <summary>
    /// Return the last checkpoint the player reached, which is the current active checkpoint.
    /// This is useful for resetting the car to the last checkpoint.
    /// </summary>
    /// <returns></returns>
    public Transform GetLastReachedCheckpoint()
    {
        // Check for a valid and populated array
        if (checkpoints == null || checkpoints.Length == 0)
        {
            Debug.LogWarning("Checkpoints array is null or empty. Returning null.");
            return null;
        }


        // The 'currentCheckpointIndex' typically points to the *next* checkpoint to reach.
        // The last reached checkpoint is therefore at index 'currentCheckpointIndex - 1'.
        int lastReachedIndex = currentCheckpointIndex - 1;

        // If no checkpoints have been reached yet, 'lastReachedIndex' will be -1.
        if (lastReachedIndex < 0)
        {
            Debug.Log("First checkpoint not reached yet, returning the first checkpoint as fallback.");
            return checkpoints[0]; // Return null to indicate no checkpoint has been reached yet
        }

        // Ensure the index is within the bounds of the array
        if (lastReachedIndex < checkpoints.Length)
        {
            Debug.Log("Returning last reached checkpoint at index: " + lastReachedIndex);
            return checkpoints[lastReachedIndex];
        }

        // If the index is out of bounds (e.g., race finished), return the last checkpoint in the list
        Debug.LogWarning("Current checkpoint index is out of bounds. Returning the final checkpoint.");
        return checkpoints[checkpoints.Length - 1];


    }
}
