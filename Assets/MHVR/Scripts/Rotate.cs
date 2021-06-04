using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public Vector3 rotateAxis;
    [Range (0f, 50f)]
    public float speed;

    private void FixedUpdate()
    {
        transform.Rotate(rotateAxis, speed * Time.deltaTime);
    }
}
