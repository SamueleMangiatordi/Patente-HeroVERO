using System.Collections.Generic;
using UnityEngine;

// This is a regular MonoBehaviour that acts as a data container for our tool.
public class AreaPlacerTool : MonoBehaviour
{
    public List<GameObject> prefabsToPlace;
    public int numberOfObjects;
    public bool isChaotic;
    public float chaosMagnitude;
}