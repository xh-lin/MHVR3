using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    public Vector3 movementAxis;
    [Range (0f, 50f)]
    public float distance;
    [Range (0f, 10f)]
    public float speed = 0.1f;

    private float lerpVal;
    private Vector3 endPosition;

    private void Start()
    {
        lerpVal = 0f;
        endPosition = movementAxis.normalized * distance;
    }

    private void FixedUpdate()
    {
        lerpVal += speed * Time.deltaTime;
        transform.localPosition = Vector3.Lerp(Vector3.zero, endPosition, lerpVal);
        if (lerpVal >= 1f || lerpVal <= 0f)
            speed = -speed;
    }
}
