using Ezereal;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events; // Ensure this is present

public class SignInteractionController : InteractionControllerBase // Inherit from the base class
{
    [Header("Sign Specific Settings")]
    [Tooltip("UserGuide to show when the car hits something related to the sign.")]
    [SerializeField] private UserGuideType carHittedUserGuide;
    [Tooltip("UserGuide to show when the player do no respect the right of way")]
    [SerializeField] private UserGuideType rightOfWayErrorUserGuide = UserGuideType.RightOfWayNotRespected;

    public bool RightOfWay { get; set; } = true; // Flag to track right of way status

    [SerializeField] private float maxVelocityOnSignStop = 0f; // Maximum speed to check right of way
    [SerializeField] private float timeToWaitForSignStop = 2f; // Time to wait for the car to stop at the sign

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
        //StartWaitingForAnyInput(OnSignDetailsEnd);
    }

    // --- NEW: Method for when the car hits something specific to the sign ---
    // This would be called by a collision detection script on the sign itself,
    // or by another script that detects "hitting the sign".
    public void OnCarHit()
    {

        CheckpointManager checkpointManager = FindAnyObjectByType<CheckpointManager>();
        Transform lastCheckpoint = checkpointManager.GetLastReachedCheckpoint();

        //carController.TeleportCar(lastCheckpoint, 0f, true);

        resetPos = lastCheckpoint;
        Debug.Log("Reset pos name = " + resetPos.name);


        //resetPos = carHitResetPos != null ? carHitResetPos : _resetPos;

        //Debug.Log($"Car hit sign '{name}'. Restarting interaction with 'Car Hitted' guide.");

        //if (resetPos != null)
        //{
        //    mainCarObject.transform.position = resetPos.position;
        //    mainCarObject.transform.rotation = resetPos.rotation;

        //}
        //else
        //{
        //    Debug.LogWarning("⚠ Nessun resetPos impostato!");
        //}

        // Example: Provide a custom action for 'car hitted'
        base.RestartInteraction(carHittedUserGuide, OnResumeAction);
    }
    
    public void CheckRightOfWay()
    {
        if (RightOfWay) return;

        base.RestartInteraction(rightOfWayErrorUserGuide, OnResumeAction);
    }

    public void OnStopSignStay()
    {
        if (carController.GetCurrentSpeed() > maxVelocityOnSignStop)
        {
            return;
        }

        StartCoroutine(OnStopSignRightOfWay());
    }

    /// <summary>
    /// Action to perform when the player commits an error and a user guide tells them to press any key to resume.
    /// When any key is pressed, it will perform this method.
    /// </summary>
    private void OnResumeAction()
    {
        Debug.Log("Custom action for SignInteractionController: Car Hitted, input received.");
        // Perform specific logic for when the player hits a sign and then presses a key to resume.
        // For example, maybe you want to disable the sign entirely after one hit, or reset a score.
        // Then, call the default resume logic:
        base.ResumeGameAfterWait();
        base.StartWaitingForAnyInput(OnSignDetailsEnd); // Restart waiting for any input to dismiss the sign details
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

    private IEnumerator OnStopSignRightOfWay()
    {
               // Wait for the car to stop at the sign
        yield return new WaitForSeconds(timeToWaitForSignStop);
        RightOfWay = true;
    }
}