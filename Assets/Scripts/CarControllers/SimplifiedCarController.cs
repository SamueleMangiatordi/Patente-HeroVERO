using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using Ezereal;
using UnityEngine.Events;
using System.Collections;
using Unity.VisualScripting;

public class SimplifiedCarController : MonoBehaviour // This is the main system resposible for car control.
{
    public UnityEvent<float> onThrottle;
    public UnityEvent<float> onHandBrake;
    public UnityEvent<float> onSteer;

    [SerializeField] private bool bypassingInputs = false; // Flag to indicate if inputs are being bypassed

    [Tooltip("Time to bypass inputs after setting speed. This is useful for tutorials or when you want to set a speed without user interference.")]
    [SerializeField] float bypassInputsTime = 0.5f; // Time to bypass inputs after setting speed

    [Header("Ezereal References")]
    [SerializeField] EzerealLightController ezerealLightController;
    [SerializeField] SimplifiedSoundController simplifiedSoundController;
    [SerializeField] SimplifiedWheelFrictionController SimplifiedWheelFrictionController;

    [Header("References")]

    public Rigidbody vehicleRB;
    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider rearLeftWheelCollider;
    public WheelCollider rearRightWheelCollider;
    WheelCollider[] wheels;

    [SerializeField] Transform frontLeftWheelMesh;
    [SerializeField] Transform frontRightWheelMesh;
    [SerializeField] Transform rearLeftWheelMesh;
    [SerializeField] Transform rearRightWheelMesh;

    [SerializeField] Transform steeringWheel;

    [SerializeField] TMP_Text currentSpeedTMP_UI;
    [SerializeField] TMP_Text currentSpeedTMP_Dashboard;
    [SerializeField] Slider accelerationSlider;

    [Header("Settings")]
    public bool isStarted = true;

    public float maxForwardSpeed = 100f; // 100f default
    public float maxReverseSpeed = 30f; // 30f default
    public float horsePower = 1000f; // 100f0 default
    public float brakePower = 2000f; // 2000f default
    public float handbrakeForce = 3000f; // 3000f default
    public float maxSteerAngle = 35f; // 30f default
    public float steeringSpeed = 5f; // 0.5f default
    public float stopThreshold = 1f; // 1f default. At what speed car will make a full stop
    public float decelerationSpeed = 0.5f; // 0.5f default
    public float maxSteeringWheelRotation = 360f; // 360 for real steering wheel. 120 would be more suitable for racing.

    [Header("Drive Type")]
    public DriveTypes driveType = DriveTypes.RWD;

    [Header("Debug Info")]
    public bool stationary = true;
    [SerializeField] float currentSpeed = 0f;
    [SerializeField] float currentAccelerationValue = 0f;
    [SerializeField] float currentReverseValue = 0f;

    // Made public properties so CarMovementAdapter can directly set them
    public float CurrentHandbrakeValue { get; set; } = 0f;
    public float CurrentSteerAngle { get; set; } = 0f; // Actual angle applied
    public float TargetSteerAngle { get; set; } = 0f; // Target angle from input


    [SerializeField] float FrontLeftWheelRPM = 0f;
    [SerializeField] float FrontRightWheelRPM = 0f;
    [SerializeField] float RearLeftWheelRPM = 0f;
    [SerializeField] float RearRightWheelRPM = 0f;

    [SerializeField] float speedFactor = 0f; // Leave at zero. Responsible for smooth acceleration and near-top-speed slowdown.


    // Add these public getters and setters to access private fields
    public float GetCurrentSpeed() { return currentSpeed; }
    public void SetIsStarted(bool value) { isStarted = value; } // New setter for isStarted

    // Made public property so CarMovementAdapter can directly set it
    public float CurrentThrottleInput { get; set; } = 0f;


    private void Awake()
    {
        wheels = new WheelCollider[]
        {
            frontLeftWheelCollider,
            frontRightWheelCollider,
            rearLeftWheelCollider,
            rearRightWheelCollider,
        };

        if (ezerealLightController == null)
        {
            Debug.LogWarning("EzerealLightController reference is missing. Ignore or attach one if you want to have light controls.");
        }

        if (simplifiedSoundController == null)
        {
            Debug.LogWarning("EzerealSoundController reference is missing. Ignore or attach one if you want to have engine sounds.");
        }

        if (SimplifiedWheelFrictionController == null)
        {
            Debug.LogWarning("EzerealWheelFrictionController reference is missing. Ignore or attach one if you want to have friction controls.");
        }

        if (vehicleRB == null)
        {
            Debug.LogError("VehicleRB reference is missing for EzerealCarController!");
        }

        if (isStarted)
        {
            Debug.Log("Car is started.");

            if (ezerealLightController != null)
            {
                ezerealLightController.MiscLightsOn();
            }

            if (simplifiedSoundController != null)
            {
                simplifiedSoundController.TurnOnEngineSound();
            }
        }
    }

