using UnityEngine;

public class FirstQuestionInteractionController : InteractionControllerBase
{

    public override void StartInteraction()
    {
        base.StartInteraction(); // Call base implementation
        base.PauseGameAndShowUserGuide();
    }

    public void OnWrongAnswer()
    {
        base.RestartInteraction(UserGuideType.FirstQuestionWrongAnswer, OnResumeAction);
    }

    public void OnCorrectAnswer()
    {
        userGuideController.EnableUserGuides(false);

        StopWaitingForAnyInput(); // Ensure we stop waiting for any input

        base.EndInteraction(); // End the interaction
        GameManager.Instance.ResumeGame();
    }

    private void OnResumeAction()
    {
        base.ResumeGameAfterWait();
        base.StartWaitingForAnyInput(AvoidLoop);
    }

    /// <summary>
    /// it is used to avoid ifnite loop of calling ResumeGameAfterWait.
    /// Since this interaction does not fit exactly other existing pattern interaction, this works as adaptation
    /// </summary>
    private void AvoidLoop()
    {

    }



}
