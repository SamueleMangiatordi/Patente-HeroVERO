using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using Ezereal;
using UnityEngine.Events;

public class SimplifiedCarController : MonoBehaviour // This is the main system resposible for car control.
{
    [SerializeField] public UnityEvent<float> onThrottle;
    [SerializeField] public UnityEvent<float> onHandBrake;
    [SerializeField] public UnityEvent<float> onSteer;

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
    [SerializeField] float currentHandbrakeValue = 0f;
    [SerializeField] float currentSteerAngle = 0f;
    [SerializeField] float targetSteerAngle = 0f;
    [SerializeField] float FrontLeftWheelRPM = 0f;
    [SerializeField] float FrontRightWheelRPM = 0f;
    [SerializeField] float RearLeftWheelRPM = 0f;
    [SerializeField] float RearRightWheelRPM = 0f;

    [SerializeField] float speedFactor = 0f; // Leave at zero. Responsible for smooth acceleration and near-top-speed slowdown.

    private float currentThrottleInput = 0f; // Current throttle input value

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
        currentThrottleInput = throttleValue.Get<float>();
        
        onThrottle?.Invoke(currentThrottleInput);
        //Debug.Log("Acceleration: " + currentAccelerationValue.ToString());

        if (isStarted && ezerealLightController != null)
        {
            if (currentThrottleInput < 0)
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
            if (currentThrottleInput > 0f) // 'W' is pressed (or positive input)
            {
                if (currentSpeed < maxForwardSpeed)
                {
                    motorTorque = currentThrottleInput * currentMotorTorque; // Apply forward torque
                }
                // If currentSpeed is negative (moving backward), pressing W should brake until 0, then accelerate forward
                if (currentSpeed < 0)
                {
                    brakeTorque = brakePower * Mathf.Abs(currentThrottleInput); // Treat W as brake if moving backward
                    motorTorque = 0; // No motor torque against the direction
                }
            }
            else if (currentThrottleInput < 0f) // 'S' is pressed (or negative input)
            {
                if (currentSpeed > -maxReverseSpeed) // Assuming you have maxReverseSpeed defined
                {
                    motorTorque = currentThrottleInput * horsePower; // Apply reverse torque (will be negative)
                }
                // If currentSpeed is positive (moving forward), pressing S should brake until 0, then accelerate backward
                if (currentSpeed > 0)
                {
                    brakeTorque = brakePower * Mathf.Abs(currentThrottleInput); // Treat S as brake if moving forward
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

            UpdateAccelerationSlider();
        }
    }

    void OnHandbrake(InputValue handbrakeValue)
    {
        currentHandbrakeValue = handbrakeValue.Get<float>();

        onHandBrake?.Invoke(currentHandbrakeValue);

        if (isStarted)
        {
            if (currentHandbrakeValue > 0)
            {
                if (SimplifiedWheelFrictionController != null)
                {
                    SimplifiedWheelFrictionController.StartDrifting(currentHandbrakeValue);
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
        if (currentHandbrakeValue > 0f)
        {
            rearLeftWheelCollider.motorTorque = 0;
            rearRightWheelCollider.motorTorque = 0;
            rearLeftWheelCollider.brakeTorque = currentHandbrakeValue * handbrakeForce;
            rearRightWheelCollider.brakeTorque = currentHandbrakeValue * handbrakeForce;


        }
        else
        {
            rearLeftWheelCollider.brakeTorque = 0;
            rearRightWheelCollider.brakeTorque = 0;
        }
    }

    void OnSteer(InputValue turnValue)
    {
        targetSteerAngle = turnValue.Get<float>() * maxSteerAngle;

        onSteer?.Invoke(targetSteerAngle);
    }

    void Steering()
    {
        float adjustedspeedFactor = Mathf.InverseLerp(20, maxForwardSpeed, currentSpeed); //minimum speed affecting steerAngle is 20
        float adjustedTurnAngle = targetSteerAngle * (1 - adjustedspeedFactor); //based on current speed.
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, adjustedTurnAngle, Time.deltaTime * steeringSpeed);

        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;

        UpdateWheel(frontLeftWheelCollider, frontLeftWheelMesh);
        UpdateWheel(frontRightWheelCollider, frontRightWheelMesh);
        UpdateWheel(rearLeftWheelCollider, rearLeftWheelMesh);
        UpdateWheel(rearRightWheelCollider, rearRightWheelMesh);
    }

    void Slowdown()
    {
        if (vehicleRB != null)
        {
            if (currentAccelerationValue == 0 && currentReverseValue == 0 && currentHandbrakeValue == 0)
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

        if (vehicleRB != null) // Unity uses m/s as for default. So I convert from m/s to km/h. For mph use 2.23694f instead of 3.6f.
        {
#if UNITY_6000_0_OR_NEWER
            currentSpeed = Vector3.Dot(vehicleRB.gameObject.transform.forward, vehicleRB.linearVelocity);
            currentSpeed *= 3.6f;
            UpdateSpeedText(currentSpeed);
#else
                currentSpeed = Vector3.Dot(vehicleRB.gameObject.transform.forward, vehicleRB.velocity);
                currentSpeed *= 3.6f; 
                UpdateSpeedText(currentSpeed);
#endif

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


    void RotateSteeringWheel()
    {
        float currentXAngle = steeringWheel.transform.localEulerAngles.x; // Maximum steer angle in degrees

        // Calculate the rotation based on the steer angle
        float normalizedSteerAngle = Mathf.Clamp(frontLeftWheelCollider.steerAngle, -maxSteerAngle, maxSteerAngle);
        float rotation = Mathf.Lerp(maxSteeringWheelRotation, -maxSteeringWheelRotation, (normalizedSteerAngle + maxSteerAngle) / (2 * maxSteerAngle));

        // Set the local rotation of the steering wheel
        steeringWheel.localRotation = Quaternion.Euler(currentXAngle, 0, rotation);
    }

    void UpdateSpeedText(float speed)
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

}
