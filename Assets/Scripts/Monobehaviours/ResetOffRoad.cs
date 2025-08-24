using UnityEngine;
using UnityEngine.Events;

public class ResetOffRoad : MonoBehaviour
{
    [SerializeField] private float resetDistance = 5f; // Distance behind the checkpoint to teleport to

    public UnityEvent onOffroadEvent;

    private SimplifiedCarController _carController;
    private CheckpointManager _checkpointManager;

    private void Start()
    {
        _carController = FindAnyObjectByType<SimplifiedCarController>();
        _checkpointManager = FindAnyObjectByType<CheckpointManager>();
    }

    public void OnOffRoad()
    {
        Transform currentCheckpointTransform = _checkpointManager.GetLastReachedCheckpoint();

        if (currentCheckpointTransform == null)
        {
            Debug.Log("CurrentActiveCheckpoint is null, using ResetOffRoad's transform as fallback.");
            currentCheckpointTransform = this.transform;
        }

        // Calculate a position slightly behind the current checkpoint
        Vector3 resetPosition = currentCheckpointTransform.position - currentCheckpointTransform.forward * resetDistance;

        // Use the checkpoint's rotation for the car's new rotation
        Quaternion resetRotation = currentCheckpointTransform.rotation;

        // Teleport the car
        _carController.TeleportCar(resetPosition, resetRotation, 0f, true);

        onOffroadEvent?.Invoke();
    }
    
}
