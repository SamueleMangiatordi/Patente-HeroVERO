using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [SerializeField] private Transform[] checkpoints; // lista ordinata
    private int currentCheckpointIndex = 0;

    private void Start()
    {
        Debug.Log("Script CheckpointTrigger attivo!");
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

    public Transform GetCurrentCheckpoint()
    {
        return checkpoints[currentCheckpointIndex];
    }
}
