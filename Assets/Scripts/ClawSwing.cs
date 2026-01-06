using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ClawSwing : MonoBehaviour
{
    [Header("Swing Settings")]
    public float swingStrength = 6f;    // how strongly movement affects swing
    public float damping = 2f;          // slows swing over time
    public float maxAngle = 25f;        // max tilt in degrees

    private Rigidbody rb;
    private Vector3 lastPosition;
    private bool allowSwing = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (!allowSwing) return;

        // 1️⃣ Calculate player movement delta
        Vector3 delta = transform.position - lastPosition;
        lastPosition = transform.position;

        // 2️⃣ Apply torque opposite to movement for follow-through
        Vector3 torque = new Vector3(
            -delta.z * swingStrength,   // forward/back -> tilt X
            0f,
            delta.x * swingStrength     // left/right -> tilt Z
        );

        rb.AddTorque(torque, ForceMode.Acceleration);

        // 3️⃣ Apply damping to gradually return to neutral
        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, Time.fixedDeltaTime * damping);

        // 4️⃣ Clamp rotation to max angles
        Vector3 angles = transform.localEulerAngles;
        angles.x = ClampAngle(angles.x, -maxAngle, maxAngle);
        angles.z = ClampAngle(angles.z, -maxAngle, maxAngle);
        transform.localEulerAngles = new Vector3(angles.x, transform.localEulerAngles.y, angles.z);
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    public void EnableSwing(bool enable)
    {
        allowSwing = enable;
    }
}