    void OnStartCar()
    {
        isStarted = !isStarted;

        if (isStarted)
        {
            Debug.Log("Car started.");

            if (ezerealLightController != null)
            {
                ezerealLightController.MiscLightsOn();
            }

            if (simplifiedSoundController != null)
            {
                simplifiedSoundController.TurnOnEngineSound();
            }

        }
        else if (!isStarted)
        {
            Debug.Log("Car turned off");

            if (ezerealLightController != null)
            {
                ezerealLightController.AllLightsOff();
            }

            if (simplifiedSoundController != null)
            {
                simplifiedSoundController.TurnOffEngineSound();
            }

            frontLeftWheelCollider.motorTorque = 0;
            frontRightWheelCollider.motorTorque = 0;
            rearLeftWheelCollider.motorTorque = 0;
            rearRightWheelCollider.motorTorque = 0;
        }


    }

    void OnThrottle(InputValue throttleValue)
    {
        if (bypassingInputs)
        {
            CurrentThrottleInput = 0; // <--- Ensure this is explicitly set to zero
            return; // Ignore input if bypassing
        }

        CurrentThrottleInput = throttleValue.Get<float>();
        onThrottle?.Invoke(CurrentThrottleInput);
        //Debug.Log("Acceleration: " + currentAccelerationValue.ToString());

        if (isStarted && ezerealLightController != null)
        {
            if (CurrentThrottleInput < 0)
            {
                ezerealLightController.BrakeLightsOn();
            }
            else
            {
                ezerealLightController.BrakeLightsOff();
            }
        }
    }

    void ApplyMotorAndBrakeTorque()
    {
        if (isStarted)
        {
            float motorTorque = 0f;
            float brakeTorque = 0f;

            // Calculate how close the car is to top speed
            // as a number from zero to one
            speedFactor = Mathf.InverseLerp(0, maxForwardSpeed, currentSpeed);

            // Use that to calculate how much torque is available 
            // (zero torque at top speed)
            float currentMotorTorque = Mathf.Lerp(horsePower, 0, speedFactor);
            // Determine if we are accelerating forward or backward
            if (CurrentThrottleInput > 0f) // 'W' is pressed (or positive input)
            {
                if (currentSpeed < maxForwardSpeed)
                {
                    motorTorque = CurrentThrottleInput * currentMotorTorque; // Apply forward torque

                }
                // If currentSpeed is negative (moving backward), pressing W should brake until 0, then accelerate forward
                if (currentSpeed < 0)
                {
                    brakeTorque = brakePower * Mathf.Abs(CurrentThrottleInput); // Treat W as brake if moving backward
                    motorTorque = 0; // No motor torque against the direction
                }
            }
            else if (CurrentThrottleInput < 0f) // 'S' is pressed (or negative input)
            {
                if (currentSpeed > -maxReverseSpeed) // Assuming you have maxReverseSpeed defined
                {
                    motorTorque = CurrentThrottleInput * horsePower; // Apply reverse torque (will be negative)
                }
                // If currentSpeed is positive (moving forward), pressing S should brake until 0, then accelerate backward
                if (currentSpeed > 0)
                {
                    brakeTorque = brakePower * Mathf.Abs(CurrentThrottleInput); // Treat S as brake if moving forward
                    motorTorque = 0; // No motor torque against the direction
                }
            }

            // Apply motor torque
            if (driveType == DriveTypes.RWD)
            {
                rearLeftWheelCollider.motorTorque = motorTorque;
                rearRightWheelCollider.motorTorque = motorTorque;
            }
            else if (driveType == DriveTypes.FWD)
            {
                frontLeftWheelCollider.motorTorque = motorTorque;
                frontRightWheelCollider.motorTorque = motorTorque;
            }
            else if (driveType == DriveTypes.AWD)
            {
                frontLeftWheelCollider.motorTorque = motorTorque;
                frontRightWheelCollider.motorTorque = motorTorque;
                rearLeftWheelCollider.motorTorque = motorTorque;
                rearRightWheelCollider.motorTorque = motorTorque;
            }

            // Apply brake torque to all wheels for simplicity in this example
            frontLeftWheelCollider.brakeTorque = brakeTorque;
            frontRightWheelCollider.brakeTorque = brakeTorque;
            rearLeftWheelCollider.brakeTorque = brakeTorque;
            rearRightWheelCollider.brakeTorque = brakeTorque;


            //Debug.Log($"ThrottleInput: {CurrentThrottleInput}, currentSpeed: {currentSpeed}, motorTorque: {motorTorque}");
            //Debug.Log($"CurrentSpeed: {currentSpeed}, SpeedFactor: {speedFactor}, MotorTorque: {currentMotorTorque}");
            //Debug.Log($"FL RPM: {FrontLeftWheelRPM}, RL RPM: {RearLeftWheelRPM}, motorTorque: {rearLeftWheelCollider.motorTorque}");
            //Debug.Log($"Throttle: {CurrentThrottleInput}, Accel: {currentAccelerationValue}, Rev: {currentReverseValue}");



            UpdateAccelerationSlider();
        }
    }

