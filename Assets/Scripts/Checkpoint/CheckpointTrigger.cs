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

            // Trova il SignInteractionController della scena
            SignInteractionController sic = FindObjectOfType<SignInteractionController>();
            if (sic != null)
            {
                sic.resetPos = this.transform;
                Debug.Log("✅ Checkpoint attraversato - resetPos aggiornato");
            }

        }
    }
}


