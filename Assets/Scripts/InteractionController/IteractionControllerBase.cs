using Ezereal;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System; // For coroutines

public abstract class InteractionControllerBase : MonoBehaviour
{
    // --- Common Serialized Fields ---
    [Header("General Interaction Settings")]
    [Tooltip("Whether the interaction is currently enabled. Prevents execution of interaction logic.")]
    [SerializeField] public bool isInteractionEnabled = false; // Controls overall activity

    [Tooltip("The car's root GameObject (e.g., 'Simplified Electric Truck - Ready').")]
    [SerializeField] protected GameObject mainCarObject; // Protected for derived classes

    [Tooltip("The speed to set the car when ending the interaction. If 0, car maintains speed.")]
    [SerializeField] protected float resumeCarSpeed = 0f;
    [Tooltip("Time to wait before resuming game after interaction ends (e.g., for teleport animation).")]
    [SerializeField] protected float resumeTimeDelay = 0.15f;

    [Tooltip("Event to simulate a button press after resuming from pause (e.g., for throttle).")]
    public UnityEvent<float> onResumePressedButton; // Public UnityEvent

    [Tooltip("Where to teleport the car if something is wrong (e.g., out of bounds).")]
    [SerializeField] protected Transform resetPos;

    [Tooltip("Reference to the UserGuideController in the scene.")]
    [SerializeField] protected UserGuideController userGuideController;

    [Tooltip("Collider that triggers the start of the interaction.")]
    [SerializeField] protected Collider enterCollider;
    [Tooltip("Collider that triggers the end of the interaction (e.g., leaving the area).")]
    [SerializeField] protected Collider exitCollider;

    [Tooltip("UserGuide to show when interaction starts.")]
    [SerializeField] protected UserGuideType startInteractionGuide;
    [Tooltip("UserGuide to show when car goes out of bounds.")]
    [SerializeField] protected UserGuideType outOfBoundsUserGuide;

    // --- Common Internal References ---
    protected SimplifiedCarController carController;
    protected CarStateParameters storedCarState; // Stores car's state before interaction
    protected EzerealCameraController cameraController;

    // --- NEW: Action for custom behavior when waiting for any input ---
    protected Action _onAnyInputReceivedAction; // Renamed for clarity: internal storage for the action

    protected bool _isWaitingForAnyInput = false; // Flag for waiting for user input to resume game

#if UNITY_EDITOR
    // Reset method for editor convenience (called when script is attached or Reset is clicked)
    protected virtual void Reset()
    {
        // Attempt to find common components in children or scene
        userGuideController = transform.GetComponentInChildren<UserGuideController>(); // Or GetComponentInChildren if it's a child
        if (userGuideController == null) Debug.LogWarning("UserGuideController not found in scene for " + name, this);

        if (transform.Find("resetPos") != null)
        {
            resetPos = transform.Find("resetPos");
        }
        else
        {
            Debug.LogWarning("resetPos Transform not found as child for " + name + ". Please create an empty GameObject named 'resetPos' as a child.", this);
        }

        // Car references should ideally be assigned manually or found more robustly
        if (mainCarObject != null)
        {
            carController = mainCarObject.GetComponentInChildren<SimplifiedCarController>();
            cameraController = mainCarObject.GetComponentInChildren<EzerealCameraController>();
        }
        else
        {
            Debug.LogWarning("mainCarObject is not assigned in " + name + ". CarController and CameraController cannot be found.", this);
        }
    }
#endif

    protected virtual void Awake()
    { 
        // Ensure essential references are assigned
        if (mainCarObject == null) { Debug.LogError($"InteractionControllerBase: 'mainCarObject' is not assigned on {name}.", this); enabled = false; return; }
        if (userGuideController == null) { Debug.LogError($"InteractionControllerBase: 'userGuideController' is not assigned on {name}.", this); enabled = false; return; }
        if (resetPos == null) { Debug.LogError($"InteractionControllerBase: 'resetPos' is not assigned on {name}.", this); enabled = false; return; }

        carController = mainCarObject.GetComponentInChildren<SimplifiedCarController>();
        cameraController = mainCarObject.GetComponentInChildren<EzerealCameraController>();

        if (carController == null) { Debug.LogError($"InteractionControllerBase: SimplifiedCarController not found on 'mainCarObject' or its children for {name}.", this); enabled = false; return; }
        if (cameraController == null) { Debug.LogError($"InteractionControllerBase: EzerealCameraController not found on 'mainCarObject' or its children for {name}.", this); enabled = false; return; }
    }

    protected virtual void Update()
    {
        if (!isInteractionEnabled) return;

        // Logic for waiting for any input to resume game (common to both)
        if (_isWaitingForAnyInput)
        {
            if (Input.anyKeyDown)
            {
                _onAnyInputReceivedAction?.Invoke();
                _isWaitingForAnyInput = false; // Stop waiting after input received
                _onAnyInputReceivedAction = null; // Clear the action to prevent multiple calls
                return;
            }
        }
    }

