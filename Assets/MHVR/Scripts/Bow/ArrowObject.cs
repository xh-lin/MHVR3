using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ArrowObject : MonoBehaviour
{
    public AudioBank bowVisualSFX;

    private AudioSource audioSource;
    private VRTK_InteractableObject interact;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        interact = GetComponent<VRTK_InteractableObject>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        var handle = collider.GetComponentInParent<BowHandle>();
        NockingArrow(handle);
    }

    /// <summary>
    /// Attach arrow to bow's nocking point.
    /// </summary>
    /// <param name="handle"></param>
    public void NockingArrow(BowHandle handle)
    {
        if (handle && !handle.aim.HasArrow() && handle.aim.IsHeld() && interact && interact.IsGrabbed())
        {
            transform.SetParent(handle.arrowNockingPoint);
            transform.localPosition = Vector3.zero;
            handle.aim.SetArrow(gameObject);
        }
    }

    // === Audio

    public void PlayShootAudio(int chargeLevel, float volumeScale)
    {
        int idx;
        switch (chargeLevel)
        {
            case 1:
                idx = 12;
                break;
            case 2:
                idx = 59;
                break;
            default:
                idx = 11;
                break;
        }
        AudioSource.PlayClipAtPoint(bowVisualSFX.audios[idx].clip, transform.position, volumeScale);
    }

    public void PlayPowerShotAudio(int chargeLevel, float volumeScale)
    {
        AudioSource.PlayClipAtPoint(bowVisualSFX.audios[28].clip, transform.position, volumeScale);
        if (chargeLevel > 1)
        {
            AudioSource.PlayClipAtPoint(bowVisualSFX.audios[63].clip, transform.position, volumeScale);
        }
    }

    public void PlayAirConeAudio(float volumeScale)
    {
        audioSource.clip = bowVisualSFX.audios[19].clip;
        audioSource.loop = true;
        audioSource.volume = volumeScale;
        audioSource.Play();
    }

    public void StopAudio()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    // ===
}
