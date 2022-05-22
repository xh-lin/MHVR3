using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Bow : MonoBehaviour
{
    [Range(0f, 1f)]
    public float touchVibration = 0.2f;
    [Tooltip("In seconds.")]
    public float vibrationDuration = 0.2f;
    public AudioBank weaponSFX;
    public AudioBank bowPhysicalSFX;
    public AudioBank bowVisualSFX;

    private BowAim aim;
    private AudioSource audioSource;
    private Animator animator;
    private new Rigidbody rigidbody;
    private Renderer[] renderers;
    private VRTK_InteractableObject interact;
    private Outline outline;

    private readonly int[] kSetArrowAudios = { 11, 12, 21 };

    private void Awake()
    {
        aim = GetComponent<BowAim>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        renderers = GetComponentsInChildren<Renderer>();
        interact = GetComponent<VRTK_InteractableObject>();
        outline = GetComponent<Outline>();

        // set to folded animation
        animator.enabled = true;
        rigidbody.isKinematic = true;
        outline.enabled = false;
    }

    private void Start()
    {
        animator.enabled = false;
        rigidbody.isKinematic = false;

        interact.InteractableObjectTouched += Touch;
        interact.InteractableObjectUntouched += Untouch;
        interact.InteractableObjectGrabbed += Grab;
        interact.InteractableObjectUngrabbed += Ungrab;
    }

    public bool IsFolded()
    {
        return animator.GetBool("isFolded");
    }

    public void SetPullAnimation(float blend)
    {
        animator.SetFloat("PullBlend", blend);
    }

    // === Glow

    public void StopGlow()
    {
        StopAllCoroutines();
        GlowSetMuliplier(0);
        GlowBreath(false);
    }

    public void GlowPulse(float minMult, float maxMult, float interpolation, bool breathAfter)
    {
        StartCoroutine(GlowPulseCoroutine(minMult, maxMult, interpolation, breathAfter));
    }

    private IEnumerator GlowPulseCoroutine(float minMult, float maxMult, float interpolation, bool breathAfter)
    {
        float lerpVal = 0;
        while (lerpVal < 1)
        {
            lerpVal += interpolation * Time.deltaTime;
            GlowSetMuliplier(Mathf.Lerp(minMult, maxMult, lerpVal));
            yield return null;
        }
        while (lerpVal > 0)
        {
            lerpVal -= interpolation * Time.deltaTime;
            GlowSetMuliplier(Mathf.Lerp(minMult, maxMult, lerpVal));
            yield return null;
        }
        GlowBreath(breathAfter);
    }

    private void GlowSetMuliplier(float mult)
    {
        foreach (var renderer in renderers)
        {
            renderer.material.SetFloat("_glowColorMultiplier", mult);
        }
    }

    private void GlowBreath(bool b)
    {
        foreach (var renderer in renderers)
        {
            renderer.material.SetInt("_isBreath", b ? 1 : 0);
        }
    }

    // === Audio

    public void PlayStringStretchAudio(float volumeScale)
    {
        audioSource.PlayOneShot(weaponSFX.audios[1].clip, volumeScale);
    }

    public void PlaySheathAudio(float volumeScale)
    {
        audioSource.PlayOneShot(weaponSFX.audios[3].clip, volumeScale);
    }

    public void PlayFoldAudio(float volumeScale)
    {
        audioSource.PlayOneShot(weaponSFX.audios[5].clip, volumeScale);
    }

    public void PlayOpenAudio(float volumeScale)
    {
        audioSource.PlayOneShot(weaponSFX.audios[6].clip, volumeScale);
    }

    public void PlayShotAudio(float volumeScale)
    {
        var shotAudio = Random.value > 0.5f ? 2 : 4;
        audioSource.PlayOneShot(weaponSFX.audios[shotAudio].clip, volumeScale);
    }

    public void PlaySetArrowAudio(float volumeScale)
    {
        var setArrowAudio = kSetArrowAudios[Random.Range(0, kSetArrowAudios.Length)];
        audioSource.PlayOneShot(bowPhysicalSFX.audios[setArrowAudio].clip, volumeScale);
    }

    public void PlayPullAudio(int chargeLevel, float volumePhysicalSFX, float volumeVisualSFX)
    {
        switch (chargeLevel)
        {
            case 1:
                var pullAudioOne = Random.value > 0.5f ? 3 : 24;
                audioSource.PlayOneShot(bowPhysicalSFX.audios[pullAudioOne].clip, volumePhysicalSFX);
                break;
            case 2:
                audioSource.PlayOneShot(bowPhysicalSFX.audios[18].clip, volumePhysicalSFX);
                audioSource.PlayOneShot(bowVisualSFX.audios[51].clip, volumeVisualSFX);
                break;
            case 3:
            default:
                audioSource.PlayOneShot(bowVisualSFX.audios[42].clip, volumeVisualSFX);
                break;
        }
        PlayPullHoldAudio(volumePhysicalSFX);
    }

    public void PlayPullHoldAudio(float volumeScale)
    {
        audioSource.clip = bowPhysicalSFX.audios[22].clip;
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

    public void ToggleFold()
    {
        if (IsFolded())
        {
            animator.SetBool("isFolded", false);    // Open
        }
        else if (!aim.HasArrow())
        {
            animator.SetBool("isFolded", true);     // Fold
        }
    }

    // === VRTK_InteractableObject event callbacks.

    private IEnumerator HapticPulse(float intensity, float duration, bool isRHand)
    {
        if (isRHand)
        {
            OVRInput.SetControllerVibration(.1f, intensity, OVRInput.Controller.RTouch);
        }
        else
        {
            OVRInput.SetControllerVibration(.1f, intensity, OVRInput.Controller.LTouch);
        }

        yield return new WaitForSeconds(duration);

        if (isRHand)
        {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        }
        else
        {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        }
    }

    protected virtual void Touch(object sender, InteractableObjectEventArgs e)
    {
        if (interact.IsInSnapDropZone())
        {
            outline.enabled = true;
            foreach (GameObject goTouching in interact.GetTouchingObjects())
            {
                if (VRTK_DeviceFinder.IsControllerRightHand(goTouching))
                {
                    StartCoroutine(HapticPulse(touchVibration, vibrationDuration, true));
                }
                else if (VRTK_DeviceFinder.IsControllerLeftHand(goTouching))
                {
                    StartCoroutine(HapticPulse(touchVibration, vibrationDuration, false));
                }
            }
        }
    }

    protected virtual void Untouch(object sender, InteractableObjectEventArgs e)
    {
        outline.enabled = false;
    }

    protected virtual void Grab(object sender, InteractableObjectEventArgs e)
    {
        outline.enabled = false;
        animator.enabled = true;
    }

    protected virtual void Ungrab(object sender, InteractableObjectEventArgs e)
    {
        SetPullAnimation(0f);
        animator.enabled = false;
    }

    // ===
}