    /// <summary>
    /// Starts the interaction, typically pausing the game and showing an initial guide.
    /// </summary>
    /// <param name="stopGame">If true, pauses the game.</param>
    public virtual void StartInteraction()
    {
        _isWaitingForAnyInput = false; // Reset this flag at the start of any interaction
        isInteractionEnabled = true;
        Debug.Log($"Interaction '{name}' started.");

        GameManager.Instance.PauseGame();

        cameraController.ResetCurrentCameraRotation(); // Reset camera
        userGuideController.SetUserGuide(startInteractionGuide); // Show initial guide

        // Store car state at the beginning of the interaction
        storedCarState = new CarStateParameters(carController);
    }

    /// <summary>
    /// Ends the interaction, typically resuming the game and hiding guides.
    /// </summary>
    /// <param name="resumeGame">If true, resumes the game.</param>
    public virtual void EndInteraction()
    {
        _isWaitingForAnyInput = false;
        isInteractionEnabled = false;
        Debug.Log($"Interaction '{name}' ended.");

        userGuideController.EnableUserGuides(false); // Hide all user guides

        GameManager.Instance.ResumeGame();

        // Disable colliders - specific implementation might be overridden
        if (exitCollider != null) exitCollider.enabled = false;
        if (enterCollider != null) enterCollider.enabled = false;
    }


    /// <summary>
    /// Restarts the interaction, usually after an "out of bounds" or "car hitted" event.
    /// This method will pause the game and show a specific user guide, then wait for input.
    /// </summary>
    /// <param name="guideTypeToShow">The specific UserGuideType to display for this restart.</param>
    /// <param name="customInputReceivedAction">Optional action to execute when input is received during this wait.</param>

    public virtual void RestartInteraction(UserGuideType guideTypeToShow, Action customInputReceivedAction = null)
    {
        // Disable enter collider temporarily to prevent immediate re-trigger
        if (enterCollider != null) enterCollider.enabled = false;

        GameManager.Instance.PauseGame(); // Pause the game


        userGuideController.SetUserGuide(guideTypeToShow); // Show the specific guide for this restart reason
        cameraController.ResetCurrentCameraRotation(); // Reset camera rotation

        isInteractionEnabled = true; // Re-enable interaction
        StartWaitingForAnyInput(customInputReceivedAction); // Start waiting with the provided action
    }

    /// <summary>
    /// Helper method to restore car state and resume the game after a wait.
    /// This is the default behavior when any input is received during a wait.
    /// </summary>
    protected void ResumeGameAfterWait(UserGuideType userGuideType = UserGuideType.None)
    {
        UserGuideType guideType = userGuideType == UserGuideType.None ? startInteractionGuide : userGuideType; // Default to startInteractionGuide if none provided
        userGuideController.SetUserGuide(guideType); // Set to the initial guide for the interaction
        GameManager.Instance.ResumeGame();
        if (storedCarState != null && resumeCarSpeed == 0)
        {

            storedCarState.ApplyToCarController(carController);
            Debug.Log("Car state restored for RestartInteraction.");
        }
        else
        {
            // Fallback if state wasn't stored (e.g., direct call to RestartInteraction without StartInteraction)
            // Teleport to resetPos with resumeCarSpeed (or 0 if resumeCarSpeed is 0)
            float velocity = resumeCarSpeed == 0 ? 1 : resumeCarSpeed;
            carController.TeleportCar(resetPos, velocity, true);
            Debug.LogWarning("No stored car state found for RestartInteraction. Using resetPos and configured resumeCarSpeed as fallback.");
        }
        StartCoroutine(GameManager.Instance.WaitToPause(resumeTimeDelay)); // Wait a bit before pausing again if needed
    }

    /// <summary>
    /// Starts waiting for any input from the user.
    /// Derived classes can provide a custom action to be executed when input is received.
    /// If no action is provided, `ResumeGameAfterWait()` is used as default.
    /// </summary>
    /// <param name="customAction">Optional action to execute when input is received.</param>
    protected void StartWaitingForAnyInput(Action customAction = null)
    {
        _isWaitingForAnyInput = true;
        // Assign the custom action, or set the default action if null
        _onAnyInputReceivedAction = customAction ?? ( () => ResumeGameAfterWait() ); // Cast needed for method group
        Debug.Log($"Interaction '{name}' is now waiting for any input.");
    }

    /// <summary>
    /// Stops waiting for any input.
    /// </summary>
    protected void StopWaitingForAnyInput()
    {
        _isWaitingForAnyInput = false;
        _onAnyInputReceivedAction = null; // Clear the action
        Debug.Log($"Interaction '{name}' stopped waiting for input.");
    }
}