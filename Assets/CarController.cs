using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField] private float motorForce = 100f; // Force applied for forward/backward movement
    [SerializeField] private float turnSpeed = 50f; // Speed of turning
    [SerializeField] private float maxSpeed = 20f; // Maximum linear speed

    [SerializeField] private Rigidbody rb;

    private float _inputForward;
    private float _inputTurn;

    void Update()
    {
        // Get input in Update for responsiveness
        _inputForward = Input.GetAxis("Vertical"); // W and S keys
        _inputTurn = Input.GetAxis("Horizontal"); // A and D keys
    }

    public void FixedUpdate()
    {
        // Apply forward/backward force
        // We use transform.forward to ensure the force is applied relative to the car's orientation
        Vector3 forwardForce = _inputForward * motorForce * transform.forward;
        rb.AddForce(forwardForce, ForceMode.Acceleration); // Use ForceMode.Acceleration for continuous force over time

        // Limit maximum speed
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z); // Ignore Y-component for speed limit
        if (flatVelocity.magnitude > maxSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z); // Keep current Y velocity
        }

        // Apply turning
        // We use torque to rotate the car
        float turnTorque = _inputTurn * turnSpeed;
        rb.AddTorque(0f, turnTorque, 0f, ForceMode.Acceleration); // Apply torque around the Y-axis

        // Prevent unwanted rotation on X and Z axes (pitch and roll)
        // This is crucial for a stable car, but might need adjustment for advanced physics
        rb.angularVelocity = new Vector3(0, rb.angularVelocity.y, 0);
    }
}