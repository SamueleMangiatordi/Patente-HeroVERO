using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private CheckpointManager checkpointManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GetComponent<Collider>().enabled = false;
            checkpointManager.GoToNextCheckpoint();
            Debug.Log("✅ Checkpoint attraversato");

        }
    }
}


