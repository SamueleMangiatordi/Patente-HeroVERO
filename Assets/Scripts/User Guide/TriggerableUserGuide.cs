using Ezereal;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem; // Make sure this namespace is present

public class TriggerableUserGuide : InteractionControllerBase // Inherit from the base class
{
    [Header("Triggerable Specific Settings")]
    [Tooltip("For how long the button must be pressed before considering it a valid press.")]
    [SerializeField] private float longPressTreshold = 0.5f;

    [Tooltip("1 for forward/right, -1 for backward/left. 0 for any non-zero input (e.g., simple button).")]
    [SerializeField] private float validAxisDir;

    [Tooltip("Array of colliders defining the bounded area for interaction.")]
    [SerializeField] private Collider[] boundedAreaColliders;

    private bool isPressed = false; // Used for checking if the button is currently pressed
    private float timePressed = 0;  // Used for checking how long the button is pressed

#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset(); // Call base Reset to get common references
        // Specific Reset logic for TriggerableUserGuide
        // No specific children to find here, assuming colliders are assigned manually
    }
#endif

    // Override Awake if you need TriggerableUserGuide specific initialization
    protected override void Awake()
    {
        base.Awake(); // ALWAYS call base.Awake() first!

        // Ensure bounded area colliders are initially disabled if not managed elsewhere
        if (boundedAreaColliders != null)
        {
            foreach (Collider col in boundedAreaColliders)
            {
                if (col != null) col.enabled = false;
            }
        }
    }

    protected override void Update()
    {
        base.Update(); // Call base Update for common logic (e.g., waitingForAnyInput)

        if (!isInteractionEnabled || _isWaitingForAnyInput) // Don't process input if not enabled or waiting for other input
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
            CorrectInteraction(); // Perform the action specific to this guide
        }
    }

    /// <summary>
    /// Handles input value to determine if an interaction button is pressed and in the correct direction.
    /// This method should be called by your input system (e.g., from SimplifiedCarController.onThrottleCar).
    /// </summary>
    /// <param name="value">The input axis value (e.g., throttle, steering).</param>
    public void HandleInteraction(float value)
    {
        if (!isInteractionEnabled || _isWaitingForAnyInput)
            return;

        if (value == 0)
        {
            isPressed = false; // If value is zero, reset isPressed
            timePressed = 0;   // Reset the timer as well
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

    public override void StartInteraction()
    {
        base.StartInteraction(); // Call base implementation

        // Specific setup for TriggerableUserGuide
        if (enterCollider != null) enterCollider.enabled = true; // Enable enter collider to detect car entry
        if (exitCollider != null) exitCollider.enabled = true; // Enable exit collider
        if (boundedAreaColliders != null)
        {
            foreach (Collider col in boundedAreaColliders)
            {
                if (col != null) col.enabled = true; // Enable bounded area colliders
            }
        }
    }

    public override void EndInteraction()
    {
        base.EndInteraction(); // Call base implementation

        // Specific cleanup for TriggerableUserGuide
        if (exitCollider != null) exitCollider.enabled = false;
        if (enterCollider != null) enterCollider.enabled = false;
        if (boundedAreaColliders != null)
        {
            foreach (Collider col in boundedAreaColliders)
            {
                if (col != null) col.enabled = false; // Disable bounded area colliders
            }
        }

        // Ensure input state is reset
        isPressed = false;
        timePressed = 0;
    }

    // This method is abstract in base, so it MUST be implemented here
    public override void CorrectInteraction()
    {
        if (resumeCarSpeed != 0f)
            carController.SetCarSpeed(resumeCarSpeed, true);

        onResumePressedButton?.Invoke(validAxisDir);

        userGuideController.EnableUserGuides(false);
        GameManager.Instance.ResumeGame();

        // Reset state after correct interaction
        isPressed = false;
        timePressed = 0;
        StopWaitingForAnyInput(); // Ensure we stop waiting for any input
    }

   
    // Override ExitBoundedAred to ensure it calls the base RestartInteraction with the correct guide
    public void ExitBoundedAred()
    {
        if (!isInteractionEnabled)
            return;

        RestartInteraction(outOfBoundsUserGuide, () => ResumeGameAfterWait()); //pass the specific guide type and method to execute after recieving an input
    }
}