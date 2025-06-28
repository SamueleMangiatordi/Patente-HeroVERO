using System;
using TMPro;
using UnityEngine;

[Serializable]
public class UserGuide : MonoBehaviour
{
    [SerializeField] private UserGuideType userGuideType; // Name of the user guide, used for identification
    public UserGuideType GuideType => userGuideType; // Public getter to access the enum type


    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private string message;

    [SerializeField] private ComplementaryUIElement[] complementaryUIElements;
    [SerializeField] private float floatingSpeed = 0.5f;  //the speed at which the UI elements will float, if addFloating is true
    [SerializeField] private float floatingHeight = 0.1f;  //the height at which the UI elements will float, if addFloating is true

    public GameObject _instructionPanel;
    private bool floatingEnabled = false;

    // Store original local positions to ensure consistent floating around their starting point
    private Vector3[] _originalLocalPositions;

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

    public void ShowGuide(bool show)
    {
        _instructionPanel.SetActive(show);
        text.text = message;
        floatingEnabled = complementaryUIElements != null && complementaryUIElements.Length > 0 && show;
        ShowAllComplementaryUI(show); // Show or hide all complementary UI elements based on the show parameter
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

    public void ResetGuide()
    {
        ShowAllComplementaryUI(false);
        ShowGuide(false);
    }

}


[Serializable]
public class ComplementaryUIElement
{
    public GameObject uiElement;
    public bool shouldFloat = true; // Default to true
}
