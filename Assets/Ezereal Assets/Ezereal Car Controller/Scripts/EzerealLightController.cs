using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Events;
using System.Security.Cryptography;

namespace Ezereal
{
    public class EzerealLightController : MonoBehaviour // This system uses Input System and has no references. Some methods here are called from other scripts.
    {
        public UnityEvent<float> onTurnSignal;
        [Tooltip("Steering angle to overcome before resetting the turn light")]
        [SerializeField] private float steeringAngleThreshold = 20f;
        [SerializeField] private float steeringAngleResetDelay = 0.5f; // Delay before resetting turn lights after steering straightens

        // --- NEW: Steering Threshold Debounce ---
        [Tooltip("Minimum time the steering angle must stay above the threshold to be considered a 'true' turn.")]
        [SerializeField] private float minTimeAboveSteeringThreshold = 0.15f; // Example: 0.15 seconds
        private float _timeAboveThreshold = 0f;
        private Coroutine _steeringCheckCoroutine; // To manage the continuous checking

        // --- State tracking for auto-disable logic ---
        // This flag indicates if a "significant turn" (above threshold for min time) has occurred.
        // It stays true as long as the steering *remains* above the threshold or just returned below it.
        private bool _hasPerformedSignificantTurn = false;


        [Header("Beam Lights")]

        [SerializeField] LightBeam currentBeam = LightBeam.off;

        [SerializeField] GameObject[] lowBeamHeadlights;
        [SerializeField] GameObject[] highBeamHeadlights;
        [SerializeField] GameObject[] lowBeamSpotlights;
        [SerializeField] GameObject[] highBeamSpotlights;
        [SerializeField] GameObject[] rearLights;

        [Header("Brake Lights")]
        [SerializeField] GameObject[] brakeLights;

        [Header("Handbrake Light")]
        [SerializeField] GameObject[] handbrakeLight;

        [Header("Reverse Lights")]
        [SerializeField] GameObject[] reverseLights;

        [Header("Turn Lights")]
        [SerializeField] GameObject[] leftTurnLights;
        [SerializeField] GameObject[] rightTurnLights;

        [Header("Misc Lights")]
        [Tooltip("Any additional lights. Interior lights.")]
        [SerializeField] GameObject[] miscLights;

        [Header("Settings")]
        [SerializeField] float lightBlinkDelay = 0.5f;

        [Header("Debug")]
        [SerializeField] bool _leftTurnActiveInternal = false;
        [SerializeField] bool _rightTurnActiveInternal = false;
        [SerializeField] bool _hazardLightsActiveInternal = false;

        // --- NEW PUBLIC PROPERTIES TO EXPOSE LIGHT STATES ---
        public bool IsLeftTurnActive => _leftTurnActiveInternal;
        public bool IsRightTurnActive => _rightTurnActiveInternal;
        public bool AreHazardLightsActive => _hazardLightsActiveInternal;
        // ----------------------------------------------------

        // Coroutine for blinking turn lights (assuming it exists elsewhere or you'll add it)
        private Coroutine _turnSignalCoroutine;


        private void Start()
        {
            AllLightsOff();
        }

