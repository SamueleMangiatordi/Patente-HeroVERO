using UnityEngine;
using UnityEngine.InputSystem;

namespace Ezereal
{
    public class EzerealCameraController : MonoBehaviour
    {
        [SerializeField] public CameraViews currentCameraView { get; private set; } = CameraViews.cockpit;

        [SerializeField] private GameObject[] cameras; // Assume cameras are in order: cockpit, close, far, locked, wheel

        private void Awake()
        {
            SetCameraView(currentCameraView, Vector3.zero);
        }

        void OnSwitchCamera()
        {
            currentCameraView = (CameraViews)(((int)currentCameraView + 1) % cameras.Length);
            SetCameraView(currentCameraView, Vector3.zero);
        }

        public void SetCameraView(CameraViews view, Vector3 rotation)
        {
            for (int i = 0; i < cameras.Length; i++)
            {
                if(i == (int)view)
                    cameras[i].transform.rotation = Quaternion.Euler(rotation);
                
                cameras[i].SetActive(i == (int)view);
                
            }
        }

        public void ResetCurrentCameraRotation()
        {
            int index = (int)currentCameraView;
            cameras[index].transform.rotation = Quaternion.identity;
        }
    }
}
