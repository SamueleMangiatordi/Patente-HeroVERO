using NUnit.Framework;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keep this object across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Tooltip("Used as shortcut to assign checkpoints. Parent object containing all checkpoint transforms as children.")]
    [SerializeField] private Transform checkpointsParent = null;

    [Tooltip("List of checkpoint transforms in the order they should be reached.")]
    [SerializeField] private Transform[] checkpoints; // lista ordinata
    private int currentCheckpointIndex = 0;


    private void Start()
    {
        if(checkpointsParent != null)
        {
            int childCount = checkpointsParent.childCount;
            checkpoints = new Transform[childCount];
            for (int i = 0; i < childCount; i++)
            {
                checkpoints[i] = checkpointsParent.GetChild(i);
            }
        }

        ActivateCheckpoint(currentCheckpointIndex);
    }

    private void ActivateCheckpoint(int index)
    {
        for (int i = 0; i < checkpoints.Length; i++)
        {
            checkpoints[i].gameObject.SetActive(i == index);
        }
    }

    public void GoToNextCheckpoint()
    {
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
    public Transform GetCurrentActiveCheckpoint()
    {
        if (currentCheckpointIndex < checkpoints.Length)
        {
            int index = currentCheckpointIndex - 1;

            if ( currentCheckpointIndex < 0 )
            {
                return checkpoints[0]; // Return the first checkpoint if no checkpoints have been reached yet
            }

            return checkpoints[currentCheckpointIndex-1] ;
        }
        return null; // or handle the case where there are no checkpoints
    }
}
