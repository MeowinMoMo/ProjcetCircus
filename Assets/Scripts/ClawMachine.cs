using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClawMachine : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;

    [Header("Claw Boundaries")]
    public Vector2 xLimits = new Vector2(-2f, 2f);
    public Vector2 zLimits = new Vector2(-2f, 2f);

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        // Get input
        float h = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxis("Vertical");   // W/S or Up/Down

        // Calculate movement
        Vector3 move = new Vector3(h, 0f, v) * moveSpeed * Time.deltaTime;

        // Apply movement
        transform.position += move;

        // Clamp within bounds
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, xLimits.x, xLimits.y),
            transform.position.y, // keep current height
            Mathf.Clamp(transform.position.z, zLimits.x, zLimits.y)
        );
    }

}
