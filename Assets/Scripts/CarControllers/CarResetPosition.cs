using System.Collections;
using UnityEngine;

public class CarResetPos : MonoBehaviour
{
    [SerializeField] private SimplifiedCarController carController; // Reference to the car controller
    [SerializeField] private UserGuideController userGuideController; // Reference to the UserGuideController

    [SerializeField] private float velocityAfterResetPos = 5; // Delay before resetting position
    [SerializeField] private float resumeGameDelayAfterResetPos = 0.15f; // Duration of the position reset animation

    [SerializeField] private float savePosInterval = 1f; // Interval to save the car's position in seconds
    [SerializeField] private int historyDuration = 10; // Duration in seconds to keep history (e.g., 10 seconds)

    private CarStateParameters[] carStateHistory; // Array to store historical states
    private int _headIndex = 0; // The index of the most recently saved state
    private int _savedStatesCount = 0; // Number of states currently saved in the history
    private float _timeSinceLastSave = 0f; // Timer to track when to save the position

    // Calculate history size based on duration and interval
    private int HistorySize => Mathf.CeilToInt(historyDuration / savePosInterval) + 1; // +1 to include current state or buffer margin

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
                waitingForAnyInput = false; // Reset the flag

                return; // Consume this input for the guide, do not proceed with other Update logic if any
            }

        }
    }

    void Awake()
    {
        // Initialize the history array based on calculated size
        carStateHistory = new CarStateParameters[HistorySize];
    }

    private void FixedUpdate()
    {
        if (carController == null)
        {
            Debug.LogWarning("CarResetPos: Missing CarController reference. Cannot save position.", this);
            return;
        }
        if (carController.vehicleRB == null)
        {
            Debug.LogWarning("CarResetPos: CarController's Rigidbody is null. Cannot save position.", this);
            return;
        }

        _timeSinceLastSave += Time.fixedDeltaTime; // Increment the timer

        if (_timeSinceLastSave >= savePosInterval)
        {
            _timeSinceLastSave = 0f; // Reset the timer

            if (_headIndex != 0 && Vector3.Distance(carStateHistory[_headIndex].position, carController.vehicleRB.position ) < 1f)
                return; // Skip saving if the position hasn't changed significantly

            // Increment head index and wrap around the buffer size
            _headIndex = (_headIndex + 1) % HistorySize;

            // Create a new CarStateParameters object and store the current state
            carStateHistory[_headIndex] = new CarStateParameters(carController);

            // Increment count of saved states, capping at HistorySize
            if (_savedStatesCount < HistorySize)
            {
                _savedStatesCount++;
            }

            // Debug.Log($"Saved state at index: {_headIndex}. Total saved states: {_savedStatesCount}");
        }
    }

    /// <summary>
    /// Resets the car's position and state to a point in time 'secondsAgo'.
    /// </summary>
    /// <param name="secondsAgo">The number of seconds into the past to reset to.
    /// Clamped between 0 and historyDuration.</param>
    public void ResetCarPosition(float secondsAgo)
    {

        if (carController == null || carController.vehicleRB == null)
        {
            Debug.LogWarning("CarResetPos: CarController or its Rigidbody is not assigned. Cannot reset position.", this);
            return;
        }

        if (_savedStatesCount == 0)
        {
            Debug.LogWarning("CarResetPos: No history available to reset to. Move the car first.", this);
            return;
        }

        GameManager.Instance.ResumeGame(); // Resume the game before resetting position

        // Clamp secondsAgo to ensure it's within the valid history range
        secondsAgo = Mathf.Clamp(secondsAgo, 0f, historyDuration);

        // Calculate how many intervals ago this state would be
        int intervalsAgo = Mathf.RoundToInt(secondsAgo / savePosInterval);

        // Ensure we don't try to access a state that hasn't been saved yet
        intervalsAgo = Mathf.Min(intervalsAgo, _savedStatesCount - 1); // Clamp to actual saved count

        // Calculate the target index in the circular buffer
        int targetIndex = (_headIndex - intervalsAgo + HistorySize) % HistorySize;

        // Debug.Log($"Resetting {secondsAgo:F2} seconds ago. Intervals ago: {intervalsAgo}. Head index: {_headIndex}. Target index: {targetIndex}");

        // Retrieve the saved state
        CarStateParameters stateToRestore = carStateHistory[targetIndex];

        if (stateToRestore != null)
        {
            // Apply the saved state to the car controller
            stateToRestore.TeleportCarToSavedPos(carController, velocityAfterResetPos, velocityAfterResetPos > 0);
            Debug.Log($"Car reset to state from {secondsAgo:F2} seconds ago. Position: {stateToRestore.position}");
        }
        else
        {
            // This should ideally not happen if _savedStatesCount is managed correctly
            Debug.LogWarning($"CarResetPos: Attempted to restore a null state at index {targetIndex}. This might indicate an issue with history tracking.", this);
        }

        float waitInputDelay = Mathf.Max(0.05f, resumeGameDelayAfterResetPos - 0.05f); // Ensure we wait a minimum time before allowing input
        StartCoroutine(WaitForInput(waitInputDelay)); // Start waiting for user input after resetting position
        StartCoroutine(GameManager.Instance.WaitToPause(resumeGameDelayAfterResetPos)); // Pause the game to allow user interaction
    }

    /// <summary>
    /// For debugging/testing: Resets the car to the most recent saved position.
    /// </summary>
    public void ResetToLastPosition()
    {
        ResetCarPosition(0f); // Reset to 0 seconds ago (most recent state)
    }

    /// <summary>
    /// For debugging/testing: Resets the car to a position a specified number of seconds ago.
    /// </summary>
    /// <param name="seconds">The seconds into the past.</param>
    [ContextMenu("Reset 5 Seconds Ago")] // Adds an option to the component's context menu in Unity editor
    public void Reset5SecondsAgo()
    {
        ResetCarPosition(5f);
    }

    IEnumerator WaitForInput(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        waitingForAnyInput = true; // Set the flag to indicate we are waiting for input
        userGuideController.SetUserGuide(UserGuideType.CarResetPosition); // Set the user guide to the car reset position type
    }
}