    void OnHandbrake(InputValue handbrakeValue)
    {
        CurrentHandbrakeValue = handbrakeValue.Get<float>();

        onHandBrake?.Invoke(CurrentHandbrakeValue);

        if (isStarted)
        {
            if (CurrentHandbrakeValue > 0)
            {
                if (SimplifiedWheelFrictionController != null)
                {
                    SimplifiedWheelFrictionController.StartDrifting(CurrentHandbrakeValue);
                }

                if (ezerealLightController != null)
                {
                    ezerealLightController.HandbrakeLightOn();
                }
            }
            else
            {
                if (SimplifiedWheelFrictionController != null)
                {
                    SimplifiedWheelFrictionController.StopDrifting();
                }

                if (ezerealLightController != null)
                {
                    ezerealLightController.HandbrakeLightOff();
                }
            }
        }
    }

    void Handbraking()
    {
        if (CurrentHandbrakeValue > 0f)
        {
            // Ensure motor torque is also zeroed when handbraking, even without bypassingInputs
            // This prevents trying to accelerate and handbrake at the same time.
            frontLeftWheelCollider.motorTorque = 0;
            frontRightWheelCollider.motorTorque = 0;
            rearLeftWheelCollider.motorTorque = 0;
            rearRightWheelCollider.motorTorque = 0;

            rearLeftWheelCollider.brakeTorque = CurrentHandbrakeValue * handbrakeForce;
            rearRightWheelCollider.brakeTorque = CurrentHandbrakeValue * handbrakeForce;


        }
        else
        {
            rearLeftWheelCollider.brakeTorque = 0;
            rearRightWheelCollider.brakeTorque = 0;
        }
    }

    void OnSteer(InputValue turnValue)
    {
        if (bypassingInputs)
        {
            TargetSteerAngle = 0; // <--- Ensure this is explicitly set to zero
            return; // Ignore input if bypassing
        }

        TargetSteerAngle = turnValue.Get<float>() * maxSteerAngle;

        onSteer?.Invoke(TargetSteerAngle);
    }

    void Steering()
    {
        float adjustedspeedFactor = Mathf.InverseLerp(20, maxForwardSpeed, currentSpeed); //minimum speed affecting steerAngle is 20
        float adjustedTurnAngle = TargetSteerAngle * (1 - adjustedspeedFactor); //based on current speed.
        CurrentSteerAngle = Mathf.Lerp(CurrentSteerAngle, adjustedTurnAngle, Time.deltaTime * steeringSpeed);


        ezerealLightController.AutoDisableTurnLight(CurrentSteerAngle); // Disable turn lights if steering angle is zero

        frontLeftWheelCollider.steerAngle = CurrentSteerAngle;
        frontRightWheelCollider.steerAngle = CurrentSteerAngle;

        UpdateWheel(frontLeftWheelCollider, frontLeftWheelMesh);
        UpdateWheel(frontRightWheelCollider, frontRightWheelMesh);
        UpdateWheel(rearLeftWheelCollider, rearLeftWheelMesh);
        UpdateWheel(rearRightWheelCollider, rearRightWheelMesh);
    }

