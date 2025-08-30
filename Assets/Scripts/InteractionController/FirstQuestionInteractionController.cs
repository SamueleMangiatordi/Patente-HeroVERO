using UnityEngine;

public class FirstQuestionInteractionController : InteractionControllerBase
{
    [SerializeField] private GameObject correctAnswerPanel;

    public override void StartInteraction()
    {
        base.StartInteraction(); // Call base implementation
        base.PauseGameAndShowUserGuide();
    }

    public void ShowGuide()
    {
        base.PauseGameAndShowUserGuide();
    }

    public void OnWrongAnswer()
    {
        base.RestartInteraction(UserGuideType.FirstQuestionWrongAnswer, () => { OnResumeAction(true,true, false); }  );
        GameManager.Instance.ResumeGame();

    }

    public void OnCorrectAnswer()
    {
        userGuideController.EnableUserGuides(false);

        GameManager.Instance.ResumeGame();

        StopWaitingForAnyInput(); // Ensure we stop waiting for any input
        correctAnswerPanel.SetActive(true);
    }

    public override void OnCarHit()
    {
        base.OnCarHit();
        base.RestartInteraction(UserGuideType.CarHitted, () => { OnResumeAction(false, false, false); });
    }

    private void OnResumeAction(bool useStoredCarState = true, bool showDefaultPanel = true, bool showUserGuide = true)
    {
        base.ResumeGameAfterWait(UserGuideType.None, useStoredCarState, showUserGuide);

        if (!showDefaultPanel)
        {
            base.StopWaitingForAnyInput();
        }

        base.StartWaitingForAnyInput(AvoidLoop);
    }

    /// <summary>
    /// it is used to avoid ifnite loop of calling ResumeGameAfterWait.
    /// Since this interaction does not fit exactly other existing pattern interaction, this works as adaptation
    /// </summary>
    private void AvoidLoop()
    {
        Debug.Log("AvoidLoop called, resuming game without further interaction.");
        userGuideController.EnableUserGuides(false); // Disable user guides
        GameManager.Instance.ClearPause();
        carController.SetCarSpeed(resumeCarSpeed, true); // Stop the car when sign details are dismissed
        StopWaitingForAnyInput();
    }



}