        /// <summary>
        /// Checks the current steering angle to automatically disable turn signals
        /// if the steering wheel straightens after a turn.
        /// This method is designed to be called continuously (e.g., from an input manager or vehicle physics).
        /// </summary>
        /// <param name="currentSteeringAngle">The current actual steering angle of the wheels.</param>
        public void AutoDisableTurnLight(float currentSteeringAngle)
        {
            // Debug.Log($"AutoDisableTurnLight called with angle: {currentSteeringAngle}", this); // Keep for debugging if needed

            // Use absolute value for comparison with threshold
            float absSteeringAngle = Mathf.Abs(currentSteeringAngle);

            // --- Logic for ensuring steering stays above threshold for minimum time ---
            if (absSteeringAngle > steeringAngleThreshold)
            {
                // If we are currently above the threshold, and we haven't confirmed a significant turn yet,
                // or if the check coroutine isn't running, start it.
                if (!_hasPerformedSignificantTurn && _steeringCheckCoroutine == null)
                {
                    _steeringCheckCoroutine = StartCoroutine(CheckSteeringAboveThreshold());
                }
                // If already confirmed as a significant turn, keep _hasPerformedSignificantTurn true
                // as long as we're above the threshold.
            }
            else // Steering is at or below threshold
            {
                // If steering is no longer above threshold, stop the check and reset timer
                if (_steeringCheckCoroutine != null)
                {
                    StopCoroutine(_steeringCheckCoroutine);
                    _steeringCheckCoroutine = null;
                }
                _timeAboveThreshold = 0f; // Reset the timer
                                          // If steering returns to center, also reset the _wasSteeringSignificantlyTurned flag
                                          // to allow a new "true" turn detection.
            }

            // --- AUTO-DISABLE LOGIC ---
            // This part runs ONLY when:
            // 1. A significant turn was previously detected (_hasPerformedSignificantTurn is true).
            // 2. The steering has now returned to being near center (absSteeringAngle <= steeringAngleThreshold).
            // 3. Either left or right turn signal (but not hazards) is active.
            if (_hasPerformedSignificantTurn && absSteeringAngle <= steeringAngleThreshold)
            {
                // Only auto-disable if individual turn signals are active, NOT hazard lights
                if ((_leftTurnActiveInternal || _rightTurnActiveInternal) && !_hazardLightsActiveInternal)
                {
                    // Ensure we stop any active blinking coroutine before starting the disable sequence
                    if (_turnSignalCoroutine != null)
                    {
                        StopCoroutine(_turnSignalCoroutine);
                    }
                    StartCoroutine(WaitToDisableLight(steeringAngleResetDelay)); // Wait before disabling
                }
                // Reset _hasPerformedSignificantTurn AFTER triggering the disable
                // to prevent it from triggering again until a new significant turn occurs.
                _hasPerformedSignificantTurn = false;
            }
        }

        // Coroutine to check if the steering stays above the threshold
        IEnumerator CheckSteeringAboveThreshold()
        {
            _timeAboveThreshold = 0f;
            while (_timeAboveThreshold < minTimeAboveSteeringThreshold)
            {
                _timeAboveThreshold += Time.deltaTime;
                yield return null; // Wait for the next frame
            }
            // If we reach here, the steering has been above the threshold for the minimum time
            _hasPerformedSignificantTurn = true;
            _steeringCheckCoroutine = null; // Mark coroutine as finished
            Debug.Log("Steering has been above threshold for minimum time. Significant turn detected.");

        }

        IEnumerator WaitToDisableLight(float delay)
        {
            yield return new WaitForSeconds(delay);
            StopAllCoroutines(); // Stop any active turn signal coroutine
                                 // Only proceed if turn signals are still active and hazards are not
            if ((_leftTurnActiveInternal || _rightTurnActiveInternal) && !_hazardLightsActiveInternal)
            {
                TurnLightsOff(); // Turn off both turn signal visuals
                _leftTurnActiveInternal = false; // Reset internal flags
                _rightTurnActiveInternal = false;
                onTurnSignal?.Invoke(0); // Notify subscribers that turn signals are off
            }
        }




        public void AllLightsOff()
        {
            AllBeamsOff();
            ReverseLightsOff();
            TurnLightsOff();
            BrakeLightsOff();
            HandbrakeLightOff();
            //MiscLightsOff();
        }

        void OnLowBeamLight()
        {
            switch (currentBeam)
            {
                case LightBeam.off:
                    LowBeamOn();
                    break;
                case LightBeam.low:
                    AllBeamsOff();
                    break;
                case LightBeam.high:
                    AllBeamsOff();
                    break;
            }
        }

        void OnHighBeamLight()
        {
            switch (currentBeam)
            {
                case LightBeam.off:
                    HighBeamOn();
                    break;
                case LightBeam.low:
                    HighBeamOn();
                    break;
                case LightBeam.high:
                    AllBeamsOff();
                    break;
            }
        }
        void OnLeftTurnSignal()
        {
            if (!_hazardLightsActiveInternal)
            {
                StopAllCoroutines();
                TurnLightsOff();
                _rightTurnActiveInternal = false;
                _leftTurnActiveInternal = !_leftTurnActiveInternal;

                if (_leftTurnActiveInternal)
                {
                    StartCoroutine(TurnSignalController(leftTurnLights, _leftTurnActiveInternal));
                    onTurnSignal?.Invoke(-1); // Notify subscribers about the turn signal activation

                }
            }
        }

