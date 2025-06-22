using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events; // Needed if you use UnityEvents directly in this script later

/**
 * Classe per controllare il flusso di informazioni da fornire all'utente durante uno stato
 */
public class TriggerableUserGuide : MonoBehaviour
{
    [SerializeField] public bool isActive = false;  //used to check if the user guide is currently active and not execute the update method if not

    [SerializeField] private float longPressTreshold = 0.5f;    //for how long the button must be pressed before considering it a valid press

    [Tooltip("1 per avanti/destra, -1 per indietro/sinistra. 0 per qualsiasi non-zero input (e.g., simple button).")]
    [SerializeField] private float validAxisDir; 

    [Tooltip("The car prefab with all gameObjects (Simplified Electric Truck - Ready) , not only the electric truck prefab")]
    [SerializeField] private GameObject mainCarObject;
    [SerializeField] private UserGuide userGuide;   //the userGuid that prompt the user with instructions, placed inside the ameObject that has this script

    [SerializeField] private Transform resetPos;    //where to teleport the car if something is wrong

    private bool isPressed = false; //used for checking if the button is currently pressed
    private float timePressed = 0;  //used for checking how long the button is pressed

#if UNITY_EDITOR
    void Reset()
    {
        userGuide = transform.GetComponentInChildren<UserGuide>();
        resetPos = transform.Find("resetPos");
    }
#endif

    public void Update()
    {
        if (!isActive)
            return;

        // If the button is currently considered pressed (and in the correct direction)
        if (!isPressed)
            return;
        
        timePressed += Time.unscaledDeltaTime; // Accumulate time regardless of Time.timeScale
        if (timePressed % 0.05f < 0.01f)
        {
            // Log the time pressed every 0.05 seconds for debugging
            Debug.Log($"Button is pressed for {timePressed:F2} seconds. Valid axis direction: {validAxisDir}");
        }
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
        isActive = false;
        Debug.Log("End Interaction");

        userGuide.ShowInstruction(false);

        if (resumeGame)
            GameManager.Instance.ResumeGame();

        // Ensure state is reset when interaction ends
        isPressed = false;
        timePressed = 0;
    }

    public void RestartInteraction()
    {
        mainCarObject.transform.position = resetPos.position;
        userGuide.ResetUserGuide();
        StartInteraction(true);
        // isPressed and timePressed are already reset by StartInteraction()
    }

    public void ExitBoundedAred()
    {
        RestartInteraction();
    }

    public void CorrectInteraction()
    {
        userGuide.ShowInstruction(false);
        GameManager.Instance.ResumeGame();
    }

    // This method will be called by a UnityEvent<float> from your input source (e.g., SimplifiedCarController.onThrottleCar)
    public void HandleInteraction(float value)
    {
        if (!isActive)
            return;

        Debug.Log( "value: " +value + " / AxisDir: " + validAxisDir + " / isPressed: " + isPressed + " / timePressed: " + timePressed);
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

        Debug.Log($"HandleInteraction: value={value}, validAxisDir={validAxisDir}, isPressed={isPressed}");
    }
}