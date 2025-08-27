using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System; // Required for [Serializable]

public class SignUserGuide : UserGuide
{
    [Tooltip("The ScriptableObject containing data for this sign (image, title, description, type).")]
    [SerializeField] private SignData signData; // Data for the sign

    // Private references to components within the _instructionPanel.
    // Renamed to avoid confusion with class names or parameters.
    private Image _signImage; //the image inside the panel that holds the sign sprite
    private TextMeshProUGUI _signTitle;
    private TextMeshProUGUI _signDescription;
    private Image _panelImage; // the Image component on the _instructionPanel itself

    protected override void Awake()
    {
        base.Awake(); // Call the base UserGuide's Awake for shared initialization

        // Initialize the sign instruction panel's specific components
        if (_instructionPanel != null)
        {
            _signImage = _instructionPanel.transform.Find("Image")?.GetComponent<Image>();

            Transform parentTexts = _instructionPanel.transform.Find("Text");
            _signTitle = parentTexts.Find("Title")?.GetComponent<TextMeshProUGUI>();
            _signDescription = parentTexts.Find("Description")?.GetComponent<TextMeshProUGUI>();

            _panelImage = _instructionPanel.GetComponent<Image>(); // Get the image on the panel itself

            // Add warnings if components are not found
            if (_signImage == null) Debug.LogWarning($"SignUserGuide: 'image' component not found on instruction panel for guide {GuideType}.", this);
            if (_signTitle == null) Debug.LogWarning($"SignUserGuide: 'title' TextMeshProUGUI not found on instruction panel for guide {GuideType}.", this);
            if (_signDescription == null) Debug.LogWarning($"SignUserGuide: 'description' TextMeshProUGUI not found on instruction panel for guide {GuideType}.", this);
            if (_panelImage == null) Debug.LogWarning($"SignUserGuide: Image component not found directly on instruction panel for guide {GuideType}. Cannot set panel color.", this);
        }
        else
        {
            Debug.LogError($"SignUserGuide: Instruction Panel is not assigned in the inspector for guide {GuideType}. Cannot initialize components.", this);
        }
    }

    public override void ShowGuide(bool show) // Override the ShowGuide method
    {
        base.ShowGuide(show); // Call the base class method to show/hide the guide panel and complementary UIs

        if (show && signData != null)
        {
            // Set content only when showing the guide
            if (_signImage != null) _signImage.sprite = signData.image;
            //if (_panelImage != null) _panelImage.color = SignTypeColor.GetColor(signData.type);
            if (_signTitle != null) _signTitle.text = signData.signName; // Use signName from SignData
            if (_signDescription != null) _signDescription.text = signData.description;
        }
        else if (show && signData == null)
        {
            Debug.LogWarning($"SignUserGuide: No SignData assigned for guide {GuideType}. Content will not be displayed.", this);
        }
    }
}