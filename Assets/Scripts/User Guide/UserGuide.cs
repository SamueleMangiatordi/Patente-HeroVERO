using System;
using TMPro;
using UnityEngine;

/**
 * Classe per controllare il flusso di informazioni da fornire all'utente durante uno stato
 */
public class UserGuide : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] texts;  //the texts that are currently displayed (it is possible to have more texts in a state, like in watergun you have main camera and than container camera).

    [SerializeField] private string[] messages;  //the messages to be displayed, hardcoded from inspector for now

    [SerializeField] private ComplementaryUIElement[] complementaryUIElements;
    [SerializeField] private float floatingSpeed = 0.5f;  //the speed at which the UI elements will float, if addFloating is true
    [SerializeField] private float floatingHeight = 0.1f;  //the height at which the UI elements will float, if addFloating is true

    public GameObject _instructionPanel;
    private bool floatingEnabled = false;

    // Store original local positions to ensure consistent floating around their starting point
    private Vector3[] _originalLocalPositions;
    
    private int currentMessageIndex = -1;  //the index of the current message, start from -1 because when you enter a state, you call the nextMessage() to update the text, this way the first message is actually the first one in the array and not the second

    private int currentTextIndex = 0;


    void Awake()
    {
        // Initialize the array to store original positions
        if (complementaryUIElements != null && complementaryUIElements.Length > 0) // Use .Length for arrays
        {
            _originalLocalPositions = new Vector3[complementaryUIElements.Length];
            for (int i = 0; i < complementaryUIElements.Length; i++)
            {
                // Access the actual GameObject from the FloatingUIElement wrapper
                if (complementaryUIElements[i] != null && complementaryUIElements[i].uiElement != null)
                {
                    // Store the original local position of each element
                    _originalLocalPositions[i] = complementaryUIElements[i].uiElement.transform.localPosition;
                }
                else
                {
                    Debug.LogWarning($"FloatingUI: Element at index {i} in complementaryUIElements is null or its uiElement is not assigned.", this);
                    // Initialize with zero to prevent NullReferenceException if accessed later
                    _originalLocalPositions[i] = Vector3.zero;
                }
            }
        }
        else
        {
            _originalLocalPositions = new Vector3[0]; // Ensure array is not null if no elements
        }
    }

    public void Update()
    {
        if (!floatingEnabled)
            return;

        if (complementaryUIElements == null || complementaryUIElements.Length == 0)
            return;

        for (int i = 0; i < complementaryUIElements.Length; i++)
        {
            GameObject obj = complementaryUIElements[i].uiElement;
            bool shouldFloatThis = complementaryUIElements[i].shouldFloat;

            if (obj != null && obj.activeSelf && shouldFloatThis) // Check the flag
            {
                // ... (your floating logic using _originalLocalPositions[i]) ...
                float offsetY = Mathf.Sin(Time.time * floatingSpeed) * floatingHeight;

                obj.transform.localPosition = new Vector3(
                    _originalLocalPositions[i].x,
                    _originalLocalPositions[i].y + offsetY,
                    _originalLocalPositions[i].z
                );
            }
        }
    }

    public void NextMessage()
    {
        currentMessageIndex++;
        if (currentMessageIndex >= messages.Length)
        {
            currentMessageIndex = 0;
        }
        texts[0].text = messages[currentMessageIndex];
    }

    public void PreviousMessage()
    {
        currentMessageIndex--;
        if (currentMessageIndex < 0)
        {
            currentMessageIndex = messages.Length - 1;
        }
        texts[0].text = messages[currentMessageIndex];
    }

    public void SetCurrentText(int indexOfTextMeshPro)
    {
        ValidateText();
        currentTextIndex = indexOfTextMeshPro;
    }

    public void ShowInstruction(bool show)
    {
        _instructionPanel?.SetActive(show);
        floatingEnabled = show;
    }

    public void ValidateText()
    {
        if (texts == null || texts.Length == 0)
        {
            Debug.LogError("texts array is null or empty!");
            return;
        }

        if (texts[0] == null)
        {
            Debug.LogError($"texts[{currentTextIndex}] is null!");
            return;
        }          
    }

    public void SetCurrentText(string text)
    {
        ValidateText();
        texts[0].text = text;
    }

    public void ShowComplementaryUI(bool show, int index)
    {
        if (complementaryUIElements == null || complementaryUIElements.Length == 0)
        {
            Debug.Log("complementaryUIElements array is null or empty!", this);
            return;
        }

        if (index >= 0 && index < complementaryUIElements.Length)
        {
            // Show or hide the specific UI element at the given index
            if (complementaryUIElements[index] != null && complementaryUIElements[index].uiElement != null)
            {
                complementaryUIElements[index].uiElement.SetActive(show);
            }
            else
            {
                Debug.LogWarning($"Complementary UI element at index {index} is null or its uiElement is not assigned!", this);
            }
            return; // Exit after handling the specific index
        }
    }

    public void ShowAllComplementaryUI(bool show)
    {
        foreach (ComplementaryUIElement elementWrapper in complementaryUIElements)
        {
            // Access the actual GameObject from the wrapper
            if (elementWrapper != null && elementWrapper.uiElement != null)
            {
                elementWrapper.uiElement.SetActive(show);
            }
            else
            {
                Debug.LogWarning("One of the complementary UI elements in the list is null or its uiElement is not assigned!", this);
            }
        }
    }

    public void ResetUserGuide()
    {
        ShowAllComplementaryUI(false);
        ShowInstruction(false);
        currentMessageIndex = -1;
    }
}

[Serializable]
public class ComplementaryUIElement
{
    public GameObject uiElement;
    public bool shouldFloat = true; // Default to true
}