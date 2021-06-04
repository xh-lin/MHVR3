using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [Range(0.0f, 20.0f)]
    public float speed = 10f;
    [Range(0.0f, 3.0f)]
    public float shakeMultiplier = 0.5f;
    [Range(0.0f, 5.0f)]
    public float shakeDuration = 1f;

    private float multiplier;
    private float seedX;
    private float seedY;

    private void Start()
    {
        multiplier = 0f;
        seedX = Random.value * 10f;
        seedY = Random.value * 10f;
    }

    private void FixedUpdate()
    {
        if (multiplier > 0.01f)
        {
            var x = (Mathf.PerlinNoise(seedX, Time.time * speed) - 0.5f) * multiplier;
            var y = (Mathf.PerlinNoise(seedY, Time.time * speed) - 0.5f) * multiplier;
            transform.localPosition = new Vector3(x, y, 0);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 8) // Layer 8 = Projectile
        {
            StopCoroutine(nameof(ShakePulseCoroutine));
            StartCoroutine(ShakePulseCoroutine(shakeMultiplier, shakeDuration));
        }
    }

    private IEnumerator ShakePulseCoroutine(float maxMult, float duration)
    {
        float upDuration = duration / 3;
        float downDuration = duration * 2 / 3;
        float startTime = Time.time;
        float t = 0f;
        while (t < 1f)
        {
            t = (Time.time - startTime) / upDuration;
            multiplier = Mathf.Lerp(0f, maxMult, t);
            yield return null;
        }

        startTime = Time.time;
        t = 0f;
        while (t < 1f)
        {
            t = (Time.time - startTime) / downDuration;
            multiplier = Mathf.SmoothStep(maxMult, 0f, t);
            yield return null;
        }

        multiplier = 0f;
        transform.localPosition = Vector3.zero;
    }
}
