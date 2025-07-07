using Ezereal;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;

public class SignInteractionController : MonoBehaviour
{
    [SerializeField] public bool isActive = true; // Whether the sign interaction is active or not

    [Tooltip("The car prefab with all gameObjects (Simplified Electric Truck - Ready) , not only the electric truck prefab")]
    [SerializeField] private GameObject mainCarObject;

    [SerializeField] private GameObject signInstructionPanel;
    [SerializeField] private SignData signData; // Data for the sign, including image, title, and description

    [SerializeField] private Collider enterCollider;
    [SerializeField] private Collider exitCollider; // Colliders to detect when the user enters or exits the interaction area

    private Image signImage;
    private TextMeshProUGUI title;
    private TextMeshProUGUI description;

    private EzerealCameraController cameraController; // Reference to the camera controller
    private CarStateParameters storedCarState; // Reference to the car state parameters
    private SimplifiedCarController carController; // Reference to the car controller

    private bool waitingForAnyInput = false;

    void Awake()
    {
        // Initialize the sign instruction panel and its components
        if (signInstructionPanel != null)
        {
            signImage = signInstructionPanel.transform.Find("image").GetComponent<Image>();
            title = signInstructionPanel.transform.Find("title").GetComponent<TextMeshProUGUI>();
            description = signInstructionPanel.transform.Find("description").GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogError("Sign Instruction Panel is not assigned in the inspector.", this);
        }
    }

    void Update()
    {
        if (!isActive)
            return;

        if (waitingForAnyInput)
        {
            if (Input.anyKeyDown)
            {

            }
        }
    }

    public void RestartInteraction()
    {
        enterCollider.enabled = false; // Disable the enter collider

        GameManager.Instance.PauseGame(); // Resume the game
        signInstructionPanel.SetActive(true); // Hide the sign instruction panel
        waitingForAnyInput = true;
    }

    public void StartInteraction()
    {
        if (isActive)
        {
            SetSignData(); // Set the sign data when interaction starts
            signInstructionPanel.SetActive(true); // Show the sign instruction panel

            GameManager.Instance.PauseGame(); // Pause the game
            cameraController.ResetCurrentCameraRotation(); // Reset the camera rotation to default

            storedCarState = new CarStateParameters(carController);
            waitingForAnyInput = true; // Set the flag to wait for any input
        }
        else
        {
            Debug.LogWarning("Sign interaction is not active.", this);
        }
    }

    public void EndInteraction()
    {
        signInstructionPanel.SetActive(false); // Hide the sign instruction panel
        GameManager.Instance.ResumeGame(); // Resume the game

        enterCollider.enabled = false;
        exitCollider.enabled = false; // Disable the exit collider
        waitingForAnyInput = false; // Reset the flag


    }

    public void SetSignData()
    {
        signImage.sprite = signData.image; // Set the sign image
        signInstructionPanel.GetComponent<Image>().color = SignTypeColor.GetColor(signData.type); // Set the color based on the sign type
        title.text = signData.signName; // Set the sign title
        description.text = signData.description; // Set the sign description
    }

}
