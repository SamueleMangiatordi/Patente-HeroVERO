using Ezereal;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // Make sure this namespace is present

/**
 * Classe per controllare il flusso di informazioni da fornire all'utente durante uno stato
 */
public class TriggerableUserGuide : MonoBehaviour
{
    [SerializeField] public bool isInteractionEnabled = false;  //used to check if the interaction is enabled, if not prevent any interaction/trigger/update method to execute

    [SerializeField] private float longPressTreshold = 0.5f;    //for how long the button must be pressed before considering it a valid press

    [Tooltip("1 per avanti/destra, -1 per indietro/sinistra. 0 per qualsiasi non-zero input (e.g., simple button).")]
    [SerializeField] private float validAxisDir;

    [Tooltip("The car prefab with all gameObjects (Simplified Electric Truck - Ready) , not only the electric truck prefab")]
    [SerializeField] private GameObject mainCarObject;
    [Tooltip("The speed to set the car when ending the interaction. If setted to 0, the car mantains the speed had before starting the interaction.")]
    [SerializeField] private float resumeCarSpeed = 0f; //the speed to set the car when starting the interaction, usually 0
    [SerializeField] private float resumeTimeDelay = 0.15f; //the time to wait before resuming the game after the interaction ends
    [Tooltip("Since when pausing the game with a button pressed and resuming it with the same button still pressed lose the input and the button is considered not pressed anymore, this event is used to simulate the user elevate the button and pressing it again")]
    public UnityEvent<float> onResumePressedButton; // UnityEvent to simulate the button press

    [SerializeField] private Transform resetPos;    //where to teleport the car if something is wrong

    [SerializeField] private UserGuideController userGuideController;   //the userGuid that prompt the user with instructions, placed inside the ameObject that has this script

    [SerializeField] private Collider enterCollider;
    [SerializeField] private Collider exitCollider; // Colliders to detect when the user enters or exits the interaction area

    [SerializeField] private Collider[] boundedAreaColliders; // Array of colliders defining the bounded area for interaction

    [SerializeField] private UserGuideType startInteractionGuide; // Type of user guide to show when interaction starts
    [SerializeField] private UserGuideType outOfBoundsUserGuide; // Type of user guide to show when interaction starts

    private SimplifiedCarController carController; // Reference to the car controller, if needed for further interactions
    private CarStateParameters storedCarState; // New field to store the car's state
    private EzerealCameraController cameraController;

    private bool waitingForAnyInput = false;

    private bool isPressed = false; //used for checking if the button is currently pressed
    private float timePressed = 0;  //used for checking how long the button is pressed


#if UNITY_EDITOR
    void Reset()
    {
        userGuideController = transform.GetComponentInChildren<UserGuideController>();
        resetPos = transform.Find("resetPos");

        carController = mainCarObject.GetComponentInChildren<SimplifiedCarController>();
        cameraController = mainCarObject.GetComponentInChildren<EzerealCameraController>();
    }
#endif


    public void Awake()
    {
        carController = mainCarObject.GetComponentInChildren<SimplifiedCarController>();
        cameraController = mainCarObject.GetComponentInChildren<EzerealCameraController>();
    }

    public void Update()
    {
        if (!isInteractionEnabled)
            return;

        if (waitingForAnyInput)
        {
            // Detects any keyboard key press or any mouse button click
            if (UnityEngine.Input.anyKeyDown)
            {
                Debug.Log("Any keyboard/mouse button detected while waiting, proceeding with normal guide.");
                
                userGuideController.SetuserGuide(startInteractionGuide); // Set the user guide to the one specified for starting interaction

                //userGuideController.NextMessage();    // Display the *next* message (your first normal instruction)
                //userGuideController.ShowAllComplementaryUI(false); // Show the first complementary UI element

                isPressed = false;
                timePressed = 0;

                GameManager.Instance.ResumeGame();
                // --- NEW: Restore car state using the stored parameters ---
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
                // ----------------------------------------------------------w
                StartCoroutine(GameManager.Instance.WaitToPause(resumeTimeDelay)); // Wait a bit before pausing to ensure the car is telported and ready
                waitingForAnyInput = false; // Stop waiting

                return; // Consume this input for the guide, do not proceed with other Update logic if any
            }

        }

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

        isInteractionEnabled = true;
        Debug.Log("Start Interaction");

        if (stopGame)
            GameManager.Instance.PauseGame();

        cameraController.ResetCurrentCameraRotation(); // Reset the camera rotation to default
        userGuideController.SetuserGuide(startInteractionGuide); // Set the user guide to the one specified for starting interaction

        // Always reset state when interaction starts
        isPressed = false;
        timePressed = 0;

        storedCarState = new CarStateParameters(carController);

    }

    public void EndInteraction(bool resumeGame = true)
    {
        waitingForAnyInput = false;

        isInteractionEnabled = false;
        Debug.Log("End Interaction : " + this.name.ToString());

        userGuideController.EnableUserGuides(false);

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

    private void RestartInteraction()
    {
        enterCollider.enabled = false;

        GameManager.Instance.PauseGame();

        //// --- NEW: Restore car state using the stored parameters ---
        //if (storedCarState != null && resumeCarSpeed == 0)
        //{

        //    storedCarState.ApplyToCarController(carController);
        //    Debug.Log("Car state restored for RestartInteraction.");
        //}
        //else
        //{
        //    // Fallback if state wasn't stored (e.g., direct call to RestartInteraction without StartInteraction)
        //    // Teleport to resetPos with resumeCarSpeed (or 0 if resumeCarSpeed is 0)
            
        //    carController.TeleportCar(resetPos, resumeCarSpeed, true);
        //    Debug.LogWarning("No stored car state found for RestartInteraction. Using resetPos and configured resumeCarSpeed as fallback.");
        //}
        //// ----------------------------------------------------------

        userGuideController.SetuserGuide(outOfBoundsUserGuide); // Reset the user guide to the one specified for starting interaction

        isInteractionEnabled = true;
        waitingForAnyInput = true;
        isPressed = false; // Reset isPressed to ensure the interaction can start fresh
        timePressed = 0; // Reset the timer as well

    }

    public void ExitBoundedAred()
    {
        if(!isInteractionEnabled)
            return;

        RestartInteraction();
    }

    public void CorrectInteraction()
    {
        if (resumeCarSpeed != 0f)
            carController.SetCarSpeed(resumeCarSpeed, true); // Reset car speed to 0 when starting interaction

        onResumePressedButton?.Invoke(validAxisDir); // Reset throttle input to 0

        isPressed = false; // Reset isPressed after handling the interaction
        timePressed = 0; // Reset the timer as well
        //isInteractionEnabled = false;
        waitingForAnyInput = false; // Ensure this is false after a correct interaction

        userGuideController.EnableUserGuides(false);
        GameManager.Instance.ResumeGame();

    }

    // This method will be called by a UnityEvent<float> from your input source (e.g., SimplifiedCarController.onThrottleCar)
    public void HandleInteraction(float value)
    {
        if (!isInteractionEnabled)
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

}