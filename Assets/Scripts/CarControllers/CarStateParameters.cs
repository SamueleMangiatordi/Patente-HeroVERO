using Ezereal; // Assuming this namespace is used for EzerealLightController, etc.
using UnityEngine;
// Removed 'using static UnityEngine.Rendering.DebugUI;' as it was unused and might cause errors.

[System.Serializable]
public class CarStateParameters
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 linearVelocity;
    public Vector3 angularVelocity;
    public float currentSpeed; // Stored for UI or external logic, but linearVelocity is primary for physics
    public bool isStarted;
    public float currentThrottleInput;
    public float currentHandbrakeValue;
    public float currentSteerAngle;
    public float targetSteerAngle;

    // Removed EzerealCameraController as it should be managed by a separate system if needed.
    // public EzerealCameraController cameraController;

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
        // --- CORRECTED: Accessing the correct public properties from SimplifiedCarController ---
        currentThrottleInput = carController.CurrentThrottleInput;
        currentHandbrakeValue = carController.CurrentHandbrakeValue; // Corrected from carController.CurrentThrottleInput
        currentSteerAngle = carController.CurrentSteerAngle;       // Corrected from carController.CurrentThrottleInput
        targetSteerAngle = carController.TargetSteerAngle;
        // --------------------------------------------------------------------------------------
        currentSpeed = carController.GetCurrentSpeed();

        // Removed cameraController capture.
        // cameraController = carController.gameObject.GetComponent<EzerealCameraController>();
    }

    /// <summary>
    /// Applies the stored state parameters back to the SimplifiedCarController.
    /// This method performs a robust state restoration.
    /// </summary>
    /// <param name="carController">The SimplifiedCarController instance to apply state to.</param>
    /// <param name="steerAngle">Optional steer angle to apply. If not specified the saved steer will used.</param> 
    public void ApplyToCarController(SimplifiedCarController carController, float steerAngle = 100)
    {
        if (carController.vehicleRB == null)
        {
            Debug.LogError("Vehicle Rigidbody is not assigned in carController! Cannot apply stored state.");
            return;
        }

        // Store original kinematic state
        bool originalIsKinematic = carController.vehicleRB.isKinematic;

        // Temporarily make the Rigidbody kinematic to prevent physics interference during teleport
        // This is crucial for a clean, immediate placement without unwanted forces.
        carController.vehicleRB.isKinematic = true;

        // Apply position and rotation immediately
        carController.vehicleRB.position = position;
        carController.vehicleRB.rotation = rotation;

        // CRITICAL: Immediately clear all existing velocities and forces from Rigidbody and WheelColliders
        // This ensures no "leftover" momentum or applied torques cause unwanted movement.
#if UNITY_6000_0_OR_NEWER
        carController.vehicleRB.linearVelocity = Vector3.zero;
#else
        carController.vehicleRB.velocity = Vector3.zero;
#endif
        carController.vehicleRB.angularVelocity = Vector3.zero;

        // Explicitly reset wheel collider torques and brakes
        carController.frontLeftWheelCollider.motorTorque = 0;
        carController.frontRightWheelCollider.motorTorque = 0;
        carController.rearLeftWheelCollider.motorTorque = 0;
        carController.rearRightWheelCollider.motorTorque = 0;

        carController.frontLeftWheelCollider.brakeTorque = 0;
        carController.frontRightWheelCollider.brakeTorque = 0;
        carController.rearLeftWheelCollider.brakeTorque = 0;
        carController.rearRightWheelCollider.brakeTorque = 0;

        // After setting position/rotation/zeroing velocities, restore kinematic state
        // This allows physics to resume correctly with the *newly set* velocities.
        carController.vehicleRB.isKinematic = originalIsKinematic;

        // Apply the stored linear and angular velocities
#if UNITY_6000_0_OR_NEWER
        carController.vehicleRB.linearVelocity = linearVelocity;
#else
        carController.vehicleRB.velocity = linearVelocity;
#endif
        carController.vehicleRB.angularVelocity = angularVelocity;

        // Restore input states on the car controller.
        // The car's FixedUpdate will then apply motor/brake torques based on these inputs.
        carController.SetIsStarted(isStarted);
        carController.CurrentThrottleInput = Mathf.Clamp(currentThrottleInput, -1, 1);
        carController.CurrentHandbrakeValue = Mathf.Clamp(currentHandbrakeValue, 0, 1);
        carController.TargetSteerAngle = steerAngle == 100 ? 0 : targetSteerAngle;
        carController.CurrentSteerAngle = steerAngle == 100 ? 0 : currentSteerAngle; // Set the current steer angle directly for immediate visual/physics accuracy

        // Immediately try to put the Rigidbody to sleep if it's supposed to be stationary.
        // If it has a non-zero velocity, it will wake up on its own.
        if (linearVelocity.sqrMagnitude < 0.001f && angularVelocity.sqrMagnitude < 0.001f) // Use sqrMagnitude for float comparison
        {
            carController.vehicleRB.Sleep();
        }
        else
        {
            carController.vehicleRB.WakeUp(); // Explicitly wake up if it should be moving
        }

        // Update wheel meshes and UI immediately to reflect the restored state
        carController.UpdateWheelMeshes();
        carController.UpdateSpeedText(carController.GetCurrentSpeed());

        // REMOVED: carController.SetCarSpeed(currentSpeed, currentSpeed > 0);
        // This call is redundant and can interfere. LinearVelocity already sets the precise speed.
        // If a *new* target speed is needed after restoration, it should be an explicit call from TriggerableUserGuide.

        // Removed cameraController logic, as it should be handled by a separate system
        // if (cameraController != null)
        // {
        //     cameraController.ResetCurrentCameraRotation();
        // }
    }

    public void TeleportCarToSavedPos(SimplifiedCarController carController, float desiredSpeed = 0, bool forward = true)
    {
        if (carController == null || carController.vehicleRB == null)
        {
            Debug.LogError("Cannot teleport to car: CarController or its Rigidbody is not assigned.");
            return;
        }
        float velocity = desiredSpeed > 0 ? desiredSpeed : currentSpeed; // Use desired speed if provided, otherwise use stored speed

        carController.TeleportCar(position, rotation, velocity, forward);
    }
}