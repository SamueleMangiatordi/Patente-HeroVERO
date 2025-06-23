using UnityEngine;

[System.Serializable]
public class CarStateParameters
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 linearVelocity;
    public Vector3 angularVelocity;
    public float currentSpeed; // Optional: Store current speed if needed for UI or logic
    public bool isStarted;
    public float currentThrottleInput;
    public float currentHandbrakeValue;
    public float currentSteerAngle;
    public float targetSteerAngle;


    /// <summary>
    /// Captures the current state of the SimplifiedCarController.
    /// </summary>
    /// <param name="carController">The SimplifiedCarController instance to capture state from.</param>
    public CarStateParameters(SimplifiedCarController carController)
    {
        if (carController.vehicleRB != null)
        {
            position = carController.vehicleRB.position;
            rotation = carController.vehicleRB.rotation;
#if UNITY_6000_0_OR_NEWER
            linearVelocity = carController.vehicleRB.linearVelocity;
#else
            linearVelocity = carController.vehicleRB.velocity;
#endif
            angularVelocity = carController.vehicleRB.angularVelocity;
        }
        else
        {
            Debug.LogWarning("Vehicle Rigidbody is null when creating CarStateParameters. Defaulting to zero values.");
            position = Vector3.zero;
            rotation = Quaternion.identity;
            linearVelocity = Vector3.zero;
            angularVelocity = Vector3.zero;
        }

        isStarted = carController.isStarted;
        // Accessing private fields via public getters
        currentThrottleInput = carController.GetCurrentThrottleInput();
        currentHandbrakeValue = carController.GetCurrentHandbrakeValue();
        currentSteerAngle = carController.GetCurrentSteerAngle();
        targetSteerAngle = carController.GetTargetSteerAngle();
        currentSpeed = carController.GetCurrentSpeed(); // Optional: Store current speed if needed for UI or logic
    }

    /// <summary>
    /// Applies the stored state parameters back to the SimplifiedCarController.
    /// This method performs a robust state restoration, including kinematics for clean teleportation.
    /// </summary>
    /// <param name="carController">The SimplifiedCarController instance to apply state to.</param>
    public void ApplyToCarController(SimplifiedCarController carController)
    {
        if (carController.vehicleRB == null)
        {
            Debug.LogError("Vehicle Rigidbody is not assigned in carController! Cannot apply stored state.");
            return;
        }

        // Store original kinematic state
        bool originalIsKinematic = carController.vehicleRB.isKinematic;
        // Temporarily make the Rigidbody kinematic for a clean teleport
        carController.vehicleRB.isKinematic = true;

        // Apply position and rotation
        carController.vehicleRB.position = position;
        carController.vehicleRB.rotation = rotation;

        // Restore kinematic state
        carController.vehicleRB.isKinematic = originalIsKinematic; // Or 'false' if it should always be dynamic

        // Apply velocities
#if UNITY_6000_0_OR_NEWER
        carController.vehicleRB.linearVelocity = linearVelocity;
#else
        carController.vehicleRB.velocity = linearVelocity;
#endif
        carController.vehicleRB.angularVelocity = angularVelocity;

        // Force the Rigidbody to sleep to ensure it's at rest if velocities are zero.
        // If velocities are non-zero, it should wake up automatically in the next physics step.
        carController.vehicleRB.Sleep();

        // Restore input states and other internal flags via setters
        carController.SetIsStarted(isStarted);
        carController.SimulateThrottleInput(currentThrottleInput);
        carController.SimulateHandBrake(currentHandbrakeValue);
        carController.SetTargetSteerAngle(targetSteerAngle);
        carController.SetCurrentSteerAngle(currentSteerAngle);

        // REMOVED: Explicitly setting motorTorque and brakeTorque to 0.
        // These should be handled by the FixedUpdate in SimplifiedCarController
        // based on the restored currentThrottleInput and currentHandbrakeValue.
        // Removing these lines allows the car to retain its restored velocity
        // if the saved state included active throttle/handbrake.

        // Update wheel meshes and UI immediately to reflect the restored state
        carController.UpdateWheelMeshes();
        // The currentSpeed of the car controller will be implicitly updated in FixedUpdate
        // But we can manually update the UI for immediate feedback
        carController.UpdateSpeedText(carController.GetCurrentSpeed());

        carController.SetCarSpeed(currentSpeed, currentSpeed > 0); // Optional: Set the car's speed if needed for UI or logic
    }
}