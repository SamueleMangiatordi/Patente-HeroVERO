using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // Make sure this namespace is present

/**
 * Classe per controllare il flusso di informazioni da fornire all'utente durante uno stato
 */
public class TriggerableUserGuide : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] public bool isActive = false;  //used to check if the user guide is currently active and not execute the update method if not

    [SerializeField] private float longPressTreshold = 0.5f;    //for how long the button must be pressed before considering it a valid press

    [Tooltip("1 per avanti/destra, -1 per indietro/sinistra. 0 per qualsiasi non-zero input (e.g., simple button).")]
    [SerializeField] private float validAxisDir;

    [Tooltip("The car prefab with all gameObjects (Simplified Electric Truck - Ready) , not only the electric truck prefab")]
    [SerializeField] private GameObject mainCarObject;
    [SerializeField] private float resumeCarSpeed = 0f; //the speed to set the car when starting the interaction, usually 0

    [Tooltip("Since when pausing the game with a button pressed and resuming it with the same button still pressed lose the input and the button is considered not pressed anymore, this event is used to simulate the user elevate the button and pressing it again")]
    public UnityEvent<float> onResumePressedButton; // UnityEvent to simulate the button press

    [SerializeField] private Transform resetPos;    //where to teleport the car if something is wrong

    [SerializeField] private UserGuide userGuide;   //the userGuid that prompt the user with instructions, placed inside the ameObject that has this script

    [SerializeField] private Collider enterCollider;
    [SerializeField] private Collider exitCollider; // Colliders to detect when the user enters or exits the interaction area

    [SerializeField] private Collider[] boundedAreaColliders; // Array of colliders defining the bounded area for interaction

    private SimplifiedCarController carController; // Reference to the car controller, if needed for further interactions

    private bool waitingForAnyInput = false;
    // NEW: InputAction to detect any button press regardless of timeScale
    private InputAction anyInputDetectionAction;

    private bool isPressed = false; //used for checking if the button is currently pressed
    private float timePressed = 0;  //used for checking how long the button is pressed



#if UNITY_EDITOR
    void Reset()
    {
        userGuide = transform.GetComponentInChildren<UserGuide>();
        resetPos = transform.Find("resetPos");

        carController = mainCarObject.GetComponentInChildren<SimplifiedCarController>();
    }
