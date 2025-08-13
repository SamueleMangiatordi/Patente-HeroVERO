using System.Collections; // Required for using Coroutines
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ezereal
{
    public class EzerealCameraController : MonoBehaviour
    {
        public static EzerealCameraController Instance { get; private set; }

        [Tooltip("The electric truck GameObject used for camera rotation reference.")]
        [SerializeField] private GameObject car;
        [SerializeField] public CameraViews currentCameraView { get; private set; } = CameraViews.cockpit;

        [SerializeField] private GameObject[] cameras; // Assume cameras are in order: cockpit, close, far, locked, wheel

        [Header("Rotation Reset Settings")]
        [Tooltip("The duration (in seconds) for Cinemachine's internal recentering animation.")]
        [SerializeField] private float cinemachineRecenterDuration = 0.5f; // Used for RecenterTime

        [Tooltip("The time (in seconds) to wait after input stops before recentering begins.")]
        [SerializeField] private float cinemachineRecenterWaitTime = 0.1f; // Used for WaitTime

        [Tooltip("Tolerance for checking if a Cinemachine Axis value is effectively zero.")]
        [SerializeField] private float axisResetTolerance = 0.01f;

        private Coroutine _resetMonitorCoroutine; // Monitors when recentering is complete


        private void Awake()
        {
            Instance = this;

            // Initialize camera view. Assuming Vector3.zero for rotation means identity or default world alignment.
            SetCameraView(currentCameraView, true);
        }

        void OnSwitchCamera()
        {
            // Calculate the next camera view
            currentCameraView = (CameraViews)(((int)currentCameraView + 1) % cameras.Length);

            // If a reset monitor is currently in progress, stop it before switching cameras
            if (_resetMonitorCoroutine != null)
            {
                StopCoroutine(_resetMonitorCoroutine);
                _resetMonitorCoroutine = null; // Clear the reference
            }

            // Set the new camera view, instantly resetting its default rotation
            SetCameraView(currentCameraView, true); // Pass true to force an immediate reset
        }

        /// <summary>
        /// Smoothly resets the currently active camera's rotation using Cinemachine's built-in recentering.
        /// </summary>
        public void ResetCurrentCameraRotation()
        {
            if (cameras == null || cameras.Length == 0)
            {
                Debug.LogWarning("EzerealCameraController: Virtual Cameras array is not assigned or empty! Cannot reset rotation.", this);
                return;
            }

            CinemachineCamera activeCamera = cameras[(int)currentCameraView].GetComponent<CinemachineCamera>();

            if (activeCamera == null)
            {
                Debug.LogWarning("EzerealCameraController: The current active virtual camera is null. Cannot reset rotation.", this);
                return;
            }

            // Stop any existing monitoring coroutine
            if (_resetMonitorCoroutine != null)
            {
                StopCoroutine(_resetMonitorCoroutine);
                _resetMonitorCoroutine = null;
            }

            // Enable recentering and start monitoring its completion
            _resetMonitorCoroutine = StartCoroutine(MonitorCinemachineRecenter(activeCamera));
        }

        //public void SetCameraView(CameraViews view, bool instantReset) // 'rotation' parameter will now imply default axis values for Cinemachine
        //{
        //    for (int i = 0; i < cameras.Length; i++)
        //    {
        //        // Deactivate all cameras first
        //        if (cameras[i] != null)
        //        {
        //            cameras[i].SetActive(false);
        //        }

        //        if (i == (int)view)
        //        {
        //            if (cameras[i] != null)
        //            {
        //                cameras[i].SetActive(true);

        //                // Get the CinemachineVirtualCamera component
        //                CinemachineCamera virtualCamera = cameras[i].GetComponent<CinemachineCamera>();
        //                if (virtualCamera == null)
        //                {
        //                    Debug.LogWarning($"EzerealCameraController: Camera GameObject '{cameras[i].name}' does not have a CinemachineVirtualCamera component.", this);
        //                    continue;
        //                }

        //                // Based on the camera type, set its axis values to default (implied by Vector3.zero in 'rotation')
        //                if (view == CameraViews.cockpit)
        //                {
        //                    CinemachinePanTilt panTilt = virtualCamera.GetComponent<CinemachinePanTilt>();
        //                    if (panTilt != null)
        //                    {
        //                        // Reset POV axes to their default (center)
        //                        panTilt.PanAxis.Value = 0f;
        //                        panTilt.TiltAxis.Value = 0f;
        //                    }
        //                }
        //                else if (view == CameraViews.close || view == CameraViews.far)
        //                {
        //                    CinemachineOrbitalFollow orbital = virtualCamera.GetComponent<CinemachineOrbitalFollow>();
        //                    if (orbital != null)
        //                    {
        //                        // Reset Orbital X-Axis to its default (0)
        //                        orbital.HorizontalAxis.Value = 0f;
        //                    }
        //                }
        //                // For other camera types, you might have different default settings or no rotation to reset
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Activates a specific virtual camera view and optionally resets its Cinemachine axes.
        /// </summary>
        /// <param name="view">The CameraViews enum representing the desired camera.</param>
        /// <param name="instantReset">If true, axes are set directly to 0. If false, existing axis values are maintained.</param>
        public void SetCameraView(CameraViews view, bool instantReset)
        {

            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i] != null)
                {
                    cameras[i].gameObject.SetActive(i == (int)view);
                }
            }

            CinemachineCamera activeCamera = cameras[(int)view].GetComponent<CinemachineCamera>();

            // If we are resetting, ensure any active recentering is stopped first.
            if (instantReset)
            {
                // Directly set the axis values to 0 for instant reset
                ResetCinemachineAxesInstant(activeCamera);
            }
        }

        ///// <summary>
        ///// Smoothly resets the currently active camera's rotation (Cinemachine axes) to its default.
        ///// </summary>
        //public void ResetCurrentCameraRotation()
        //{
        //    int index = (int)currentCameraView;
        //    GameObject currentCameraGameObject = cameras[index];

        //    if (currentCameraGameObject == null)
        //    {
        //        Debug.LogWarning("EzerealCameraController: The current camera GameObject is null. Cannot reset rotation.", this);
        //        return;
        //    }
        //    if (car == null)
        //    {
        //        // This warning is less critical now as we are resetting Cinemachine axes,
        //        // which might not directly depend on 'car.transform.rotation' for their target default (usually 0).
        //        // However, if the intent was to align with car's *forward*, then car is still relevant.
        //        // For POV and Orbital, the default is typically axis.Value = 0.
        //        Debug.LogWarning("EzerealCameraController: The 'car' reference is null. Default reset for Cinemachine axes will be to zero.", this);
        //    }

        //    // Stop any existing smooth rotation coroutine before starting a new one
        //    if (_resetRotationCoroutine != null)
        //    {
        //        StopCoroutine(_resetRotationCoroutine);
        //    }

        //    // Get the CinemachineVirtualCamera component from the active camera GameObject
        //    CinemachineCamera virtualCamera = currentCameraGameObject.GetComponent<CinemachineCamera>();
        //    if (virtualCamera == null)
        //    {
        //        Debug.LogWarning($"EzerealCameraController: Active camera '{currentCameraGameObject.name}' does not have a CinemachineVirtualCamera component.", this);
        //        return;
        //    }

        //    // Determine the type of Cinemachine component and start the appropriate reset coroutine
        //    if (currentCameraView == CameraViews.cockpit)
        //    {
        //        CinemachinePanTilt panTilt = virtualCamera.GetComponent<CinemachinePanTilt>();

        //        if (panTilt != null)
        //        {
        //            _resetRotationCoroutine = StartCoroutine(SmoothlyResetPanTilt(panTilt));
        //        }
        //        else
        //        {
        //            Debug.LogWarning("EzerealCameraController: Cockpit camera view but no CinemachinePOV component found.", this);
        //        }
        //    }
        //    else if (currentCameraView == CameraViews.close || currentCameraView == CameraViews.far)
        //    {
        //        CinemachineOrbitalFollow orbital = virtualCamera.GetComponent<CinemachineOrbitalFollow>();
        //        if (orbital != null)
        //        {
        //            _resetRotationCoroutine = StartCoroutine(SmoothlyResetOrbitalXAxis(orbital));
        //        }
        //        else
        //        {
        //            Debug.LogWarning("EzerealCameraController: Free camera view but no CinemachineOrbitalTransposer component found.", this);
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogWarning($"EzerealCameraController: Reset not implemented for camera view: {currentCameraView}", this);
        //    }
        //}

        // Helper to instantly reset Cinemachine axes
        private void ResetCinemachineAxesInstant(CinemachineCamera virtualCamera)
        {
            // Reset POV component (for cockpit)
            if (virtualCamera.TryGetComponent<CinemachinePanTilt>(out var panTilt))
            {
                panTilt.PanAxis.Value = 0f;
                panTilt.TiltAxis.Value = 0f;
                panTilt.PanAxis.Recentering.Enabled = false; // Ensure it's not trying to recenter
                panTilt.TiltAxis.Recentering.Enabled = false;
            }
            // Reset Orbital component (for close/far)
            if (virtualCamera.TryGetComponent<CinemachineOrbitalFollow>(out var orbital))
            {
                orbital.HorizontalAxis.Value = 0f;
                orbital.HorizontalAxis.Recentering.Enabled = false; // Ensure it's not trying to recenter
            }
            // Add other Cinemachine components if you have more types (e.g., FreeLook, Transposer etc.)
        }

        // Coroutine to enable Cinemachine recentering and monitor its completion
        IEnumerator MonitorCinemachineRecenter(CinemachineCamera virtualCamera)
        {
            bool recenterStarted = false;
            bool panTiltRecenterEnabled = false;
            bool orbitalRecenterEnabled = false;

            // Get Cinemachine components.
            CinemachinePanTilt panTilt = virtualCamera.GetComponent<CinemachinePanTilt>();
            CinemachineOrbitalFollow orbital = virtualCamera.GetComponent<CinemachineOrbitalFollow>();

            if (panTilt != null)
            {
                panTilt.PanAxis.Recentering.Enabled = true;
                panTilt.PanAxis.Recentering.Wait = cinemachineRecenterWaitTime;
                panTilt.PanAxis.Recentering.Time = cinemachineRecenterDuration;
                panTilt.TiltAxis.Recentering.Enabled = true;
                panTilt.TiltAxis.Recentering.Wait = cinemachineRecenterWaitTime;
                panTilt.TiltAxis.Recentering.Time = cinemachineRecenterDuration;
                panTiltRecenterEnabled = true;
                recenterStarted = true;
            }
            if (orbital != null)
            {
                orbital.HorizontalAxis.Recentering.Enabled = true;
                orbital.HorizontalAxis.Recentering.Wait = cinemachineRecenterWaitTime;
                orbital.HorizontalAxis.Recentering.Time = cinemachineRecenterDuration;
                orbitalRecenterEnabled = true;
                recenterStarted = true;
            }

            if (!recenterStarted)
            {
                _resetMonitorCoroutine = null;
                yield break;
            }

            // --- IMPORTANT FOR TIME.TIMESCALE = 0 ---
            // If Time.timeScale is 0, Cinemachine's internal updates might also be paused.
            // To ensure recentering happens, you might need to manually call virtualCamera.UpdateCameraState().
            // However, the best approach is usually to configure CinemachineBrain's Update Method.
            // If you set CinemachineBrain's "Update Method" to "Late Update (unscaled)"
            // or a similar "unscaled" option, it will update regardless of Time.timeScale.
            // Alternatively, if you need to manually control updates (e.g., for frame-by-frame debug),
            // you could set the Virtual Camera's "Update Method" to "Manual Update" and call its UpdateCameraState()
            // from your own Update/FixedUpdate with Time.unscaledDeltaTime.
            // For typical gameplay, ensuring CinemachineBrain is set to an unscaled update method is simplest.

            // Wait until recentering is complete
            bool isRecenterComplete = false;
            while (!isRecenterComplete)
            {
                isRecenterComplete = true; // Assume complete unless proven otherwise

                if (panTiltRecenterEnabled)
                {
                    // Check if both Pan and Tilt axes are close to their target (0)
                    if (Mathf.Abs(panTilt.PanAxis.Value) > axisResetTolerance ||
                        Mathf.Abs(panTilt.TiltAxis.Value) > axisResetTolerance)
                    {
                        isRecenterComplete = false;
                    }
                }

                if (orbitalRecenterEnabled)
                {
                    // Check if Orbital X-Axis is close to its target (0)
                    if (Mathf.Abs(orbital.HorizontalAxis.Value) > axisResetTolerance)
                    {
                        isRecenterComplete = false;
                    }
                }

                // If recentering is not yet complete, yield and check again next frame
                if (!isRecenterComplete)
                {
                    yield return null; // Wait for the next frame
                }
            }

            // Recentering is complete, now disable the recentering properties
            if (panTiltRecenterEnabled)
            {
                panTilt.PanAxis.Recentering.Enabled = false;
                panTilt.TiltAxis.Recentering.Enabled = false;
            }
            if (orbitalRecenterEnabled)
            {
                orbital.HorizontalAxis.Recentering.Enabled = false;
            }

            _resetMonitorCoroutine = null;
        }
    }
}