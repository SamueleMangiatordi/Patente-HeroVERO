using TMPro;
using UnityEngine;
using System;
using System.Linq; // Required for [Serializable]

[Serializable]
public class SignleTextUserGuide : UserGuide
{
    [Tooltip("The TextMeshProUGUI component where the main title will be displayed.")]
    [SerializeField] protected TextMeshProUGUI mainTitleText;
    [Tooltip("The primary message text for this guide.")]
    [TextArea(3, 10)] // Makes the string field multi-line in Inspector
    [SerializeField] protected string mainMessage;

    protected override void Awake()
    {
        base.Awake(); // Call the base UserGuide's Awake for shared initialization

        // If mainTitleText isn't assigned, try to find it within the instruction panel.
        // Assumes a child GameObject named "title" with a TextMeshProUGUI component.
        if (mainTitleText == null && _instructionPanel != null)
        {
            Transform titleTransform = _instructionPanel.transform.Find("title");
            if (titleTransform != null)
            {
                mainTitleText = titleTransform.GetComponent<TextMeshProUGUI>();
            }

            if (mainTitleText == null)
            {
                Debug.LogWarning($"BaseTextUserGuide: 'mainTitleText' TextMeshProUGUI not assigned and not found as a child named 'title' of instruction panel for guide {GuideType}.", this);
            }
        }
        else if (mainTitleText == null)
        {
            Debug.LogWarning($"BaseTextUserGuide: 'mainTitleText' not assigned and instruction panel is null for guide {GuideType}. Cannot display text content.", this);
        }
    }

    public override void ShowGuide(bool show)
    {
        base.ShowGuide(show); // Call the base UserGuide's ShowGuide to handle panel activation and complementary UIs

        if (show)
        {
            if (mainTitleText != null && mainMessage != null && (mainMessage.Length > 0) )
            {
                mainTitleText.text = mainMessage; // Set the message text when showing the guide
            }
            else
            {
                Debug.LogWarning($"BaseTextUserGuide: mainTitleText is null for guide {GuideType}. Cannot set message.", this);
            }
        }
    }
}