using Ezereal;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PersonalAccidentController : MonoBehaviour
{
    [SerializeField] private GameObject mainCarPrefab;
    [SerializeField] private Transform teleportPoint;
    [SerializeField] private ParticleSystem carFireParticle;

    [SerializeField] private CameraViews startCameraView = CameraViews.Accident_far;

    [SerializeField] private GameObject accidentPanel;

    [SerializeField] private TriangleChoiceController _tringleChoiceController;
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject[] objectToDisableOnEnter;

    private SimplifiedCarController _carController;
    private EzerealCameraController _cameraController;
    private PlayerInput _playerInput;
    private CarAdapter _carAdapter;

    private bool _isAccidentEnabled = false;
    private bool _isWaitingInpit = false;

    private float _waitingInputTimer = 0f;
    private float _waitingInputTreshold = 0.5f; // Time in seconds to wait for input before resuming

    private bool transitionStarted = false;

    private void Awake()
    {
        carFireParticle.Stop();
    }

    private void Start()
    {
        _carController = mainCarPrefab.GetComponentInChildren<SimplifiedCarController>();
        _carAdapter = mainCarPrefab.GetComponentInChildren<CarAdapter>();
        _playerInput = mainCarPrefab.GetComponentInChildren<PlayerInput>();
        _cameraController = mainCarPrefab.GetComponentInChildren<EzerealCameraController>();
        pauseButton = GameObject.Find("PauseButton").GetComponent<Button>();
    }

    public void StartFire()
    {
        carFireParticle.Play();
        _carController.BypassingInputs = true; // Disabilita gli input del giocatore
        
        _playerInput.enabled = false; // Disabilita il componente PlayerInput per bloccare tutti gli input
        _cameraController.ResetCurrentCameraRotation();
        _carAdapter.SimulateThrottleInput(0f); // Set throttle to 0 to stop the car

        AiCarSpawner.IgnoreAllAiPlayerCollision(1000000);
        StartCoroutine(EnsureTransitionStart());
    }

    public void StartTranistion()
    {
        StartCoroutine(StartTransition());
        
        foreach (GameObject obj in objectToDisableOnEnter)
        {
            obj.SetActive(false);
        }
    }

    protected virtual void Update()
    {
        if (!_isAccidentEnabled) return;

        // Logic for waiting for any input to resume game (common to both)
        if (_isWaitingInpit)
        {
            _waitingInputTimer += Time.unscaledDeltaTime;

            if (_waitingInputTimer > _waitingInputTreshold && Input.anyKeyDown)
            {
                _waitingInputTimer = 0f; // Reset the timer after receiving input
                _isWaitingInpit = false; // Stop waiting for input

                accidentPanel.SetActive(false);
                pauseButton.interactable = true;
                _tringleChoiceController.Enter();
                return;
            }
        }
    }

    private IEnumerator StartTransition()
    {
        transitionStarted = true;
        yield return StartCoroutine(FadingTeleportController.Instance.WaitFadingTeleport(teleportPoint));

        EzerealCameraController.Instance.SetCameraView(startCameraView, false);

        accidentPanel.SetActive(true);
        _isWaitingInpit = true;
        pauseButton.interactable = false;
        _isAccidentEnabled = true;
        _carController.BypassingInputs = true; // Disabilita gli input del giocatore

    }

    private IEnumerator EnsureTransitionStart()
    {
        ResetOffRoad[] resetOffRoad = GameObject.FindObjectsByType<ResetOffRoad>(FindObjectsSortMode.None);
        foreach (var reset in resetOffRoad)
        {
            reset.enabled = false;
        }

        yield return new WaitForSecondsRealtime(3f); // Small delay to ensure all systems are ready
        if (!transitionStarted)
        {
            yield return StartCoroutine(StartTransition());
        }
    }
}
