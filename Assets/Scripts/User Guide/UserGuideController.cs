using System;
using TMPro;
using UnityEngine;
using System.Linq; // Needed for .Any() in some cases, not explicitly used in current snippet

/// <summary>
/// Controls the flow of information provided to the user via various UserGuide types.
/// </summary>
public class UserGuideController : MonoBehaviour
{
    [Tooltip("Array of all UserGuide objects (SingleTextUserGuide, SignUserGuide, etc.) managed by this controller.")]
    [SerializeField] private UserGuide[] userGuides; // Changed to UserGuide[] to support all derived types

    private UserGuide currentActiveUserGuide = null; // Reference to the currently active guide

    private void Awake()
    {
        // Ensure all guides are initially hidden at the start of the game
        if (userGuides != null)
        {
            foreach (UserGuide guide in userGuides) // Iterate over the base UserGuide type
            {
                if (guide != null)
                {
                    guide.ShowGuide(false);
                }
                else
                {
                    Debug.LogWarning("UserGuideController: One of the UserGuide objects in the array is null during Awake!", this);
                }
            }
        }
        currentActiveUserGuide = null; // No guide active on start
    }

    /// <summary>
    /// Activates a specific user guide and deactivates the previously active one.
    /// </summary>
    /// <param name="guideTypeToShow">The type of UserGuide to activate.</param>
    public void SetUserGuide(UserGuideType guideTypeToShow) // Renamed parameter for clarity
    {
        if (userGuides == null || userGuides.Length == 0)
        {
            Debug.LogError("UserGuideController: UserGuides array is null or empty! Cannot set guide.", this);
            return;
        }

        // Hide the currently active guide, if any
        if (currentActiveUserGuide != null)
        {
            currentActiveUserGuide.ShowGuide(false);
        }

        // Find and show the new user guide based on its type
        UserGuide newGuide = FindUserGuideByType(guideTypeToShow); // Now returns UserGuide base type
        if (newGuide != null)
        {
            newGuide.ShowGuide(true);
            currentActiveUserGuide = newGuide;
        }
        else
        {
            Debug.LogWarning($"UserGuideController: UserGuide with type '{guideTypeToShow}' not found in the userGuides array. No guide will be shown.", this);
            currentActiveUserGuide = null; // No guide active if not found
        }
    }

    /// <summary>
    /// Globally enables or disables all managed user guides.
    /// </summary>
    /// <param name="show">True to show all guides, false to hide all guides.</param>
    public void EnableUserGuides(bool show)
    {
        if (userGuides == null) return;

        foreach (UserGuide userGuide in userGuides) // Iterate over the base UserGuide type
        {
            if (userGuide != null)
            {
                userGuide.ShowGuide(show);
            }
            else
            {
                Debug.LogWarning("UserGuideController: One of the UserGuide objects in the userGuides array is null during EnableUserGuides!", this);
            }
        }
    }

    // Helper method to find a UserGuide by its enum type
    private UserGuide FindUserGuideByType(UserGuideType type) // Returns the base UserGuide type
    {
        foreach (UserGuide guide in userGuides)
        {
            if (guide != null && guide.GuideType == type)
            {
                return guide;
            }
        }
        return null; // Not found
    }
}