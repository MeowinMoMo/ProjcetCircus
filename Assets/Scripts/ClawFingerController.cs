using System.Collections.Generic;
using UnityEngine;

public class ClawFingerController : MonoBehaviour
{
    [Header("Finger Pivots (assign the pivot objects)")]
    public List<Transform> fingerPivots = new List<Transform>();

    [Header("Finger Rotation Settings")]
    public float openAngle = 20f;     // positive Z
    public float neutralAngle = 0f;   // neutral Z
    public float closeAngle = -23f;   // negative Z
    public float fingerSpeed = 6f;

    [Header("Claw State")]
    public bool isNeutral = true;
    public bool isOpen = false;
    public bool isClose = false;

    private List<Vector2> cachedXY = new List<Vector2>();
    private List<float> currentZ = new List<float>();

    void Start()
    {
        cachedXY.Clear();
        currentZ.Clear();

        foreach (var finger in fingerPivots)
        {
            if (finger == null)
            {
                cachedXY.Add(Vector2.zero);
                currentZ.Add(0f);
                continue;
            }
            Vector3 euler = finger.localEulerAngles;
            cachedXY.Add(new Vector2(euler.x, euler.y));

            float z = euler.z > 180f ? euler.z - 360f : euler.z;
            currentZ.Add(z);
        }

        SetFingersInstant();
    }

    void Update()
    {
        UpdateFingers();
    }

    void UpdateFingers()
    {
        float targetZ = GetTargetZ();

        for (int i = 0; i < fingerPivots.Count; i++)
        {
            if (fingerPivots[i] == null) continue;

            // Smoothly move currentZ towards targetZ
            currentZ[i] = Mathf.MoveTowards(currentZ[i], targetZ, fingerSpeed * Time.deltaTime);

            fingerPivots[i].localEulerAngles = new Vector3(
                cachedXY[i].x,
                cachedXY[i].y,
                currentZ[i]
            );
        }
    }

    float GetTargetZ()
    {
        if (isOpen) return openAngle;
        if (isClose) return closeAngle;
        return neutralAngle; // default
    }

    // Instantly set fingers to current state
    public void SetFingersInstant()
    {
        float targetZ = GetTargetZ();

        for (int i = 0; i < fingerPivots.Count; i++)
        {
            if (fingerPivots[i] == null) continue;

            currentZ[i] = targetZ;
            fingerPivots[i].localEulerAngles = new Vector3(
                cachedXY[i].x,
                cachedXY[i].y,
                targetZ
            );
        }
    }

    // Helper method to switch states
    public void SetState(bool open, bool neutral, bool close)
    {
        isOpen = open;
        isNeutral = neutral;
        isClose = close;
    }

    // Optional: cycle through states manually
    public void CycleState()
    {
        if (isNeutral) SetState(true, false, false);     // neutral → open
        else if (isOpen) SetState(false, false, true);   // open → close
        else SetState(false, true, false);               // close → neutral
    }
}
