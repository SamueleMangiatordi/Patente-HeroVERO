using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // Required for using Coroutines

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
        private Quaternion _startRotation;         // Camera's rotation when the reset starts
        private Quaternion _targetRotation;        // The desired final rotation (e.g., car's rotation)
        private float _resetStartTime;             // Time.time when the reset started

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

        public void SetCameraView(CameraViews view, Vector3 rotation)
        {
            for (int i = 0; i < cameras.Length; i++)
            {
                // Deactivate all cameras first
                cameras[i].SetActive(false);

                if (i == (int)view)
                {
                    // Activate the selected camera
                    cameras[i].SetActive(true);
                    // Set its initial rotation. This will be an instant snap, if you want it to be smooth
                    // on initial switch, you would need another smooth transition here.
                    cameras[i].transform.rotation = Quaternion.Euler(rotation);
                }
            }
        }

        /// <summary>
        /// Smoothly resets the currently active camera's rotation back to align with the car's rotation.
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
                Debug.LogWarning("EzerealCameraController: The 'car' reference is null. Cannot reset camera rotation relative to car.", this);
                return;
            }

            // Stop any existing smooth rotation coroutine before starting a new one
            if (_resetRotationCoroutine != null)
            {
                StopCoroutine(_resetRotationCoroutine);
            }

            // Define the starting and target rotations for the interpolation
            _startRotation = currentCameraGameObject.transform.rotation;
            _targetRotation = car.transform.rotation; // The camera will smoothly rotate to match the car's world rotation

            // Start the coroutine for smooth rotation
            _resetRotationCoroutine = StartCoroutine(SmoothlyRotateCamera(currentCameraGameObject));
        }

        // Coroutine to handle the smooth rotation over time
        private IEnumerator SmoothlyRotateCamera(GameObject cameraToRotate)
        {
            _resetStartTime = Time.time; // Record the time when the rotation started
            float elapsedTime = 0f;      // Tracks how much time has passed since the start

            // Loop until the elapsed time reaches the desired duration
            while (elapsedTime < resetRotationDuration)
            {
                // Calculate the interpolation factor (t) from 0 to 1
                // This determines how far along the rotation path we are
                elapsedTime = Time.time - _resetStartTime;
                float t = elapsedTime / resetRotationDuration;

                // Use Quaternion.Slerp (Spherical Linear Interpolation) for smooth, consistent rotation speed
                cameraToRotate.transform.rotation = Quaternion.Slerp(_startRotation, _targetRotation, t);

                yield return null; // Wait for the next frame before continuing the loop
            }

            // Ensure the camera's rotation is exactly the target rotation at the end
            // This prevents minor floating-point inaccuracies
            cameraToRotate.transform.rotation = _targetRotation;

            // Clear the coroutine reference as it has finished
            _resetRotationCoroutine = null;
        }
    }
}