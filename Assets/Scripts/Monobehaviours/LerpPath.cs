using System.Collections.Generic;
using UnityEngine;

public class LerpPath : MonoBehaviour
{
    [SerializeField] public bool reversePath = false;
    [SerializeField] private float proximityThreshold = 0.1f;
    [SerializeField] private bool loopPath = false;
    [SerializeField] private float defaultVelocity = LerpVelocity.Fast;

    [SerializeField] private LerpMovement lerpMovementObj;
    [SerializeField] private Rigidbody rbMovingObj;
    [SerializeField] private Transform[] path;

    private bool _isMovingToTarget = false;
    private int currentPointIndex = 0;
    private bool reachedEndPath = false;
    

#if UNITY_EDITOR
    private void Reset()
    {
        path = GetComponentsInChildren<Transform>();
        for (int i = 0; i < path.Length; i++)
        {
            path[i].name = "PathPoint" + i;
        }
    }
#endif

    public void FixedUpdate()
    {
        if(reachedEndPath)
            return;

        if (!_isMovingToTarget)
            return;

        if (rbMovingObj == null || path == null || path.Length == 0)
            return;

        if (currentPointIndex >= path.Length || (reversePath && currentPointIndex == 0))
        {
            if (loopPath)
                currentPointIndex = 0;
            else
            {
                reachedEndPath = true;
                return;
            }
                
        }

        if(Vector3.Distance(rbMovingObj.position,path[currentPointIndex].position) <= proximityThreshold)
        {
            currentPointIndex = reversePath ? (currentPointIndex - 1) : (currentPointIndex + 1);
            if (currentPointIndex < path.Length)
                lerpMovementObj.GoTo(path[currentPointIndex], false, defaultVelocity);
            else if (loopPath && path.Length > 0)
                lerpMovementObj.GoTo(path[0], false, defaultVelocity);
        }
    }

    public void FollowPath(LerpMovement lerpMovement, Rigidbody rbMovingObj, bool reverse = false, float velocity = 0)
    {
        this.lerpMovementObj = lerpMovement;
        this.rbMovingObj = rbMovingObj;
        _isMovingToTarget = true;
        currentPointIndex = reverse ? path.Length-1 : 0;
        reachedEndPath = false;
        reversePath = reverse;

        if (path.Length > 0)
            lerpMovement.GoTo(path[currentPointIndex], false, velocity > 0 ? velocity : defaultVelocity);
    }

}