#endif
    public void Awake()
    {
        carController = mainCarObject.GetComponentInChildren<SimplifiedCarController>();

        if (resumeCarSpeed == 0f)
            resumeCarSpeed = 1f;

        // NEW: Initialize the anyInputDetectionAction
        anyInputDetectionAction = new InputAction("AnyInput", type: InputActionType.Button);

        anyInputDetectionAction.updateMode = InputActionUpdateMode.UnscaledGameTime;

        // Add bindings for common 'any' input sources
        anyInputDetectionAction.AddBinding("<Keyboard>/*"); // Any keyboard key
        anyInputDetectionAction.AddBinding("<Mouse>/leftButton"); // Left mouse click
        anyInputDetectionAction.AddBinding("<Mouse>/rightButton"); // Right mouse click
        anyInputDetectionAction.AddBinding("<Gamepad>/*Button"); // Any gamepad button (e.g., A, B, X, Y, shoulder buttons)
                                                                 // You can add more specific bindings like <Gamepad>/leftStick/x or <Gamepad>/rightTrigger if you want axis movement to count

        // Set the update mode to ignore Time.timeScale
        anyInputDetectionAction.ApplyBindingOverrides(new InputBinding { updateMode = InputActionUpdateMode.UnscaledGameTime });

        // Subscribe to the performed event
        anyInputDetectionAction.performed += OnAnyInputDetected;

        // Start disabled, only enable when we specifically want to detect "any input"
        anyInputDetectionAction.Disable();
    }

    void OnDestroy()
    {
        // NEW: Unsubscribe and dispose of the InputAction to prevent memory leaks
        if (anyInputDetectionAction != null)
        {
            anyInputDetectionAction.performed -= OnAnyInputDetected;
            anyInputDetectionAction.Dispose();
        }
    }

    // NEW: Callback for when any input is detected by anyInputDetectionAction
    private void OnAnyInputDetected(InputAction.CallbackContext context)
    {
        if (isActive && waitingForAnyInput)
        {
            Debug.Log("Any input detected via New Input System, proceeding with normal guide.");
            waitingForAnyInput = false; // Stop waiting
            userGuide.NextMessage();    // Display the *next* message (your first normal instruction)

            // Reset long press state to allow normal interaction to start fresh
            isPressed = false;
            timePressed = 0;

            // Disable the specific action once input is detected
            anyInputDetectionAction.Disable();
        }
    }

    public void Update()
    {
        if (!isActive)
            return;

        if (!waitingForAnyInput)
            return;

        // If the button is currently considered pressed (and in the correct direction)
        if (!isPressed)
            return;

        timePressed += Time.unscaledDeltaTime; // Accumulate time regardless of Time.timeScale

        if (timePressed >= longPressTreshold)
        {
            Debug.Log($"Long press detected after {timePressed:F2} seconds! Input direction: {validAxisDir}");
            isPressed = false; // Reset state so it doesn't trigger again for this press
            timePressed = 0;   // Reset timer
            CorrectInteraction(); // Perform the action
        }
    }


    public void StartInteraction(bool stopGame = true)
    {
        waitingForAnyInput = false;
        anyInputDetectionAction.Disable(); // Ensure this is disabled on normal start

        isActive = true;
        Debug.Log("Start Interaction");
        if (stopGame)
            GameManager.Instance.PauseGame();

        userGuide.ShowInstruction(true);
        userGuide.NextMessage();

        // Always reset state when interaction starts
        isPressed = false;
        timePressed = 0;

    }

    public void EndInteraction(bool resumeGame = true)
    {
        waitingForAnyInput = false;
        anyInputDetectionAction.Disable(); // Ensure this is disabled on normal start

        isActive = false;
        Debug.Log("End Interaction");

        userGuide.ShowInstruction(false);

        if (resumeGame)
            GameManager.Instance.ResumeGame();

        // Ensure state is reset when interaction ends
        isPressed = false;
        timePressed = 0;

        exitCollider.enabled = false;
        enterCollider.enabled = false; // Disable the enter collider to prevent re-entry until explicitly restarted

        foreach (Collider col in boundedAreaColliders)
        {
            col.enabled = false;
        }
    }

    public void RestartInteraction()
    {
        enterCollider.enabled = false; // Re-enable the enter collider to allow re-entry

        carController.TeleportCar(resetPos);
        userGuide.ResetUserGuide();

        userGuide.ShowInstruction(true);
        userGuide.NextMessage();
        userGuide.NextMessage();

        userGuide.ShowAllComplementaryUI(true); // Hide the first complementary UI element if it was shown

        waitingForAnyInput = true;

        isPressed = false; // Reset isPressed to ensure the interaction can start fresh
        timePressed = 0; // Reset the timer as well

        anyInputDetectionAction.Enable(); // NEW: Enable the "any input" action here!
    }

    public void ExitBoundedAred()
    {
        RestartInteraction();
    }

    public void CorrectInteraction()
    {
        carController.SetCarSpeed(resumeCarSpeed, true); // Reset car speed to 0 when starting interaction
        onResumePressedButton?.Invoke(validAxisDir); // Reset throttle input to 0

        isPressed = false; // Reset isPressed after handling the interaction
        timePressed = 0; // Reset the timer as well
        isActive = false;
        waitingForAnyInput = false; // Ensure this is false after a correct interaction
        anyInputDetectionAction.Disable(); // Ensure this is disabled after correct interaction

        userGuide.ShowInstruction(false);
        GameManager.Instance.ResumeGame();

    }

    // This method will be called by a UnityEvent<float> from your input source (e.g., SimplifiedCarController.onThrottleCar)
    public void HandleInteraction(float value)
    {
        if (!isActive)
            return;

        if (value == 0)
        {
            isPressed = false; // If value is zero, reset isPressed
            timePressed = 0; // Reset the timer as well
            return;
        }

        // Define a small deadzone to handle near-zero float values
        const float deadzone = 0.05f;

        // Determine if the input value is considered "active" (beyond deadzone)
        bool isInputActive = Mathf.Abs(value) > deadzone;

        // Check if the input is in the desired direction
        bool isInputInDesiredDirection = false;
        if (validAxisDir > 0 && value > deadzone) // Desired positive (e.g., W, D) and input is positive
        {
            isInputInDesiredDirection = true;
        }
        else if (validAxisDir < 0 && value < -deadzone) // Desired negative (e.g., S, A) and input is negative
        {
            isInputInDesiredDirection = true;
        }
        else if (validAxisDir == 0 && isInputActive) // If validAxisDir is 0, any active input is valid (for simple button, or any axis movement)
        {
            isInputInDesiredDirection = true;
        }

        // Set isPressed based on whether the input is active AND in the correct direction
        isPressed = isInputActive && isInputInDesiredDirection;

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!waitingForAnyInput)
            return;

        Debug.Log("Any keyboard/mouse button detected while waiting, proceeding with normal guide.");
        waitingForAnyInput = false; // Stop waiting
        userGuide.NextMessage();    // Display the *next* message (your first normal instruction)
        userGuide.ShowAllComplementaryUI(false); // Show the first complementary UI element
                                                 // Reset long press state to allow normal interaction to start fresh
        isPressed = false;
        timePressed = 0;


    }
}