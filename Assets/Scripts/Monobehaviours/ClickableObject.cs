using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableObject : MonoBehaviour, IPointerDownHandler
{
    public Action<GameObject> onClickDown;
    public void OnPointerDown(PointerEventData eventData)
    {
        try
        {
            this.GetComponent<AudioSource>().Play();
        }
        catch (MissingComponentException mce)
        {
            Debug.LogWarning("No audio source attached to the gameobject");
        }

        onClickDown?.Invoke(gameObject);
    }
}
