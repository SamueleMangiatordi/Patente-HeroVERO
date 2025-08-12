using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class LerpMovement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private bool _canBeDragged = true;
    [SerializeField] private bool _kinematicOnDestination = false;
    [SerializeField] private bool _disableColliderOnDestination = false;

    [SerializeField] private float dragSpeed = 10f;
    [SerializeField] private float goingSpeed = 10f;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider col;

    public Action onEndMovement;
    public bool isMovingToTarget { get; private set; } = false; //prevent other interaction while moving to target

    private bool _isDragging = false;   // Is the object being dragged by the user
    private bool IsDragging
    {
        get => _isDragging;
        set => _isDragging = _canBeDragged ? value : false;
    }

    private bool _canInteract = true;   // Can the object be interacted with
    private float _zDistance = 0f;
    private Vector3 _targetPosition;


    private bool _isRotatingToTarget = false;
    private Vector3 _targetRotation;

    private static LerpMovement selected = null;

#if UNITY_EDITOR
    void Reset()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }
#endif

    public void FixedUpdate()
    {
        if (!IsDragging && !isMovingToTarget)
            return;

        if (rb.position == _targetPosition)
        {
            if (isMovingToTarget)
            {
                isMovingToTarget = false; // Stop moving when reached
                _canInteract = true; // Allow interaction again

                onEndMovement?.Invoke();
                onEndMovement = null; // Reset the action to avoid multiple calls
                if (_kinematicOnDestination) rb.isKinematic = true;
                if (_disableColliderOnDestination) col.enabled = false; // Reset collider state
            }

            if (_isRotatingToTarget)
            {
                _isRotatingToTarget = false;
                rb.rotation = Quaternion.Euler(_targetRotation);
            }

            return;
        }

        if (IsDragging)
        {
            if (_canInteract)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                _zDistance = Camera.main.WorldToScreenPoint(rb.position).z;
                _targetPosition = Mouse.current.position.value;
                _targetPosition.z = _zDistance;
                _targetPosition = Camera.main.ScreenToWorldPoint(_targetPosition);
            }
        }

        rb.position = Vector3.Lerp(rb.position, _targetPosition, Time.fixedDeltaTime * (_canInteract ? dragSpeed : goingSpeed));

        if (_isRotatingToTarget)
        {
            Quaternion targetRotation = Quaternion.Euler(_targetRotation);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * goingSpeed);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_canInteract)
            return;

        if (selected != null)
            return;

        selected = this;

        IsDragging = true;
        rb.useGravity = false;
        _zDistance = rb.position.z;

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_canInteract && selected == this)
        {
            selected = null;
            return;
        }

        if (!_canInteract)
            return;

        if (!selected || selected != this)
            return;

        selected = null;

        IsDragging = false;
        rb.useGravity = true;
    }

    public void GoTo(Transform transform, bool rotateToTarget = false, float velocity = LerpVelocity.Slow)
    {
        _canInteract = false;
        IsDragging = false;
        isMovingToTarget = true;
        _targetPosition = transform.position;

        goingSpeed = velocity;

        if (rotateToTarget)
        {
            this._isRotatingToTarget = true;
            _targetRotation = transform.rotation.eulerAngles;
        }
    }

    public void GoTo(Vector3 transformPosition, Vector3 rotation, float velocity = LerpVelocity.Normal)
    {
        _canInteract = false;
        IsDragging = false;
        isMovingToTarget = true;
        _targetPosition = transformPosition;

        if (rotation != Vector3.zero)
        {
            _isRotatingToTarget = true;
            _targetRotation = rotation;
        }
        else
        {
            _isRotatingToTarget = false;
        }

        goingSpeed = velocity;
    }

    public void GoTo(Vector3 transformPosition, float velocity = LerpVelocity.Normal)
    {
        GoTo(transformPosition, Vector3.zero, velocity);
    }

    public void BlockInteraction()
    {
        _canInteract = false;
        IsDragging = false;
        selected = null; // Also deselect
    }
}

public static class LerpVelocity
{
    public const float None = 0f;
    public const float VerySlow = 1f;
    public const float Slow = 3f;
    public const float Normal = 5f;
    public const float Fast = 10f;
}
