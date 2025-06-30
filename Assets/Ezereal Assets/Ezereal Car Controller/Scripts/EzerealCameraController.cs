using System.Collections; // Required for using Coroutines
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ezereal
{
    public class EzerealCameraController : MonoBehaviour
    {
        [Tooltip("The electric truck GameObject used for camera rotation reference.")]
        [SerializeField] private GameObject car;
        [SerializeField] public CameraViews currentCameraView { get; private set; } = CameraViews.cockpit;

        [SerializeField] private GameObject[] cameras; // Assume cameras are in order: cockpit, close, far, locked, wheel

        [Header("Rotation Reset Settings")]
        [Tooltip("The duration (in seconds) for the camera to smoothly reset its rotation.")]
        [SerializeField] private float resetRotationDuration = 0.5f; // Default duration for the smooth reset

        // Private fields to manage the smooth rotation process
        private Coroutine _resetRotationCoroutine; // Stores reference to the running coroutine
       
        private void Awake()
        {
            // Initialize camera view. Assuming Vector3.zero for rotation means identity or default world alignment.
            SetCameraView(currentCameraView, Vector3.zero);
        }

        void OnSwitchCamera()
        {
            // Calculate the next camera view
            currentCameraView = (CameraViews)(((int)currentCameraView + 1) % cameras.Length);

            // If a rotation reset is currently in progress, stop it before switching cameras
            if (_resetRotationCoroutine != null)
            {
                StopCoroutine(_resetRotationCoroutine);
                _resetRotationCoroutine = null; // Clear the reference
            }

            // Set the new camera view instantly with its default rotation (Vector3.zero means Quaternion.identity)
            SetCameraView(currentCameraView, Vector3.zero);
        }

        public void SetCameraView(CameraViews view, Vector3 rotation) // 'rotation' parameter will now imply default axis values for Cinemachine
        {
            for (int i = 0; i < cameras.Length; i++)
            {
                // Deactivate all cameras first
                if (cameras[i] != null)
                {
                    cameras[i].SetActive(false);
                }

                if (i == (int)view)
                {
                    if (cameras[i] != null)
                    {
                        cameras[i].SetActive(true);

                        // Get the CinemachineVirtualCamera component
                        CinemachineCamera virtualCamera = cameras[i].GetComponent<CinemachineCamera>();
                        if (virtualCamera == null)
                        {
                            Debug.LogWarning($"EzerealCameraController: Camera GameObject '{cameras[i].name}' does not have a CinemachineVirtualCamera component.", this);
                            continue;
                        }

                        // Based on the camera type, set its axis values to default (implied by Vector3.zero in 'rotation')
                        if (view == CameraViews.cockpit)
                        {
                            CinemachinePanTilt panTilt = virtualCamera.GetComponent<CinemachinePanTilt>();
                            if (panTilt != null)
                            {
                                // Reset POV axes to their default (center)
                                panTilt.PanAxis.Value = 0f;
                                panTilt.TiltAxis.Value = 0f;
                            }
                        }
                        else if (view == CameraViews.close || view == CameraViews.far)
                        {
                            CinemachineOrbitalFollow orbital = virtualCamera.GetComponent<CinemachineOrbitalFollow>();
                            if (orbital != null)
                            {
                                // Reset Orbital X-Axis to its default (0)
                                orbital.HorizontalAxis.Value = 0f;
                            }
                        }
                        // For other camera types, you might have different default settings or no rotation to reset
                    }
                }
            }
        }

        /// <summary>
        /// Smoothly resets the currently active camera's rotation (Cinemachine axes) to its default.
        /// </summary>
        public void ResetCurrentCameraRotation()
        {
            int index = (int)currentCameraView;
            GameObject currentCameraGameObject = cameras[index];

            if (currentCameraGameObject == null)
            {
                Debug.LogWarning("EzerealCameraController: The current camera GameObject is null. Cannot reset rotation.", this);
                return;
            }
            if (car == null)
            {
                // This warning is less critical now as we are resetting Cinemachine axes,
                // which might not directly depend on 'car.transform.rotation' for their target default (usually 0).
                // However, if the intent was to align with car's *forward*, then car is still relevant.
                // For POV and Orbital, the default is typically axis.Value = 0.
                Debug.LogWarning("EzerealCameraController: The 'car' reference is null. Default reset for Cinemachine axes will be to zero.", this);
            }

            // Stop any existing smooth rotation coroutine before starting a new one
            if (_resetRotationCoroutine != null)
            {
                StopCoroutine(_resetRotationCoroutine);
            }

            // Get the CinemachineVirtualCamera component from the active camera GameObject
            CinemachineCamera virtualCamera = currentCameraGameObject.GetComponent<CinemachineCamera>();
            if (virtualCamera == null)
            {
                Debug.LogWarning($"EzerealCameraController: Active camera '{currentCameraGameObject.name}' does not have a CinemachineVirtualCamera component.", this);
                return;
            }

            // Determine the type of Cinemachine component and start the appropriate reset coroutine
            if (currentCameraView == CameraViews.cockpit)
            {
                CinemachinePanTilt panTilt = virtualCamera.GetComponent<CinemachinePanTilt>();
                if (panTilt != null)
                {
                    _resetRotationCoroutine = StartCoroutine(SmoothlyResetPanTilt(panTilt));
                }
                else
                {
                    Debug.LogWarning("EzerealCameraController: Cockpit camera view but no CinemachinePOV component found.", this);
                }
            }
            else if (currentCameraView == CameraViews.close || currentCameraView == CameraViews.far)
            {
                CinemachineOrbitalFollow orbital = virtualCamera.GetComponent<CinemachineOrbitalFollow>();
                if (orbital != null)
                {
                    _resetRotationCoroutine = StartCoroutine(SmoothlyResetOrbitalXAxis(orbital));
                }
                else
                {
                    Debug.LogWarning("EzerealCameraController: Free camera view but no CinemachineOrbitalTransposer component found.", this);
                }
            }
            else
            {
                Debug.LogWarning($"EzerealCameraController: Reset not implemented for camera view: {currentCameraView}", this);
            }
        }

        // Coroutine to handle smooth reset for CinemachinePOV axes (Pan and Tilt)
        private IEnumerator SmoothlyResetPanTilt(CinemachinePanTilt panTilt)
        {
            if (panTilt == null) yield break; // Safety check

            float initialVerticalValue = panTilt.TiltAxis.Value;
            float initialHorizontalValue = panTilt.PanAxis.Value;

            float resetStartTimeUnscaled = Time.unscaledTime; // Use unscaled time
            float elapsedTime = 0f;

            while (elapsedTime < resetRotationDuration)
            {
                elapsedTime = Time.unscaledTime - resetStartTimeUnscaled; // Use unscaled time
                float t = elapsedTime / resetRotationDuration;

                // Lerp both vertical and horizontal axis values to 0
                panTilt.TiltAxis.Value = Mathf.Lerp(initialVerticalValue, 0f, t);
                panTilt.PanAxis.Value = Mathf.Lerp(initialHorizontalValue, 0f, t);

                panTilt.transform.rotation = Quaternion.Euler(panTilt.TiltAxis.Value, panTilt.PanAxis.Value, 0f); // Update rotation based on axes
                yield return null;
            }

            // Ensure values are exactly 0 at the end
            panTilt.TiltAxis.Value = 0f;
            panTilt.PanAxis.Value = 0f;
            _resetRotationCoroutine = null;
        }

        // Coroutine to handle smooth reset for CinemachineOrbitalTransposer's X-Axis
        private IEnumerator SmoothlyResetOrbitalXAxis(CinemachineOrbitalFollow orbital)
        {
            if (orbital == null) yield break; // Safety check

            float initialXAxisValue = orbital.HorizontalAxis.Value;

            float resetStartTimeUnscaled = Time.unscaledTime; // Use unscaled time
            float elapsedTime = 0f;

            while (elapsedTime < resetRotationDuration)
            {
                elapsedTime = Time.unscaledTime - resetStartTimeUnscaled; // Use unscaled time
                float t = elapsedTime / resetRotationDuration;

                // Lerp the X-Axis value to 0
                orbital.HorizontalAxis.Value = Mathf.Lerp(initialXAxisValue, 0f, t);

                yield return null;
            }

            // Ensure value is exactly 0 at the end
            orbital.HorizontalAxis.Value = 0f;
            _resetRotationCoroutine = null;
        }
    }
}