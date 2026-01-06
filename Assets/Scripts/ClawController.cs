using System.Collections;
using UnityEngine;

public class ClawController : MonoBehaviour
{
    [Header("Player Movement")]
    public float moveSpeed = 4f;
    public Vector2 xLimits = new Vector2(-5f, 5f);
    public Vector2 zLimits = new Vector2(-5f, 5f);

    [Header("Vertical Movement")]
    public float dropDistance = 5f;
    public float liftHeight = 5f;
    public float verticalSpeed = 5f;

    [Header("Drop / Shoot Area")]
    public Transform dropArea;
    public float horizontalSpeed = 3f;

    [Header("Finger Controller")]
    public ClawFingerController fingerController;

    [Header("Swing Settings")]
    public Rigidbody clawRb;
    public float swingForce = 1.2f;
    public float swingDamping = 0.98f;

    bool isSequenceRunning;
    Vector3 startPosition;

    public ClawSwing clawSwing;
    void Start()
    {
        startPosition = transform.position;
        fingerController.SetState(false, true, false); // Neutral
    }

    void Update()
    {
        if (!isSequenceRunning)
        {
            HandlePlayerMovement();

            if (Input.GetKeyDown(KeyCode.Space))
                StartCoroutine(ClawSequence());
        }
    }

    void HandlePlayerMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0f, v) * moveSpeed * Time.deltaTime;
        transform.position += move;

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, xLimits.x, xLimits.y),
            transform.position.y,
            Mathf.Clamp(transform.position.z, zLimits.x, zLimits.y)
        );
    }

    IEnumerator ClawSequence()
    {
        isSequenceRunning = true;
        clawSwing.EnableSwing(false);
        float startY = transform.position.y;
        float dropY = startY - dropDistance;
        
        


        // 1️⃣ OPEN FIRST
        fingerController.SetState(true, false, false);
        yield return WaitFinger();

        // 2️⃣ DROP (Y ONLY + SWING)
        while (transform.position.y > dropY)
        {
            float newY = Mathf.MoveTowards(
                transform.position.y,
                dropY,
                verticalSpeed * Time.deltaTime
            );

            transform.position = new Vector3(
                transform.position.x,
                newY,
                transform.position.z
            );

            // Apply small random horizontal swing force
            Vector3 swingDir = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            );

            clawRb.AddForce(swingDir * swingForce, ForceMode.Acceleration);

            yield return null;
        }


        // 🔒 FORCE exact drop Y
        transform.position = new Vector3(
            transform.position.x,
            dropY,
            transform.position.z
        );

        // 3️⃣ CLOSE (ONLY AFTER FULL DROP)
        fingerController.SetState(false, false, true);
        yield return WaitFinger();

        StartCoroutine(DampenSwing());

        // 4️⃣ LIFT (FROM CURRENT POSITION)
        while (transform.position.y < liftHeight)
        {
            float newY = Mathf.MoveTowards(
                transform.position.y,
                liftHeight,
                verticalSpeed * Time.deltaTime
            );

            transform.position = new Vector3(
                transform.position.x,
                newY,
                transform.position.z
            );

            yield return null;
        }

        // 5️⃣ MOVE TO DROP AREA (HORIZONTAL ONLY)
        Vector3 shootTarget = new Vector3(
            dropArea.position.x,
            liftHeight,
            dropArea.position.z

        );

        yield return MoveHorizontal(shootTarget);

        // 6️⃣ OPEN TO RELEASE
        fingerController.SetState(true, false, false);
        yield return WaitFinger();

        // 7️⃣ RETURN TO START (XY)
        Vector3 returnTarget = new Vector3(
            startPosition.x,
            liftHeight,
            startPosition.z
        );

        yield return MoveHorizontal(returnTarget);

        // 8️⃣ LOWER TO START HEIGHT
        while (transform.position.y > startPosition.y)
        {
            float newY = Mathf.MoveTowards(
                transform.position.y,
                startPosition.y,
                verticalSpeed * Time.deltaTime
            );

            transform.position = new Vector3(
                transform.position.x,
                newY,
                transform.position.z
            );

            yield return null;
        }

        // 9️⃣ NEUTRAL
        fingerController.SetState(false, true, false);

        clawSwing.EnableSwing(true);
        isSequenceRunning = false;

    }
    IEnumerator DampenSwing()
    {
        while (clawRb.linearVelocity.magnitude > 0.05f)
        {
            clawRb.linearVelocity *= swingDamping;
            yield return null;
        }

        clawRb.linearVelocity = Vector3.zero;
    }
    IEnumerator MoveHorizontal(Vector3 target)
    {
        while (
            Mathf.Abs(transform.position.x - target.x) > 0.01f ||
            Mathf.Abs(transform.position.z - target.z) > 0.01f
        )
        {
            float x = Mathf.MoveTowards(
                transform.position.x,
                target.x,
                horizontalSpeed * Time.deltaTime
            );

            float z = Mathf.MoveTowards(
                transform.position.z,
                target.z,
                horizontalSpeed * Time.deltaTime
            );

            transform.position = new Vector3(x, transform.position.y, z);
            yield return null;
        }
    }

    IEnumerator WaitFinger()
    {
        // Wait until finger lerp finishes naturally
        yield return new WaitForSeconds(2.5f);
    }
}
