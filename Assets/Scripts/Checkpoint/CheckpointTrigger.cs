using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [SerializeField] private CheckpointManager checkpointManager;
    [SerializeField] private SignInteractionController signInteractionController;

    private void Start()
    {
        checkpointManager = FindAnyObjectByType<CheckpointManager>();
        signInteractionController = FindAnyObjectByType<SignInteractionController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GetComponent<Collider>().enabled = false;
            checkpointManager.GoToNextCheckpoint();
            Debug.Log("✅ Checkpoint attraversato");

            if (signInteractionController != null)
            {
                signInteractionController.resetPos = this.transform;
                Debug.Log("✅ Checkpoint attraversato - resetPos aggiornato");
            }

        }
    }
}


