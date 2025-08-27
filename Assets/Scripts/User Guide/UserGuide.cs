using System;
using System.Linq;
using TMPro;
using UnityEngine;

[Serializable]
public abstract class UserGuide : MonoBehaviour
{
    [Tooltip("The unique type of this user guide for identification.")]
    [SerializeField] private UserGuideType userGuideType;
    public UserGuideType GuideType => userGuideType;

    [Tooltip("The main GameObject panel that contains all UI elements for this guide.")]
    [SerializeField] protected GameObject _instructionPanel; // Changed to protected for derived classes

    [Tooltip("Optional UI elements that float or have separate visibility controls.")]
    [SerializeField] private ComplementaryUIElement[] complementaryUIElements;
    [Tooltip("The speed at which complementary UI elements will float.")]
    [SerializeField] private float floatingSpeed = 0.5f;
    [Tooltip("The height range for complementary UI elements' floating motion.")]
    [SerializeField] private float floatingHeight = 0.1f;

    protected bool floatingEnabled = false; // Changed to protected

    // Store original local positions to ensure consistent floating around their starting point
    private Vector3[] _originalLocalPositions;

    protected virtual void Awake() // Changed to protected virtual
    {
        // Initialize the array to store original positions for floating
        if (complementaryUIElements != null && complementaryUIElements.Length > 0)
        {
            _originalLocalPositions = new Vector3[complementaryUIElements.Length];
            for (int i = 0; i < complementaryUIElements.Length; i++)
            {
                if (complementaryUIElements[i] != null && complementaryUIElements[i].uiElement != null)
                {
                    _originalLocalPositions[i] = complementaryUIElements[i].uiElement.transform.localPosition;
                }
                else
                {
                    //Debug.LogWarning($"UserGuide: Complementary UI element at index {i} is null or its uiElement is not assigned for guide {userGuideType}.", this);
                    _originalLocalPositions[i] = Vector3.zero;
                }
            }
        }
        else
        {
            _originalLocalPositions = new Vector3[0];
        }
    }

    protected virtual void Update() // Changed to protected virtual
    {
        if (!floatingEnabled)
            return;

        if (complementaryUIElements == null || complementaryUIElements.Length == 0 || _originalLocalPositions == null || _originalLocalPositions.Length == 0)
            return;

        for (int i = 0; i < complementaryUIElements.Length; i++)
        {
            GameObject obj = complementaryUIElements[i].uiElement;
            bool shouldFloatThis = complementaryUIElements[i].shouldFloat;

            if (obj != null && obj.activeSelf && shouldFloatThis)
            {
                float offsetY = Mathf.Sin(Time.time * floatingSpeed) * floatingHeight;

                obj.transform.localPosition = new Vector3(
                    _originalLocalPositions[i].x,
                    _originalLocalPositions[i].y + offsetY,
                    _originalLocalPositions[i].z
                );
            }
        }
    }

    /// <summary>
    /// Base method to show or hide the user guide panel and its complementary UI elements.
    /// Derived classes should override this to add specific content display logic.
    /// </summary>
    /// <param name="show">True to show, false to hide.</param>
    public virtual void ShowGuide(bool show)
    {
        if (_instructionPanel != null)
        {
            _instructionPanel.SetActive(show);
        }
        else
        {
            Debug.LogWarning($"UserGuide: Instruction Panel is not assigned for guide {userGuideType}. Cannot show/hide.", this);
        }

        // Enable floating only if the guide is active and there are elements set to float
        floatingEnabled = show && (complementaryUIElements != null && complementaryUIElements.Any(el => el != null && el.shouldFloat));
        ShowAllComplementaryUI(show); // Show or hide all complementary UI elements based on the show parameter
    }

    public void ShowComplementaryUI(bool show, int index)
    {
        if (complementaryUIElements == null || complementaryUIElements.Length == 0)
        {
            Debug.Log("UserGuide: complementaryUIElements array is null or empty! Cannot show/hide specific element.", this);
            return;
        }

        if (index >= 0 && index < complementaryUIElements.Length)
        {
            if (complementaryUIElements[index] != null && complementaryUIElements[index].uiElement != null)
            {
                complementaryUIElements[index].uiElement.SetActive(show);
            }
            else
            {
                Debug.LogWarning($"UserGuide: Complementary UI element at index {index} is null or its uiElement is not assigned!", this);
            }
        }
        else
        {
            Debug.LogWarning($"UserGuide: Index {index} is out of bounds for complementaryUIElements array of size {complementaryUIElements.Length}.", this);
        }
    }

    public void ShowAllComplementaryUI(bool show)
    {
        if (complementaryUIElements == null) return;
        foreach (ComplementaryUIElement elementWrapper in complementaryUIElements)
        {
            if (elementWrapper != null && elementWrapper.uiElement != null)
            {
                elementWrapper.uiElement.SetActive(show);
            }
            else
            {
                //Debug.LogWarning("UserGuide: One of the complementary UI elements in the list is null or its uiElement is not assigned!", this);
            }
        }
    }

    public virtual void ResetGuide() // Made virtual for potential overrides
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
