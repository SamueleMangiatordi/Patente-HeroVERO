using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Events;

namespace Ezereal
{
    public class EzerealLightController : MonoBehaviour // This system uses Input System and has no references. Some methods here are called from other scripts.
    {
        public UnityEvent<float> onTurnSignal;
        [Tooltip("Steering angle to overcome before resetting the turn light")]
        [SerializeField] private float steeringAngleThreshold = 20f;
        [SerializeField] private float steeringAngleResetDelay = 0.5f; // Delay before resetting turn lights after steering straightens

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

        private bool _wasOverSteeringThreshold = false; // To track if the steering angle is over the threshold

        private void Start()
        {
            AllLightsOff();
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

        /// <summary>
        /// Checks the current steering angle to automatically disable turn signals
        /// if the steering wheel straightens after a turn.
        /// </summary>
        /// <param name="currentSteeringAngle">The current actual steering angle of the wheels.</param>
        public void AutoDisableTurnLight(float currentSteeringAngle)
        {
            // Use absolute value for comparison with threshold
            float absSteeringAngle = Mathf.Abs(currentSteeringAngle);

            // If currently turning (angle exceeds threshold)
            if (absSteeringAngle > steeringAngleThreshold)
            {
                _wasOverSteeringThreshold = true; // Mark that we've been turning
            }
            // If we were turning, and now the steering wheel is near center (within threshold)
            else if (_wasOverSteeringThreshold && absSteeringAngle <= steeringAngleThreshold)
            {
                // Only auto-disable if individual turn signals are active, NOT hazard lights
                if ((_leftTurnActiveInternal || _rightTurnActiveInternal) && !_hazardLightsActiveInternal)
                {
                   StartCoroutine(WaitToDisableLight(steeringAngleResetDelay)); // Wait before disabling to allow for a smooth transition
                }
                _wasOverSteeringThreshold = false; // Reset the flag
            }
            // If hazard lights are active, this method should not interfere with them.
            // If no turn signals are active and steering is straight, _wasOverSteeringThreshold remains false.
        }
        IEnumerator WaitToDisableLight(float delay)
        {
            yield return new WaitForSeconds(delay);
            StopAllCoroutines(); // Stop any active turn signal coroutine
            TurnLightsOff(); // Turn off both turn signal visuals
            _leftTurnActiveInternal = false; // Reset internal flags
            _rightTurnActiveInternal = false;
            onTurnSignal?.Invoke(0); // Notify subscribers that turn signals are off
        }


    }


}
