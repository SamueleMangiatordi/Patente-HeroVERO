using System;
using TMPro;
using UnityEngine;

/**
 * Classe per controllare il flusso di informazioni da fornire all'utente durante uno stato
 */
public class TriggerableUserGuide : MonoBehaviour
{
    [SerializeField] public bool isActive = false;

    [Tooltip("The car prefab with all gameObjects, not only the electric truck prefab")]
    [SerializeField] private GameObject mainCarObject;
    [SerializeField] private UserGuide userGuide;

    [SerializeField] private Collider enterTrigger;
    [SerializeField] private Collider exitTrigger;

    [Tooltip("The parent that contains the bounding of the valid zone for the player to move")]
    [SerializeField] private Transform boundedAreaTriggerParent;

    [SerializeField] private Transform resetPos;

    private Collider[] boundedAreaTriggers;

#if UNITY_EDITOR
    void Reset()
    {
        userGuide = transform.GetComponentInChildren<UserGuide>();
        enterTrigger = transform.Find("enterTrigger").GetComponent<Collider>();
        exitTrigger = transform.Find("exitTrigger").GetComponent<Collider>();
        resetPos = transform.Find("resetPos");

        boundedAreaTriggerParent = transform.Find("boundedAreaTriggerParent");
        boundedAreaTriggers = boundedAreaTriggerParent.GetComponentsInChildren<Collider>();

    }
#endif

    public void StartInteraction(bool stopGame = true)
    {
        isActive = true;
        Debug.Log("Start Interaction");
        if(stopGame)
            GameManager.Instance.PauseGame();

        userGuide.ShowInstruction(true);
        userGuide.NextMessage();

    }

    public void EndInteraction(bool resumeGame = true)
    {
        isActive = false;
        Debug.Log("End Interaction");

        userGuide.ShowInstruction(false);

        if (resumeGame)
            GameManager.Instance.ResumeGame();
    }

    public void RestartInteraction()
    {
        mainCarObject.transform.position = resetPos.position;
        userGuide.ResetUserGuide();
        StartInteraction(true);
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

}