        void OnRightTurnSignal()
        {
            if (!_hazardLightsActiveInternal)
            {
                StopAllCoroutines();
                TurnLightsOff();
                _leftTurnActiveInternal = false;
                _rightTurnActiveInternal = !_rightTurnActiveInternal;

                if (_rightTurnActiveInternal)
                {
                    StartCoroutine(TurnSignalController(rightTurnLights, _rightTurnActiveInternal));
                    onTurnSignal?.Invoke(1); // Notify subscribers about the turn signal activation
                }
            }
        }

        void OnHazardLights()
        {
            StopAllCoroutines();
            TurnLightsOff();
            _leftTurnActiveInternal = false;
            _rightTurnActiveInternal = false;
            _hazardLightsActiveInternal = !_hazardLightsActiveInternal;

            if (_hazardLightsActiveInternal)
            {
                StartCoroutine(HazardLightsController());
            }
        }

        IEnumerator TurnSignalController(GameObject[] turnLights, bool isActive)
        {
            while (isActive)
            {
                SetLight(turnLights, true);
                yield return new WaitForSeconds(lightBlinkDelay);
                SetLight(turnLights, false);
                yield return new WaitForSeconds(lightBlinkDelay);
            }
        }

        IEnumerator HazardLightsController()
        {
            while (_hazardLightsActiveInternal)
            {
                TurnLightsOn();
                yield return new WaitForSeconds(lightBlinkDelay);
                TurnLightsOff();
                yield return new WaitForSeconds(lightBlinkDelay);
            }
        }
        void SetLight(GameObject[] lights, bool isActive)
        {
            if (isActive)
            {
                foreach (var light in lights)
                {
                    light.SetActive(true);
                }
            }
            else
            {
                foreach (var light in lights)
                {
                    light.SetActive(false);
                }
            }
        }

        void AllBeamsOff()
        {
            SetLight(lowBeamHeadlights, false);
            SetLight(lowBeamSpotlights, false);
            SetLight(rearLights, false);

            SetLight(highBeamHeadlights, false);
            SetLight(highBeamSpotlights, false);

            currentBeam = LightBeam.off;
        }

        void LowBeamOn()
        {
            SetLight(lowBeamHeadlights, true);
            SetLight(lowBeamSpotlights, true);
            SetLight(rearLights, true);

            SetLight(highBeamHeadlights, false);
            SetLight(highBeamSpotlights, false);

            currentBeam = LightBeam.low;
        }

        void HighBeamOn()
        {
            SetLight(lowBeamHeadlights, true);
            SetLight(lowBeamSpotlights, false);
            SetLight(rearLights, true);

            SetLight(highBeamHeadlights, true);
            SetLight(highBeamSpotlights, true);

            currentBeam = LightBeam.high;
        }

        void TurnLightsOff()
        {
            SetLight(leftTurnLights, false);
            SetLight(rightTurnLights, false);
        }

        void TurnLightsOn()
        {
            SetLight(leftTurnLights, true);
            SetLight(rightTurnLights, true);
        }

        void SetHazardLightsOn()
        {
            SetLight(leftTurnLights, true);
            SetLight(rightTurnLights, true);
        }

        public void BrakeLightsOff()
        {
            SetLight(brakeLights, false);
        }

        public void BrakeLightsOn()
        {
            SetLight(brakeLights, true);
        }

        public void HandbrakeLightOff()
        {
            SetLight(handbrakeLight, false);
        }

        public void HandbrakeLightOn()
        {
            SetLight(handbrakeLight, true);
        }

        public void ReverseLightsOff()
        {
            SetLight(reverseLights, false);
        }

        public void ReverseLightsOn()
        {
            SetLight(reverseLights, true);
        }

        public void MiscLightsOff() // Interior Lights
        {
            SetLight(miscLights, false);
        }

        public void MiscLightsOn() // Interior Lights
        {
            SetLight(miscLights, true);
        }



    }


}