    void Slowdown()
    {
        if (vehicleRB != null)
        {
            if (currentAccelerationValue == 0 && currentReverseValue == 0 && CurrentHandbrakeValue == 0)
            {
#if UNITY_6000_0_OR_NEWER
                vehicleRB.linearVelocity = Vector3.Lerp(vehicleRB.linearVelocity, Vector3.zero, Time.deltaTime * decelerationSpeed);
#else
                    vehicleRB.velocity = Vector3.Lerp(vehicleRB.velocity, Vector3.zero, Time.deltaTime * decelerationSpeed);
#endif
            }
        }
    }

    private void FixedUpdate()
    {
        if (vehicleRB != null) // Unity uses m/s as for default. So I convert from m/s to km/h. For mph use 2.23694f instead of 3.6f.
        {
#if UNITY_6000_0_OR_NEWER
            currentSpeed = Vector3.Dot(vehicleRB.gameObject.transform.forward, vehicleRB.linearVelocity);
#else
                currentSpeed = Vector3.Dot(vehicleRB.gameObject.transform.forward, vehicleRB.velocity);
                
#endif
            currentSpeed *= 3.6f;
            UpdateSpeedText(currentSpeed);

        }

        ApplyMotorAndBrakeTorque(); // Accelerate and decelerate/reverse

        Handbraking();

        Steering();

        Slowdown();

        RotateSteeringWheel();

        if
            (
                Mathf.Abs(frontLeftWheelCollider.rpm) < stopThreshold &&
                Mathf.Abs(frontRightWheelCollider.rpm) < stopThreshold &&
                Mathf.Abs(rearLeftWheelCollider.rpm) < stopThreshold &&
                Mathf.Abs(rearRightWheelCollider.rpm) < stopThreshold
            )
        {
            stationary = true;
        }
        else
        {
            stationary = false;
        }

        FrontLeftWheelRPM = frontLeftWheelCollider.rpm;
        FrontRightWheelRPM = frontRightWheelCollider.rpm;
        RearLeftWheelRPM = rearLeftWheelCollider.rpm;
        RearRightWheelRPM = rearRightWheelCollider.rpm;
    }

    private void UpdateWheel(WheelCollider col, Transform mesh)
    {
        col.GetWorldPose(out Vector3 position, out Quaternion rotation);
        mesh.SetPositionAndRotation(position, rotation);
    }

    // New public helper method to update all wheel meshes
    public void UpdateWheelMeshes()
    {
        UpdateWheel(frontLeftWheelCollider, frontLeftWheelMesh);
        UpdateWheel(frontRightWheelCollider, frontRightWheelMesh);
        UpdateWheel(rearLeftWheelCollider, rearLeftWheelMesh);
        UpdateWheel(rearRightWheelCollider, rearRightWheelMesh);
    }


    void RotateSteeringWheel()
    {
        float currentXAngle = steeringWheel.transform.localEulerAngles.x; // Maximum steer angle in degrees

        // Calculate the rotation based on the steer angle
        float normalizedSteerAngle = Mathf.Clamp(frontLeftWheelCollider.steerAngle, -maxSteerAngle, maxSteerAngle);
        float rotation = Mathf.Lerp(maxSteeringWheelRotation, -maxSteeringWheelRotation, (normalizedSteerAngle + maxSteerAngle) / (2 * maxSteerAngle));

        // Set the local rotation of the steering wheel
        steeringWheel.localRotation = Quaternion.Euler(currentXAngle, 0, rotation);
    }

    public void UpdateSpeedText(float speed)
    {
        speed = Mathf.Abs(speed);

        currentSpeedTMP_UI.text = speed.ToString("F0");
        currentSpeedTMP_Dashboard.text = speed.ToString("F0");
    }

    void UpdateAccelerationSlider()
    {
        float sliderValue = currentAccelerationValue > 0 ? currentAccelerationValue : currentReverseValue;
        accelerationSlider.value = Mathf.Lerp(accelerationSlider.value, sliderValue, Time.deltaTime * 15f);
    }

    public bool InAir()
    {
        foreach (WheelCollider wheel in wheels)
        {
            if (wheel.GetGroundHit(out _))
            {
                return false;
            }
        }
        return true;
    }






