using System.Collections.Generic;
using UnityEngine;

// This script can be added to a GameObject in the scene
public class AreaPlacerTool : MonoBehaviour
{
    // These variables will be set by the Editor window
    public List<GameObject> prefabsToPlace;
    public int numberOfObjects;
    public bool isChaotic;
    public float chaosMagnitude;
}