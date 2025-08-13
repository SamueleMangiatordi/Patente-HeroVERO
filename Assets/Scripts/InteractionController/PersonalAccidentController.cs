using Ezereal;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonalAccidentController : MonoBehaviour
{
    [SerializeField] private GameObject mainCarPrefab;
    [SerializeField] private Transform teleportPoint;
    [SerializeField] private ParticleSystem carFireParticle;

    [SerializeField] private CameraViews startCameraView = CameraViews.Accident_far;

    [SerializeField] private GameObject accidentPanel;

    [SerializeField] private TriangleChoiceController _tringleChoiceController;

    private SimplifiedCarController _carController;
    private CarAdapter _carAdapter;

    private bool _isAccidentEnabled = false;
    private bool _isWaitingInpit = false;

    private float _waitingInputTimer = 0f;
    private float _waitingInputTreshold = 0.5f; // Time in seconds to wait for input before resuming

    private void Awake()
    {
        carFireParticle.Stop();
    }

    private void Start()
    {
        _carController = mainCarPrefab.GetComponentInChildren<SimplifiedCarController>();
        _carAdapter = mainCarPrefab.GetComponentInChildren<CarAdapter>();
    }

    public void StartFire()
    {
        carFireParticle.Play();
        _carController.BypassingInputs = true; // Disabilita gli input del giocatore
        _carAdapter.SimulateThrottleInput(0f); // Set throttle to 0 to stop the car
    }

    public void StartTranistion()
    {
        StartCoroutine(StartTransition());

        
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
                _tringleChoiceController.Enter();
                return;
            }
        }
    }

    private IEnumerator StartTransition()
    {
        yield return StartCoroutine(FadingTeleportController.Instance.WaitFadingTeleport(teleportPoint));

        EzerealCameraController.Instance.SetCameraView(startCameraView, false);

        accidentPanel.SetActive(true);
        _isWaitingInpit = true;
        _isAccidentEnabled = true;

    }
}