    /// <summary>
    /// Sets the car's linear velocity to a specific speed and direction,
    /// overriding any previous motion. This is useful for tutorial scenarios
    /// where you want to reset the car's speed.
    /// </summary>
    /// <param name="desiredSpeedKmh">The desired absolute speed in kilometers per hour (km/h).</param>
    /// <param name="forward">If true, the car will move in its forward direction. If false, it will move in its backward direction.</param>
    public void SetCarSpeed(float desiredSpeedKmh, bool forward)
    {
        if (vehicleRB == null)
        {
            Debug.LogError("Vehicle Rigidbody is not assigned for SetCarSpeed! Cannot set velocity.");
            return;
        }

        if( Mathf.Abs(desiredSpeedKmh) < 0.1)
            desiredSpeedKmh = 0.1f * Mathf.Sign(desiredSpeedKmh); // Ensure a minimum speed to avoid zero velocity issues

        desiredSpeedKmh = Mathf.Clamp(desiredSpeedKmh, -maxReverseSpeed, maxForwardSpeed); // Clamp speed to valid range
        // Convert desired speed from km/h to meters per second (m/s) for Rigidbody.velocity
        float speedMs = desiredSpeedKmh / 3.6f;

        // Determine the direction vector based on the 'forward' parameter
        Vector3 direction = forward ? vehicleRB.transform.forward : -vehicleRB.transform.forward;

        // Apply the velocity to the Rigidbody
#if UNITY_6000_0_OR_NEWER
        vehicleRB.linearVelocity = direction * speedMs;
#else
        vehicleRB.velocity = direction * speedMs;
#endif
        vehicleRB.angularVelocity = Vector3.zero; // Also clear angular velocity


        //// --- Temporarily disable all Wheel Colliders ---
        //foreach (var wheel in wheels)
        //{
        //    wheel.enabled = false;
        //}


        foreach (var wheel in wheels)
        {
            wheel.motorTorque = 0;

            // NEW: blocca il movimento delle ruote con freno anche se la velocità non è zero
            wheel.brakeTorque = 0;
            wheel.rotationSpeed = wheel.radius * desiredSpeedKmh / 3.6f; // Set rotation speed based on desired speed
        }


        // Reset current input states to prevent the car from immediately accelerating/braking
        // based on previous user input after resuming.
        CurrentThrottleInput = 0f;
        CurrentHandbrakeValue = 0f;
        TargetSteerAngle = 0f; // Reset steering target
        CurrentSteerAngle = 0f; // Reset current steering angle to neutral

        // Update the 'isStarted' state based on whether a non-zero speed is desired.
        // If speed is set to 0, consider the car "not started" in terms of continuous movement.
        isStarted = (desiredSpeedKmh > 0.01f); // Use a small threshold to account for floating point inaccuracies


        // If the desired speed is very close to zero, ensure the car is truly stopped
        if (desiredSpeedKmh <= 0.1f) // Small threshold for "zero speed"
        {

//            vehicleRB.angularVelocity = Vector3.zero; // Stop any rotational movement
//#if UNITY_6000_0_OR_NEWER
//            vehicleRB.linearVelocity = Vector3.zero; // Explicitly set linear velocity to zero
//#else
//            vehicleRB.velocity = Vector3.zero; // Explicitly set linear velocity to zero
//#endif

//            // Apply brake torque to fully stop wheels if desired speed is zero
//            foreach (var wheel in wheels)
//            {
//                wheel.brakeTorque = brakePower; // Apply full brake to ensure stop
//                wheel.rotationSpeed = 0f;
//            }

            // Optionally, turn off lights and engine sounds if the car is fully stopped
            if (ezerealLightController != null) ezerealLightController.AllLightsOff();
            if (simplifiedSoundController != null) simplifiedSoundController.TurnOffEngineSound();
        }
        else // If setting a positive speed, ensure car sounds and lights are on
        {
            if (ezerealLightController != null) ezerealLightController.MiscLightsOn();
            if (simplifiedSoundController != null) simplifiedSoundController.TurnOnEngineSound();
        }

        //// --- Re-enable all Wheel Colliders ---
        //foreach (var wheel in wheels)
        //{
        //    wheel.enabled = true;
        //}


        // Update the internal 'currentSpeed' variable and UI immediately for consistency.
        currentSpeed = desiredSpeedKmh;
        UpdateSpeedText(currentSpeed);

        vehicleRB.WakeUp(); // Ensure Rigidbody is awake for a moment

        StartCoroutine(BypassInputs(bypassInputsTime)); // Bypass inputs for a short time to prevent immediate user input interference
                                                      
    }

