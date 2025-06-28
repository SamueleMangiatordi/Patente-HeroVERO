using System;
using TMPro;
using UnityEngine;

/**
 * Classe per controllare il flusso di informazioni da fornire all'utente durante uno stato
 */
public class UserGuideController : MonoBehaviour
{
    [SerializeField] private UserGuide[] userGuides; // Array of UserGuide objects

    private UserGuide currentActiveUserGuide = null; // Reference to the currently active guide

    private void Awake()
    {
        // Ensure all guides are initially hidden
        if (userGuides != null)
        {
            foreach (UserGuide guide in userGuides)
            {
                if (guide != null)
                {
                    guide.ShowGuide(false);
                }
            }
        }
        currentActiveUserGuide = null; // No guide active on start
    }

    // --- MODIFIED: Parameter type changed to UserGuideType enum ---
    public void SetuserGuide(UserGuideType guideTypeToShow)
    {
        if (userGuides == null || userGuides.Length == 0)
        {
            Debug.LogError("UserGuides array is null or empty in UserGuideController!");
            return;
        }

        // Hide the currently active guide, if any
        if (currentActiveUserGuide != null)
        {
            currentActiveUserGuide.ShowGuide(false);
        }

        // Find and show the new user guide
        UserGuide newGuide = FindUserGuideByType(guideTypeToShow);
        if (newGuide != null)
        {
            newGuide.ShowGuide(true);
            currentActiveUserGuide = newGuide;
        }
        else
        {
            Debug.LogWarning($"UserGuide with type '{guideTypeToShow}' not found in the userGuides array! No guide will be shown.", this);
            currentActiveUserGuide = null; // No guide active if not found
        }
    }


    public void EnableUserGuides(bool show)
    {
        if (userGuides == null) return;
        foreach (UserGuide userGuide in userGuides)
        {
            if (userGuide != null)
            {
                userGuide.ShowGuide(show);
            }
            else
            {
                Debug.LogWarning("One of the UserGuide objects in the userGuides array is null!", this);
            }
        }
    }

    // Helper method to find a UserGuide by its enum type
    private UserGuide FindUserGuideByType(UserGuideType type)
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
    // -------------------------------------------------------------

}