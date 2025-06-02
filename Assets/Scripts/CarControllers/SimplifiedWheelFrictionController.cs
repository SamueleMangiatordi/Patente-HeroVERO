using UnityEngine;

    public class SimplifiedWheelFrictionController : MonoBehaviour
    {
        [Header("Ezereal References")]
        [SerializeField] SimplifiedCarController simplifiedCarController;

        WheelFrictionCurve fLWSidewaysFriction;
        WheelFrictionCurve fRWSidewaysFriction;
        WheelFrictionCurve rLWSidewaysFriction;
        WheelFrictionCurve rRWSidewaysFriction;

        WheelFrictionCurve fLWForwardFriction;
        WheelFrictionCurve fRWForwardFriction;
        WheelFrictionCurve rLWForwardFriction;
        WheelFrictionCurve rRWForwardFriction;

        void Start()
        {
            if (simplifiedCarController != null)
            {
                SetForwardFriction();
                SetSidewaysFriction();
            }
            else
            {
                Debug.LogWarning("ezerealWheelFrictionController is missing ezerealCarController. Ignore it or attach one if you want to have friction controls.");
            }

        }

        void SetForwardFriction()
        {
            fLWForwardFriction = new WheelFrictionCurve
            {
                extremumSlip = simplifiedCarController.frontLeftWheelCollider.forwardFriction.extremumSlip,
                extremumValue = simplifiedCarController.frontLeftWheelCollider.forwardFriction.extremumValue,
                asymptoteSlip = simplifiedCarController.frontLeftWheelCollider.forwardFriction.asymptoteSlip,
                asymptoteValue = simplifiedCarController.frontLeftWheelCollider.forwardFriction.asymptoteValue,
                stiffness = simplifiedCarController.frontLeftWheelCollider.forwardFriction.stiffness
            };

            fRWForwardFriction = new WheelFrictionCurve
            {
                extremumSlip = simplifiedCarController.frontRightWheelCollider.forwardFriction.extremumSlip,
                extremumValue = simplifiedCarController.frontRightWheelCollider.forwardFriction.extremumValue,
                asymptoteSlip = simplifiedCarController.frontRightWheelCollider.forwardFriction.asymptoteSlip,
                asymptoteValue = simplifiedCarController.frontRightWheelCollider.forwardFriction.asymptoteValue,
                stiffness = simplifiedCarController.frontRightWheelCollider.forwardFriction.stiffness
            };

            rLWForwardFriction = new WheelFrictionCurve
            {
                extremumSlip = simplifiedCarController.rearLeftWheelCollider.forwardFriction.extremumSlip,
                extremumValue = simplifiedCarController.rearLeftWheelCollider.forwardFriction.extremumValue,
                asymptoteSlip = simplifiedCarController.rearLeftWheelCollider.forwardFriction.asymptoteSlip,
                asymptoteValue = simplifiedCarController.rearLeftWheelCollider.forwardFriction.asymptoteValue,
                stiffness = simplifiedCarController.rearLeftWheelCollider.forwardFriction.stiffness
            };

            rRWForwardFriction = new WheelFrictionCurve
            {
                extremumSlip = simplifiedCarController.rearRightWheelCollider.forwardFriction.extremumSlip,
                extremumValue = simplifiedCarController.rearRightWheelCollider.forwardFriction.extremumValue,
                asymptoteSlip = simplifiedCarController.rearRightWheelCollider.forwardFriction.asymptoteSlip,
                asymptoteValue = simplifiedCarController.rearRightWheelCollider.forwardFriction.asymptoteValue,
                stiffness = simplifiedCarController.rearRightWheelCollider.forwardFriction.stiffness
            };
        }

        void SetSidewaysFriction()
        {
            fLWSidewaysFriction = new WheelFrictionCurve
            {
                extremumSlip = simplifiedCarController.frontLeftWheelCollider.sidewaysFriction.extremumSlip,
                extremumValue = simplifiedCarController.frontLeftWheelCollider.sidewaysFriction.extremumValue,
                asymptoteSlip = simplifiedCarController.frontLeftWheelCollider.sidewaysFriction.asymptoteSlip,
                asymptoteValue = simplifiedCarController.frontLeftWheelCollider.sidewaysFriction.asymptoteValue,
                stiffness = simplifiedCarController.frontLeftWheelCollider.sidewaysFriction.stiffness
            };

            fRWSidewaysFriction = new WheelFrictionCurve
            {
                extremumSlip = simplifiedCarController.frontRightWheelCollider.sidewaysFriction.extremumSlip,
                extremumValue = simplifiedCarController.frontRightWheelCollider.sidewaysFriction.extremumValue,
                asymptoteSlip = simplifiedCarController.frontRightWheelCollider.sidewaysFriction.asymptoteSlip,
                asymptoteValue = simplifiedCarController.frontRightWheelCollider.sidewaysFriction.asymptoteValue,
                stiffness = simplifiedCarController.frontRightWheelCollider.sidewaysFriction.stiffness
            };

            rLWSidewaysFriction = new WheelFrictionCurve
            {
                extremumSlip = simplifiedCarController.rearLeftWheelCollider.sidewaysFriction.extremumSlip,
                extremumValue = simplifiedCarController.rearLeftWheelCollider.sidewaysFriction.extremumValue,
                asymptoteSlip = simplifiedCarController.rearLeftWheelCollider.sidewaysFriction.asymptoteSlip,
                asymptoteValue = simplifiedCarController.rearLeftWheelCollider.sidewaysFriction.asymptoteValue,
                stiffness = simplifiedCarController.rearLeftWheelCollider.sidewaysFriction.stiffness
            };

            rRWSidewaysFriction = new WheelFrictionCurve
            {
                extremumSlip = simplifiedCarController.rearRightWheelCollider.sidewaysFriction.extremumSlip,
                extremumValue = simplifiedCarController.rearRightWheelCollider.sidewaysFriction.extremumValue,
                asymptoteSlip = simplifiedCarController.rearRightWheelCollider.sidewaysFriction.asymptoteSlip,
                asymptoteValue = simplifiedCarController.rearRightWheelCollider.sidewaysFriction.asymptoteValue,
                stiffness = simplifiedCarController.rearRightWheelCollider.sidewaysFriction.stiffness
            };
        }

        public void StartDrifting(float currentHandbrakeValue)
        {
            if (simplifiedCarController != null)
            {
                //use if you need to

                //rLwheelForwardFriction.extremumSlip = 
                //rRwheelForwardFriction.extremumSlip = 
                //rLwheelForwardFriction.extremumValue = 
                //rRwheelForwardFriction.extremumValue = 

                rLWSidewaysFriction.extremumSlip = 3f * currentHandbrakeValue;
                rRWSidewaysFriction.extremumSlip = 3f * currentHandbrakeValue;
                rLWSidewaysFriction.extremumValue = 0.7f * currentHandbrakeValue;
                rRWSidewaysFriction.extremumValue = 0.7f * currentHandbrakeValue;

                //Debug.Log(rLWSidewaysFriction.extremumSlip.ToString());

                //ezerealCarController.rearLeftWheelCollider.forwardFriction = rLwheelForwardFriction;
                //ezerealCarController.rearRightWheelCollider.forwardFriction = rRwheelForwardFriction;

                simplifiedCarController.rearLeftWheelCollider.sidewaysFriction = rLWSidewaysFriction;
                simplifiedCarController.rearRightWheelCollider.sidewaysFriction = rRWSidewaysFriction;
            }
        }

        public void StopDrifting()
        {
            if (simplifiedCarController != null)
            {
                //use if you need to

                //rLwheelForwardFriction.extremumSlip = 
                //rRwheelForwardFriction.extremumSlip = 
                //rLwheelForwardFriction.extremumValue = 
                //rRwheelForwardFriction.extremumValue = 

                //Set default value here
                rLWSidewaysFriction.extremumSlip = 0.2f;
                rRWSidewaysFriction.extremumSlip = 0.2f;
                rLWSidewaysFriction.extremumValue = 1f;
                rRWSidewaysFriction.extremumValue = 1f;

                //Debug.Log(rLWSidewaysFriction.extremumSlip.ToString());

                //ezerealCarController.rearLeftWheelCollider.forwardFriction = rLwheelForwardFriction;
                //ezerealCarController.rearRightWheelCollider.forwardFriction = rRwheelForwardFriction;

                simplifiedCarController.rearLeftWheelCollider.sidewaysFriction = rLWSidewaysFriction;
                simplifiedCarController.rearRightWheelCollider.sidewaysFriction = rRWSidewaysFriction;
            }
        }
    }
