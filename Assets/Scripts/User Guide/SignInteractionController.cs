using Ezereal;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SignInteractionController : MonoBehaviour
{
    [SerializeField] public bool isActive = true; // Whether the sign interaction is active or not

    [Tooltip("The car prefab with all gameObjects (Simplified Electric Truck - Ready) , not only the electric truck prefab")]
    [SerializeField] private GameObject mainCarObject;
    [Tooltip("The speed to set the car when ending the interaction. If setted to 0, the car mantains the speed had before starting the interaction.")]
    [SerializeField] private float resumeCarSpeed = 0f; //the speed to set the car when starting the interaction, usually 0
    [SerializeField] private float resumeTimeDelay = 0.15f; //the time to wait before resuming the game after the interaction ends
    [Tooltip("Since when pausing the game with a button pressed and resuming it with the same button still pressed lose the input and the button is considered not pressed anymore, this event is used to simulate the user elevate the button and pressing it again")]
    public UnityEvent<float> onResumePressedButton; // UnityEvent to simulate the button press

    [SerializeField] private Collider enterCollider;
    [SerializeField] private Collider exitCollider; // Colliders to detect when the user enters or exits the interaction area

    [SerializeField] private Transform resetPos;    //where to teleport the car if something is wrong

    [SerializeField] private UserGuideController userGuideController; // Reference to the user guide controller
    [SerializeField] private UserGuideType startInteractionGuide; // Type of user guide to show when interaction starts
    [SerializeField] private UserGuideType outOfBoundsUserGuide; // Type of user guide to show when interaction starts
    [SerializeField] private UserGuideType carHittedUserGuide; // Type of user guide to show when interaction ends

    private EzerealCameraController cameraController; // Reference to the camera controller
    private CarStateParameters storedCarState; // Reference to the car state parameters
    private SimplifiedCarController carController; // Reference to the car controller

    private bool waitingForAnyInput = false;

    void Update()
    {
        if (!isActive)
            return;

        if (waitingForAnyInput)
        {
            if (Input.anyKeyDown)
            {
                userGuideController.SetUserGuide(startInteractionGuide);

                GameManager.Instance.ResumeGame(); // Resume the game
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
    }


    public void StartInteraction()
    {
        if (isActive)
        {
            userGuideController.SetUserGuide(startInteractionGuide); // Show the sign instruction panel

            GameManager.Instance.PauseGame(); // Pause the game
            cameraController.ResetCurrentCameraRotation(); // Reset the camera rotation to default

            storedCarState = new CarStateParameters(carController);
            waitingForAnyInput = false;
        }
        else
        {
            Debug.LogWarning("Sign interaction is not active.", this);
        }
    }

    public void EndInteraction()
    {
        waitingForAnyInput = false; // Reset the flag
        isActive = false; // Disable the sign interaction

        userGuideController.EnableUserGuides(false); // Hide the user guides

        GameManager.Instance.ResumeGame(); // Resume the game

        exitCollider.enabled = false; // Disable the exit collider
        enterCollider.enabled = false; // Disable the enter collider

    }

    public void RestartInteraction(UserGuideType guideTypeToShow)
    {
        enterCollider.enabled = false; // Disable the enter collider

        GameManager.Instance.PauseGame(); // Resume the game

        userGuideController.SetUserGuide(guideTypeToShow); // Show the user guide
        isActive = true; // Enable the sign interaction
        waitingForAnyInput = true;
    }
   

}
