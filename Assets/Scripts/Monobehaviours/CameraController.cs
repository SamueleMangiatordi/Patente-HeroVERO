using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraController : MonoBehaviour
{
    public float defaultCameraVelocity = LerpVelocity.Normal;

    [SerializeField] private CameraPositions cameraPositions;
    [SerializeField] private LerpMovement lerpMovement;

    public CameraTarget currentPosition { get; private set; } = CameraTarget.Init;
    bool _stopped;

    public void MoveCameraTo(CameraTarget target, float velocity = LerpVelocity.None, bool waitCameraToStop = true)
    {
        _stopped = false;
        currentPosition = target;

        Transform targetPosition = cameraPositions.Positions[target];
        float realVelocity = velocity == LerpVelocity.None ? defaultCameraVelocity : velocity;
        lerpMovement.GoTo(targetPosition, true, realVelocity);

        if(waitCameraToStop)
            StartCoroutine(WaitForCameraToStop(targetPosition));
        else
            _stopped = true;
    }

    IEnumerator WaitForCameraToStop(Transform target)
    {
        while (Vector3.Distance(Camera.main.transform.position, target.position) > 0.01f)
        {
            yield return null; // aspetta il frame successivo
        }

        _stopped = true;
    }

    public IEnumerator WaitAndMoveCamera(float delay, CameraTarget target, float velocity = LerpVelocity.Normal, bool waitCameraToStop = true)
    {
        yield return new WaitForSeconds(delay);
        MoveCameraTo(target, velocity, waitCameraToStop);
    }

    public bool GetStopped()
    {
        return _stopped;
    }

}

[Serializable]
public class CameraPositions
{
    [SerializeField] private Transform _init;
    [SerializeField] private Transform _insert_filter;
    [SerializeField] private Transform _insert_filter_2;
    [SerializeField] private Transform _insert_material;
    [SerializeField] private Transform _insert_material_2;
    [SerializeField] private Transform _water_state;
    [SerializeField] private Transform _water_state_2;
    [SerializeField] private Transform _monitor_state;
    [SerializeField] private Transform _process_state;
    [SerializeField] private Transform _result_state;
    [SerializeField] private Transform _result_state_2;
    [SerializeField] private Transform _result_state_3;

    // Qui sotto per esempio continuerei con la nomenclatura dei target identica a quella degli stati e nello stesso ordine

    private Dictionary<CameraTarget, Transform> _positions;

    public Dictionary<CameraTarget, Transform> Positions
    {
        get
        {
            if (_positions == null)
            {
                _positions = new Dictionary<CameraTarget, Transform>
                {
                    { CameraTarget.Init, _init },
                    { CameraTarget.InsertFilter, _insert_filter }, //AGGIUSTAAAAAA
                    { CameraTarget.InsertFilter2, _insert_filter_2 },
                    { CameraTarget.InsertMaterial, _insert_material },
                    { CameraTarget.InsertMaterial2, _insert_material_2 },
                    { CameraTarget.WaterState, _water_state },
                    { CameraTarget.WaterState2, _water_state_2 },
                    { CameraTarget.MonitorState, _monitor_state },
                    { CameraTarget.ProcessState, _process_state },
                    { CameraTarget.ResultState, _result_state },
                    { CameraTarget.ResultState2, _result_state_2 },
                    { CameraTarget.ResultState3, _result_state_3 },
                };
            }
            return _positions;
        }
    }
}

public enum CameraTarget
{
    Init,
    InsertFilter,
    InsertFilter2,
    InsertMaterial,
    InsertMaterial2,
    WaterState,
    WaterState2,
    MonitorState,
    ProcessState,
    ResultState,
    ResultState2,
    ResultState3
}
