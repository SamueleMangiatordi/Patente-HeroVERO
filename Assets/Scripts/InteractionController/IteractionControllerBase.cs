using Ezereal;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;
using UnityEngine.UI; // For coroutines

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

    [Tooltip("Time to ignore AI collisions after resuming. This is useful to prevent immediate collisions after teleporting the car, entering in an infinite loop.")]
    [SerializeField] protected float aiCarIgnoreCollisionTime = 6f; // Time to ignore AI collisions after resuming

    [Tooltip("Time to wait before start considering input as valid. It is used to avoid random input")]
    [SerializeField] protected float _waitingInputTreshold = 1f;

    [Tooltip("Event to simulate a button press after resuming from pause (e.g., for throttle).")]
    public UnityEvent<float> onResumePressedButton; // Public UnityEvent

    [Tooltip("Where to teleport the car if something is wrong (e.g., out of bounds).")]
    [SerializeField] public Transform resetPos;
    [Tooltip("Collider that triggers the start of the interaction.")]
    [SerializeField] protected Collider enterCollider;
    [Tooltip("Collider that triggers the end of the interaction (e.g., leaving the area).")]
    [SerializeField] protected Collider exitCollider;

    [Header("User Guide Settings")]
    [Tooltip("Reference to the UserGuideController in the scene.")]
    [SerializeField] protected UserGuideController userGuideController;
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

    protected float resumeTimeDelay = 0.15f; //Time to wait before resuming game after interaction ends (e.g., for teleport animation). It is used to allow the movement of the car, which cannot happen if timescale is 0

    protected bool _isWaitingForAnyInput = false; // Flag for waiting for user input to resume game
    private float _waitingInputTimer = 0f; // Timer to track waiting time for input

    private Transform _resetPos = null;

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

    private void Start()
    {
        GameObject temp = new GameObject(name + "_ResetPos");
        temp.transform.position = resetPos.position;
        temp.transform.rotation = resetPos.rotation;

        _resetPos = temp.transform;
    }

    protected virtual void Update()
    {
        if (!isInteractionEnabled) return;

        // Logic for waiting for any input to resume game (common to both)
        if (_isWaitingForAnyInput)
        {
            _waitingInputTimer += Time.unscaledDeltaTime;

            if (_waitingInputTimer > _waitingInputTreshold && Input.anyKeyDown)
            {
                _waitingInputTimer = 0f; // Reset the timer after receiving input

                _onAnyInputReceivedAction?.Invoke();
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

        // Store car state at the beginning of the interaction
        storedCarState = new CarStateParameters(carController);
    }

    /// <summary>
    /// Stops the interaction, typically pausing the game and showing an initial guide.
    /// </summary>
    public virtual void PauseGameAndShowUserGuide()
    {
        GameManager.Instance.PauseGame();

        cameraController.ResetCurrentCameraRotation(); // Reset camera
        userGuideController.SetUserGuide(startInteractionGuide); // Show initial guide

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
    /// When the iinput is recieved, it will call the action provided (if any) or resume the game with the default behavior.
    /// </summary>
    /// <param name="guideTypeToShow">The specific UserGuideType to display for this restart.</param>
    /// <param name="onInputReceived">Optional action to execute when input is received during this wait.</param>

    public virtual void RestartInteraction(UserGuideType guideTypeToShow, Action onInputReceived = null)
    {
        // Disable enter collider temporarily to prevent immediate re-trigger
        if (enterCollider != null) enterCollider.enabled = false;

        GameManager.Instance.PauseGame(); // Pause the game


        userGuideController.SetUserGuide(guideTypeToShow); // Show the specific guide for this restart reason
        cameraController.ResetCurrentCameraRotation(); // Reset camera rotation

        isInteractionEnabled = true; // Re-enable interaction
        StartWaitingForAnyInput(onInputReceived); // Start waiting with the provided action
    }

    /// <summary>
    /// Helper method to restore car state and resume the game after a wait and show the user guide provided (default one if no one is provided).
    /// This is the default behavior when any input is received during a wait.
    /// </summary>
    protected void ResumeGameAfterWait(UserGuideType userGuideType = UserGuideType.None, bool useStoredCarState = true, bool showUserGuide = true)
    {
        UserGuideType guideType = userGuideType == UserGuideType.None ? startInteractionGuide : userGuideType; // Default to startInteractionGuide if none provided
        if(showUserGuide)
            userGuideController.SetUserGuide(guideType); // Set to the initial guide for the interaction
        
        GameManager.Instance.ResumeGame();
        if (storedCarState != null && resumeCarSpeed == 0 && useStoredCarState)
        {

            storedCarState.ApplyToCarController(carController);
            AiCarSpawner.IgnoreAllAiPlayerCollision(aiCarIgnoreCollisionTime); // Ignore AI collisions for a short time after resuming

            Debug.Log("Car state restored for RestartInteraction.");
        }
        else
        {
            Debug.Log("Reset pos name = " + resetPos.name);
            carController.TeleportCar(resetPos, resumeCarSpeed, true);
            Debug.LogWarning("No stored car state found for RestartInteraction. Using resetPos and configured resumeCarSpeed as fallback.");
        }
        StartCoroutine(GameManager.Instance.WaitToPause(resumeTimeDelay)); // Wait a bit before pausing again if needed

        StartCoroutine(ResetPosTransform()); 
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
        _onAnyInputReceivedAction = customAction ?? (() => ResumeGameAfterWait()); // Cast needed for method group
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

    IEnumerator ResetPosTransform()
    {
        yield return new WaitForSeconds(3f);
        resetPos = _resetPos;
    }
}