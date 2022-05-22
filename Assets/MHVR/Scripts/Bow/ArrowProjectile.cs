using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    public AudioBank bowVisualSFX;

    private new Rigidbody rigidbody;

    private bool isFlying = false;
    private readonly int[] kHitDirtAudios = { 14, 18, 52, 55 };

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (isFlying)
        {
            transform.LookAt(transform.position + rigidbody.velocity);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isFlying)
        {
            isFlying = false;
            PlayHitAudio(0.3f);
            Destroy(gameObject);
        }
    }

    public void Shooted(Color color, float life)
    {
        var particleSystem = GetComponent<ParticleSystem>();
        if (color != Color.clear)
        {
            var main = particleSystem.main;
            main.startColor = color;        // set particle color
        }
        else
        {
            particleSystem.Stop();
        }

        isFlying = true;
        Destroy(gameObject, life);
    }

    // === Audio

    private void PlayHitAudio(float volumeScale)
    {
        var hitDirtAudio = kHitDirtAudios[Random.Range(0, kHitDirtAudios.Length)];
        AudioSource.PlayClipAtPoint(bowVisualSFX.audios[hitDirtAudio].clip, transform.position, volumeScale);
    }

    // ===
}
