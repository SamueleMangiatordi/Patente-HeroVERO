using Ezereal;
using System.Collections.Generic;
using UnityEngine;

public class TriangleChoiceController : MonoBehaviour
{
    [SerializeField] private CameraViews cameraView = CameraViews.AccidentTriangleDistanceChoice;

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

    private List<Transform> _trianglesMeshes = new List<Transform>();

    private void Awake()
    {
        mainTriangle.SetActive(false);
        foreach (var triangle in triangles)
        {
            triangle.SetActive(false);
            _trianglesMeshes.Add(triangle.GetComponentInChildren<MeshRenderer>().transform);
        }

        _mainTriangleLerp = mainTriangle.GetComponent<LerpMovement>();

        _mainTriangleDefaultPos = mainTriangle.transform.position;  
    }

    private void Start()
    {
        foreach (var triangle in _trianglesMeshes)
        {
            ClickableObject clickable = triangle.GetComponent<ClickableObject>();
            clickable.onClickDown += (gameObj) =>
            {
                Debug.Log($"Triangle clicked: {gameObj.name}");
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

        // Aggiungo un offset di 90 gradi (PI/2) per far partire l'animazione dall'alto
        // The sine wave now starts at 1 (its peak), so the objects move upward first.
        float sineValue = Mathf.Sin(Time.time * idleHeightAnimationSpeed + Mathf.PI / 2f);

        mainTriangle.transform.position += idleHeightAnimation * sineValue * Time.deltaTime * Vector3.up;
        mainTriangle.transform.Rotate(Vector3.up, idleRotationSpeed * Time.deltaTime);

        foreach (var triangle in _trianglesMeshes)
        {
            triangle.transform.position += idleHeightAnimation * sineValue * Time.deltaTime * Vector3.up;
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
    }

    public void MoveTo(Transform target)
    {
        _isIdleAnimationActive = false;

        if(Vector2.Distance(mainTriangle.transform.position, target.position) < 2f)
        {
            _mainTriangleLerp.GoTo(_mainTriangleDefaultPos, LerpVelocity.Slow);
            return;
        }

        _mainTriangleLerp.onEndMovement += () =>
        {
            confirmChoicePanel.SetActive(true);
        };
        _mainTriangleLerp.GoTo(target, true, LerpVelocity.Slow);
    }
}
