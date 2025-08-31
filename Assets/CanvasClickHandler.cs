using UnityEngine;
using UnityEngine.EventSystems;

public class CanvasClickHandler : MonoBehaviour, IPointerClickHandler
{
    // Make sure your 3D objects are on this specific layer
    [SerializeField] private LayerMask clickableObjectLayer;

    public void OnPointerClick(PointerEventData eventData)
    {
        // Create a ray from the camera through the clicked point on the screen.
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        RaycastHit hit;

        // Perform a raycast to check for a 3D object.
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickableObjectLayer))
        {
            // If the ray hits a 3D object, check if it has the ClickableObject component.
            ClickableObject clickable = hit.collider.GetComponent<ClickableObject>();
            if (clickable != null)
            {
                // Manually call the OnPointerDown event on the found component.
                clickable.OnPointerDown(eventData);
            }
        }
    }
}