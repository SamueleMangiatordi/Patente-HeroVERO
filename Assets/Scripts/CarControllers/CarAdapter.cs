using Ezereal;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CarAdapter : MonoBehaviour
{
    public SimplifiedCarController carController;


    /// <summary>
    /// Simulates throttle input for the car. This directly sets the throttle value on the car controller.
    /// </summary>
    /// <param name="value">The throttle input value (clamped between -1 and 1).</param>
    public void SimulateThrottleInput(float value)
    {
        if (carController == null) return;
        carController.CurrentThrottleInput = Mathf.Clamp(value, -1, 1);
    }

    /// <summary>
    /// Simulates steer input for the car, setting the target steer angle.
    /// </summary>
    /// <param name="value">The steer input value (clamped between -1 and 1).</param>
    public void SimulateSteerInput(float value)
    {
        if (carController == null) return;
        // Sets the target steer angle on the car controller, which is then smoothed by its Steering() method.
        carController.TargetSteerAngle = Mathf.Clamp(value*carController.maxSteerAngle, -carController.maxSteerAngle, carController.maxSteerAngle);
    }

    /// <summary>
    /// Simulates handbrake input for the car.
    /// </summary>
    /// <param name="value">The handbrake input value (clamped between 0 and 1).</param>
    public void SimulateHandBrake(float value)
    {
        if (carController == null) return;
        carController.CurrentHandbrakeValue = Mathf.Clamp(value, 0, 1);
    }
}
