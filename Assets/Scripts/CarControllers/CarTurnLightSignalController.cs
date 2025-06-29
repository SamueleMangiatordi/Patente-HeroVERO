using Ezereal;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CarTurnLightSignalController : MonoBehaviour
{
    public bool isTurnLightSignalCheckEnabled = true; // Enable or disable the turn signal check

    public int errorsAllowd = 3; // Number of allowed errors before triggering a warning
    public int ErrorsCount { get; private set; } = 0; // Current error count

    [Tooltip("Event triggered when the car is turning without the appropriate turn signal active.")]
    public UnityEvent onError;
    [Tooltip("Event triggered when the car is turning without the appropriate turn signal active, and the error count exceeds the allowed limit.")]
    public UnityEvent onErrorExceeded; // Event triggered when error count exceeds allowed limit

    [SerializeField] private SimplifiedCarController carController; // Reference to the car controller script
    [SerializeField] private EzerealLightController ezerealLightController; // Assign this in the Inspector
    [SerializeField] private CarResetPos carResetPos; // Reference to the CarResetPos script for resetting car state
    [SerializeField] private UserGuideController userGuideController; // Reference to the UserGuideController

    [Tooltip("Time in seconds where to reset the car position")]
    [SerializeField] private float carResetPosHistoryTime = 10f; // Time in seconds to keep the car reset position history
    [Tooltip("Interval before checking again after an error. Set to 0 to disable.")]
    [SerializeField] private float intervalBeforeCheckAgain = 10f; // Interval before checking again after an error
    // --- NEW: Turn Signal Check Settings (moved from SimplifiedCarController) ---
    [Header("Turn Signal Check")]
    [Tooltip("The steering angle (degrees) beyond which the car is considered to be turning.")]
    [SerializeField]
    private float turnAngleThreshold = 10f; // Adjust as needed
    [Tooltip("Delay in seconds before checking if a turn signal is active after exceeding the threshold.")]
    [SerializeField]
    private float turnSignalCheckDelay = 0.5f;
    [Tooltip("How long the warning persists if light is not active.")]
    [SerializeField]
    private float warningDuration = 5f;


    private float _turnSignalCheckTimer = 0f;
    private bool _isCurrentlyTurningLeft = false;
    private bool _isCurrentlyTurningRight = false;
    private bool _warningActive = false; // To prevent spamming warnings

    private bool waitingForAnyInput = false; // Flag to indicate if we are waiting for any input

    void Update()
    {
        if (waitingForAnyInput)
        {
            // Detects any keyboard key press or any mouse button click
            if (UnityEngine.Input.anyKeyDown)
            {
                Debug.Log("Any keyboard/mouse button detected while waiting, proceeding with normal guide.", this);

                GameManager.Instance.ResumeGame(); // Resume the game if any input is detected
                userGuideController.EnableUserGuides(false); // Enable user guides
                carResetPos.ResetCarPosition(carResetPosHistoryTime); // Reset car position if any input is detected
                ResetErrorCount();
                waitingForAnyInput = false; // Reset the flag

                isTurnLightSignalCheckEnabled = false; // Reset the timer to check again
                StartCoroutine(WaitToCheckAgainTurnLight(intervalBeforeCheckAgain)); // Wait a bit before checking again

                return; // Consume this input for the guide, do not proceed with other Update logic if any
            }

        }
    }

    void FixedUpdate()
    {
        if (!isTurnLightSignalCheckEnabled)
            return; // Skip the turn signal check if it's disabled

        // Ensure we have a carController and ezerealLightController before checking
        if (carController == null || ezerealLightController == null)
        {
            Debug.LogWarning("CarAdapter: Missing CarController or ezerealLightController reference. Cannot perform turn signal check.", this);
            return;
        }

        if (waitingForAnyInput)
            return;

        if (_warningActive)
            return;

        float currentSteer = carController.CurrentSteerAngle; // Get the actual applied steer angle from carController

        // Check if car is turning significantly
        if (currentSteer > turnAngleThreshold) // Turning Right
        {
            if (!_isCurrentlyTurningRight) // Just started turning right
            {
                _isCurrentlyTurningRight = true;
                _isCurrentlyTurningLeft = false; // Ensure left turning state is reset
                _turnSignalCheckTimer = turnSignalCheckDelay; // Start timer
                _warningActive = false; // Reset warning
            }

            // Check timer
            if (_isCurrentlyTurningRight && !_warningActive && _turnSignalCheckTimer <= 0f)
            {
                if (!ezerealLightController.IsRightTurnActive && !ezerealLightController.AreHazardLightsActive)
                {
                    Debug.LogWarning("WARNING: Turning Right without right turn signal activated!", this);
                    _warningActive = true; // Activate warning flag to prevent repeated warnings
                    StartCoroutine(ClearWarningAfterDelay(warningDuration));

                    ErrorsCount++; // Increment error count

                    if (ErrorsCount < errorsAllowd)
                    {
                        onError?.Invoke(); // Invoke the event if set
                        userGuideController.SetuserGuide(UserGuideType.TurnSignalError);
                        StartCoroutine(ClearErrorAfterDelay(warningDuration)); // Clear error after warning duration

                    }
                    else
                    {
                        userGuideController.SetuserGuide(UserGuideType.TurnSignalErrorExceeded);
                        GameManager.Instance.PauseGame();
                        onErrorExceeded?.Invoke(); // Invoke the event for exceeding error limit
                        waitingForAnyInput = true;
                    }
                    // Trigger your specific behavior here, e.g., play a sound, show UI
                    // Example: PlayWarningSound();
                    // Example: ShowWarningUI("Activate Right Turn Signal!");
                }
            }
        }
        else if (currentSteer < -turnAngleThreshold) // Turning Left
        {
            if (!_isCurrentlyTurningLeft) // Just started turning left
            {
                _isCurrentlyTurningLeft = true;
                _isCurrentlyTurningRight = false; // Ensure right turning state is reset
                _turnSignalCheckTimer = turnSignalCheckDelay; // Start timer
                _warningActive = false; // Reset warning
            }

            // Check timer
            if (_isCurrentlyTurningLeft && !_warningActive && _turnSignalCheckTimer <= 0f)
            {
                if (!ezerealLightController.IsLeftTurnActive && !ezerealLightController.AreHazardLightsActive)
                {
                    Debug.LogWarning("WARNING: Turning Left without left turn signal activated!", this);
                    // Trigger your specific behavior here
                    _warningActive = true; // Activate warning flag
                    StartCoroutine(ClearWarningAfterDelay(warningDuration));

                    ErrorsCount++; // Increment error count

                    if (ErrorsCount < errorsAllowd)
                    {
                        onError?.Invoke(); // Invoke the event if set
                        userGuideController.SetuserGuide(UserGuideType.TurnSignalError);
                        StartCoroutine(ClearErrorAfterDelay(warningDuration)); // Clear error after warning duration
                    }
                    else
                    {
                        userGuideController.SetuserGuide(UserGuideType.TurnSignalErrorExceeded);
                        onErrorExceeded?.Invoke(); // Invoke the event for exceeding error limit
                        GameManager.Instance.PauseGame();
                        waitingForAnyInput = true;
                    }
                }
            }
        }
        else // Steering is close to center (not turning significantly)
        {
            if (_isCurrentlyTurningLeft || _isCurrentlyTurningRight) // If we were just turning
            {
                _isCurrentlyTurningLeft = false;
                _isCurrentlyTurningRight = false;
                //_warningActive = false; // Clear any active warning if turning stops
                //StopCoroutine(nameof(ClearWarningAfterDelay)); // Stop any pending warning clear
            }
            _turnSignalCheckTimer = turnSignalCheckDelay; // Reset timer when not turning
        }

        // Update timer only when actively turning beyond threshold
        if ((_isCurrentlyTurningLeft || _isCurrentlyTurningRight) && _turnSignalCheckTimer > 0f)
        {
            _turnSignalCheckTimer -= Time.deltaTime;
        }
    }

    IEnumerator ClearWarningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _warningActive = false;
    }

    IEnumerator ClearErrorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        userGuideController.EnableUserGuides(false); // Disable user guide after warning    
    }

    IEnumerator WaitToCheckAgainTurnLight(float delay)
    {
        yield return new WaitForSeconds(delay);
        isTurnLightSignalCheckEnabled = true; // Reset the timer to check again
    }


    public void ResetErrorCount()
    {
        ErrorsCount = 0; // Reset the error count
        _warningActive = false; // Reset warning state
    }

    public void EnableTurnSignalCheck(bool enable)
    {
        isTurnLightSignalCheckEnabled = enable; // Enable or disable the turn signal check
        if (!enable)
        {
            ResetErrorCount(); // Reset error count when disabling
        }
    }

}
