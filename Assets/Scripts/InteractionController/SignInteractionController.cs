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

    [Tooltip("Value of right of way when starting the game. Set it to true to allow the player car to pass, set it to false to give precedence to other cars in the scene and trigger an error if the player car cross the road anyway")]
    public bool rightOfWay = true;
    public bool RightOfWay { get; set; } = true; // Flag to track right of way status

    [SerializeField] private float maxVelocityOnSignStop = 0f; // Maximum speed to check right of way
    [SerializeField] private float timeToWaitForSignStop = 2f; // Time to wait for the car to stop at the sign
    [SerializeField] private AudioSource stopConfirmAudioSource;
    // No specific Awake or Update override needed unless you add unique logic here.
    // The base Awake and Update will handle common initialization and waitingForAnyInput.

    private Coroutine stopCoroutine = null;
    private Coroutine rightOfWayCoroutine = null;

#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset(); // Call base Reset to get common references
        // No specific reset logic for SignInteractionController's own fields
    }
#endif

    protected override void Start()
    {
        base.Start();
        RightOfWay = rightOfWay;
    }

    public override void StartInteraction()
    {
        base.StartInteraction();
        //StartWaitingForAnyInput(OnSignDetailsEnd);
    }

    public override void EndInteraction()
    {
        base.EndInteraction();
        if (stopCoroutine != null)
            StopCoroutine(stopCoroutine);

        if(rightOfWayCoroutine != null)
            StopCoroutine(rightOfWayCoroutine);

        stopCoroutine = null;
        rightOfWayCoroutine = null;
    }

    // --- NEW: Method for when the car hits something specific to the sign ---
    // This would be called by a collision detection script on the sign itself,
    // or by another script that detects "hitting the sign".
    public override void OnCarHit()
    {
        base.OnCarHit();

        // Example: Provide a custom action for 'car hitted'
        base.RestartInteraction(UserGuideType.CarHitted, () => { OnResumeAction(false, false, false, carHittedUserGuide); });
    }

    public void CheckRightOfWay()
    {
        if (RightOfWay)
        {

            if (stopCoroutine != null)
            {
                rightOfWayCoroutine = StartCoroutine(WaitToDisableRightOfWay(4f)); // Disable right of way after 5 seconds
            }
            return;
        }

        base.RestartInteraction(rightOfWayErrorUserGuide, () => { OnResumeAction(); });
    }

    public void OnStopSignStay()
    {
        if (carController.GetCurrentSpeed() > maxVelocityOnSignStop)
        {
            return;
        }

        if (stopCoroutine == null)
        {
            stopCoroutine = StartCoroutine(OnStopSignRightOfWay());
        }
    }

    /// <summary>
    /// Action to perform when the player commits an error and a user guide tells them to press any key to resume.
    /// When any key is pressed, it will perform this method.
    /// </summary>
    private void OnResumeAction(bool useStoredCarState = true, bool showSignDetail = true, bool showUserGuide = true, UserGuideType userGuideToShow = UserGuideType.None)
    {
        Debug.Log("Custom action for SignInteractionController: Car Hitted, input received.");
        // Perform specific logic for when the player hits a sign and then presses a key to resume.
        // For example, maybe you want to disable the sign entirely after one hit, or reset a score.
        // Then, call the default resume logic:
        base.ResumeGameAfterWait(userGuideToShow, useStoredCarState, showUserGuide);

        if (!showSignDetail)
        {
            base.StopWaitingForAnyInput();
        }

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
        stopCoroutine = null;
    }

    private IEnumerator OnStopSignRightOfWay()
    {
        // Wait for the car to stop at the sign
        yield return new WaitForSeconds(timeToWaitForSignStop);
        if (!isInteractionEnabled)
            yield break;

        RightOfWay = true;
        stopConfirmAudioSource = stopConfirmAudioSource ?? GameObject.Find("audio e video").transform.Find("ClickButtonSounds").GetComponent<AudioSource>();
        stopConfirmAudioSource.Play();
    }

    private IEnumerator WaitToDisableRightOfWay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        RightOfWay = false;
        stopCoroutine = null;
    }
}