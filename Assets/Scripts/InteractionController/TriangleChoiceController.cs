using Ezereal;
using System.Collections.Generic;
using UnityEngine;

public class TriangleChoiceController : MonoBehaviour
{
    [SerializeField] private CameraViews cameraView = CameraViews.AccidentTriangleDistanceChoice;

    [SerializeField] private GameObject correctTriangleDistance;

    [SerializeField] private GameObject mainTriangle;
    [SerializeField] private List<GameObject> triangles;

    [Header("Triangle Idle Animation")]
    [SerializeField] private float idleHeightAnimation = 3f;
    [SerializeField] private float idleHeightAnimationSpeed = 1f;
    [SerializeField] private float idleRotationSpeed = 30f;
    private bool _isIdleAnimationActive = false;

    [Header("UI")]
    [SerializeField] private GameObject distanceChoicePanel;
    [SerializeField] private GameObject confirmChoicePanel;

    private LerpMovement _mainTriangleLerp;
    private Vector3 _mainTriangleDefaultPos;

    // Dictionary to hold the triangles and their default positions
    private Dictionary<Transform, Vector3> _trianglesMeshes = new();

    private GameObject _currentSelectedTriangle = null;

    private void Awake()
    {
        mainTriangle.SetActive(false);
        foreach (var triangle in triangles)
        {
            triangle.SetActive(false);
            GameObject meshObj = triangle.GetComponentInChildren<MeshRenderer>().gameObject;

            _trianglesMeshes.Add(meshObj.transform, meshObj.transform.position);
        }

        _mainTriangleLerp = mainTriangle.GetComponent<LerpMovement>();

        _mainTriangleDefaultPos = mainTriangle.transform.position;  
    }

    private void Start()
    {
        foreach (var (triangleMesh, defaultPos) in _trianglesMeshes)
        {
            ClickableObject clickable = triangleMesh.GetComponent<ClickableObject>();
            clickable.onClickDown += (gameObj) =>
            {
                Debug.Log($"Triangle clicked: {gameObj.transform.parent.name}");
                MoveTo(gameObj.transform);
            };
        }
    }


    public void Enter()
    {
        EzerealCameraController.Instance.SetCameraView(cameraView, false);
        _isIdleAnimationActive = true;

        mainTriangle.SetActive(true);
        foreach (var triangle in triangles)
        {
            triangle.SetActive(true);
        }

    }

    private void Update()
    {
        if (!_isIdleAnimationActive)
            return;

        // Use a shifted sine wave that oscillates between 0 and 1.
        // Mathf.Sin() goes from -1 to 1. By adding 1 and dividing by 2,
        // we remap this range to 0 to 1.
        float sineNormalized = (Mathf.Sin(Time.time * idleHeightAnimationSpeed) + 1f) / 2f;

        // The final vertical position is now a Lerp between the default position and the peak position.
        Vector3 mainTriangleNewPos = _mainTriangleDefaultPos + Vector3.up * (idleHeightAnimation * sineNormalized);

        mainTriangle.transform.position = mainTriangleNewPos;
        //mainTriangle.transform.Rotate(Vector3.up, idleRotationSpeed * Time.deltaTime);

        foreach (var (triangleMesh, defaultPos) in _trianglesMeshes)
        {
            // Apply the same logic to the other triangles
            Vector3 triangleNewPos = defaultPos + Vector3.up * (idleHeightAnimation * sineNormalized);
            triangleMesh.transform.position = triangleNewPos;
        }

    }

    public void Exit()
    {
        EzerealCameraController.Instance.SetCameraView(CameraViews.Accident_far, false);
        _isIdleAnimationActive = false;

        foreach (var triangle in triangles)
        {
            triangle.SetActive(false);
        }
        confirmChoicePanel.SetActive(false);
    }

    public void MoveTo(Transform target)
    {
        if (confirmChoicePanel.activeSelf)
            return;


        _isIdleAnimationActive = false;

        if(Vector3.Distance(mainTriangle.transform.position, target.position) < 0.5f)
        {
            Debug.Log("Already at the target position, moving back.");
            _currentSelectedTriangle.SetActive(true);
            _currentSelectedTriangle = null;

            _mainTriangleLerp.GoTo(_mainTriangleDefaultPos, LerpVelocity.Slow);
            _isIdleAnimationActive = true;
            return;
        }

        Debug.Log("Moving to target position: " + target.position);
        _mainTriangleLerp.onEndMovement += () =>
        {
            _currentSelectedTriangle.SetActive(false);

            confirmChoicePanel.SetActive(true);
        };
        _mainTriangleLerp.GoTo(target, false, LerpVelocity.Slow);
        _currentSelectedTriangle = target.gameObject;
    }

    public void ConfirmChoice()
    {
        if (_currentSelectedTriangle.transform.parent.gameObject != correctTriangleDistance)
        {
            DismissChoiche();
            return;
        }

        Exit();
    }

    public void DismissChoiche()
    {
        confirmChoicePanel.SetActive(false);

        foreach (var (triangleMesh, _) in _trianglesMeshes)
        {
            triangleMesh.gameObject.SetActive(true);
        }
        _mainTriangleLerp.GoTo(_mainTriangleDefaultPos, LerpVelocity.Slow);
        _isIdleAnimationActive = true;
    }
}