    public void TeleportCar(Vector3 targetPosition, Quaternion targetRotation, float desiredSpeed, bool forward)
    {
        // Create a temporary transform to hold the target position and rotation
        Transform targetTransform = new GameObject("TempTarget").transform;
        targetTransform.SetPositionAndRotation(targetPosition, targetRotation);
        // Call the overloaded TeleportCar method with the temporary transform
        TeleportCar(targetTransform, desiredSpeed, forward);
        // Clean up the temporary transform
        Destroy(targetTransform.gameObject);
    }

    /// <summary>
    /// Teleports the car's Rigidbody to a new position and rotation,
    /// stopping all current velocity to ensure a clean placement.
    /// This is useful for resetting the car's position in tutorials or when exiting bounded areas.
    /// </summary>
    /// <param name="targetTransform">The Transform representing the desired position and rotation for the car.</param>
    /// <param name="desiredSpeed">The desired speed in km/h for the car *after* teleporting. Use 0 for a complete stop.</param>
    /// <param name="forward">If true, the desired speed is applied in the car's forward direction; otherwise, backward.</param>
    public void TeleportCar(Transform targetTransform, float desiredSpeed, bool forward)
    {
        if (vehicleRB == null)
        {
            Debug.LogError("Vehicle Rigidbody is not assigned for TeleportCar! Cannot teleport.");
            return;
        }

        if (targetTransform == null)
        {
            Debug.LogError("Target Transform for TeleportCar is null! Cannot teleport.");
            return;
        }
        // Store original kinematic state
        bool originalIsKinematic = vehicleRB.isKinematic;

        // Temporarily make the Rigidbody kinematic for a clean teleport
        vehicleRB.isKinematic = true;

        // Instantly move the Rigidbody to the new position and rotation
        vehicleRB.position = targetTransform.position;
        vehicleRB.rotation = targetTransform.rotation;

        // Restore Rigidbody's kinematic state
        vehicleRB.isKinematic = originalIsKinematic;

        // Stop all motion before teleporting to prevent physics glitches
#if UNITY_6000_0_OR_NEWER
        vehicleRB.linearVelocity = Vector3.zero;
#else
        vehicleRB.velocity = Vector3.zero;
#endif
        vehicleRB.angularVelocity = Vector3.zero;
        // Force the Rigidbody to sleep.
        vehicleRB.Sleep();
        // ---------------------------------------------------------------------

        // Also update the meshes to match the new position immediately
        UpdateWheelMeshes(); // Use the new helper method

        // Reset wheel torques and brakes to ensure a clean state after teleport
        frontLeftWheelCollider.motorTorque = 0;
        frontRightWheelCollider.motorTorque = 0;
        rearLeftWheelCollider.motorTorque = 0;
        rearRightWheelCollider.motorTorque = 0;

        frontLeftWheelCollider.brakeTorque = 0;
        frontRightWheelCollider.brakeTorque = 0;
        rearLeftWheelCollider.brakeTorque = 0;
        rearRightWheelCollider.brakeTorque = 0;

        // Reset current input states to prevent the car from immediately accelerating/braking
        // based on previous user input after resuming.
        CurrentThrottleInput = 0f;
        CurrentHandbrakeValue = 0f;
        TargetSteerAngle = 0f; // Reset steering target
        CurrentSteerAngle = 0f; // Reset current steering angle to neutral

        // Optionally, ensure the car is considered "stopped" and turn off lights/sounds after teleport
        isStarted = false;
        if (ezerealLightController != null) ezerealLightController.AllLightsOff();
        if (simplifiedSoundController != null) simplifiedSoundController.TurnOffEngineSound();

        // Update UI to reflect zero speed
        currentSpeed = 0f;
        UpdateSpeedText(currentSpeed);

        SetCarSpeed(desiredSpeed, forward); // Set the car speed after teleporting

    }

    IEnumerator BypassInputs(float delay)
    {
        bypassingInputs = true; // Set the flag to indicate inputs are being bypassed
        yield return new WaitForSecondsRealtime(delay);
        bypassingInputs = false; // Reset the flag after the delay
    }

}
