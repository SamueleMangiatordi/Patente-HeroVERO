using Ezereal;
using System;
using UnityEngine;
using UnityEngine.Events; // Ensure this is present

public class SignInteractionController : InteractionControllerBase // Inherit from the base class
{
    [Header("Sign Specific Settings")]
    [Tooltip("UserGuide to show when the car hits something related to the sign.")]
    [SerializeField] private UserGuideType carHittedUserGuide;

    // No specific Awake or Update override needed unless you add unique logic here.
    // The base Awake and Update will handle common initialization and waitingForAnyInput.

#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset(); // Call base Reset to get common references
        // No specific reset logic for SignInteractionController's own fields
    }
#endif

    public override void StartInteraction()
    {
        base.StartInteraction();
        StartWaitingForAnyInput(OnSignDetailsEnd);
    }

    // --- NEW: Method for when the car hits something specific to the sign ---
    // This would be called by a collision detection script on the sign itself,
    // or by another script that detects "hitting the sign".
    public void OnCarHit()
    {
        if (!isInteractionEnabled) return;

        Debug.Log($"Car hit sign '{name}'. Restarting interaction with 'Car Hitted' guide.");

        // Example: Provide a custom action for 'car hitted'
        base.RestartInteraction(carHittedUserGuide, OnCarHitResumeAction);
    }



    // Custom action to be invoked when input is received after car hits sign
    private void OnCarHitResumeAction()
    {
        Debug.Log("Custom action for SignInteractionController: Car Hitted, input received.");
        // Perform specific logic for when the player hits a sign and then presses a key to resume.
        // For example, maybe you want to disable the sign entirely after one hit, or reset a score.
        // Then, call the default resume logic:
        ResumeGameAfterWait();
        StartWaitingForAnyInput(OnSignDetailsEnd); // Restart waiting for any input to dismiss the sign details
    }


    /// <summary>
    /// Method called when the user click to dismiss the signal detail panel
    /// </summary>
    private void OnSignDetailsEnd()
    {
        userGuideController.EnableUserGuides(false); // Disable user guides
        GameManager.Instance.ResumeGame(); // Resume the game after dismissing the sign details
        carController.SetCarSpeed(resumeCarSpeed, true); // Stop the car when sign details are dismissed
        CarAdapter carAdapter = carController.GetComponent<CarAdapter>();
         carAdapter.SimulateThrottleInput(0); // Ensure throttle is set to 0
        StopWaitingForAnyInput();
    }
}