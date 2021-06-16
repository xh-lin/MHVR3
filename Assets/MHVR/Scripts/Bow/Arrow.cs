// Mofidied from VRTK.Examples.Archery.Arrow

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float maxArrowLife = 10f;
    public float maxCollidedLife = 1f;
    public SoundBank bowVisualSFX;

    private bool isCollided = false;
    private bool inFlight = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private AudioSource source;
    private Rigidbody rigidBody;
    private GameObject arrowHolder;

    private int[] hitDirtSounds;
    private int hitDirtSoundsIdx;

    [HideInInspector]
    public ParticleSystem ps;

    private void Awake()
    {
        SetOrigns();
        source = GetComponent<AudioSource>();
        rigidBody = GetComponent<Rigidbody>();
        ps = GetComponentInChildren<ParticleSystem>();
        ps.Stop();

        hitDirtSounds = new int[] { 14, 18, 52, 55 };
        hitDirtSoundsIdx = 0;
    }

    private void FixedUpdate()
    {
        if (inFlight && !isCollided)
        {
            transform.LookAt(transform.position + rigidBody.velocity);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (inFlight && isActiveAndEnabled && gameObject.activeInHierarchy)
        {
            StopSound();
            PlayHitSound(0.3f);
            ps.Stop();
            ResetArrow();
        }
    }

    // === Play sound

    public void PlayShotSound(int chargeLevel, float volumeScale)
    {
        switch (chargeLevel)
        {
            case 1:
                source.PlayOneShot(bowVisualSFX.audio[12].clip, volumeScale);
                break;
            case 2:
                source.PlayOneShot(bowVisualSFX.audio[59].clip, volumeScale * 5f);
                break;
            case 3:
            default:
                source.PlayOneShot(bowVisualSFX.audio[11].clip, volumeScale * 5f);
                break;
        }
    }

    public void PlaySpreadShotSound(int chargeLevel, float volumeScale)
    {
        switch (chargeLevel)
        {
            case 1:
                source.PlayOneShot(bowVisualSFX.audio[28].clip, volumeScale);
                break;
            case 2:
            default:
                source.PlayOneShot(bowVisualSFX.audio[28].clip, volumeScale * 5f);
                source.PlayOneShot(bowVisualSFX.audio[63].clip, volumeScale * 5f);
                break;
        }
    }

    public void PlayHitSound(float volumeScale)
    {
        source.PlayOneShot(bowVisualSFX.audio[hitDirtSounds[hitDirtSoundsIdx]].clip, volumeScale);
        hitDirtSoundsIdx = (hitDirtSoundsIdx + 1) % hitDirtSounds.Length;
    }

    public void PlayAirConeSound(float volumeScale)
    {
        source.clip = bowVisualSFX.audio[19].clip;
        source.loop = true;
        source.volume = volumeScale;
        source.Play();
    }

    public void StopSound()
    {
        if (source.isPlaying)
        {
            source.Stop();
        }
    }

    // ===

    public void SetArrowHolder(GameObject holder)
    {
        arrowHolder = holder;
        arrowHolder.SetActive(false);
    }

    public void OnNock()
    {
        isCollided = false;
        inFlight = false;
    }

    public void InFlight()
    {
        inFlight = true;
    }

    public void Fired(Color color)
    {
        if (color != Color.clear)
        {
            var main = ps.main;
            main.startColor = color;
            ps.Play();
        }
        DestroyArrow(maxArrowLife);
    }

    public void ResetArrow()
    {
        DestroyArrow(maxCollidedLife);
        isCollided = true;
        inFlight = false;
        RecreateNotch();
        ResetTransform();
    }

    

    // === helper functions

    private void SetOrigns()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        originalScale = transform.localScale;
    }

    private void RecreateNotch()
    {
        //swap the arrow holder to be the parent again
        arrowHolder.transform.SetParent(null);
        arrowHolder.SetActive(true);

        //make the arrow a child of the holder again
        transform.SetParent(arrowHolder.transform);

        //reset the state of the rigidbodies and colliders
        rigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rigidBody.isKinematic = true;
        GetComponent<Collider>().enabled = false;
        arrowHolder.GetComponent<Rigidbody>().isKinematic = false;
    }

    private void ResetTransform()
    {
        arrowHolder.transform.position = transform.position;
        arrowHolder.transform.rotation = transform.rotation;
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        transform.localScale = originalScale;
    }

    private void DestroyArrow(float time)
    {
        Destroy(arrowHolder, time);
        Destroy(gameObject, time);
    }

    // ===
}
