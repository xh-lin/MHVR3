// Mofidied from VRTK.Examples.Archery.ArrowSpawner

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ArrowSpawner : MonoBehaviour
{
    [Tooltip("Spawn cool down in seconds.")]
    public float spawnDelay = 0.2f;
    [Range(0f, 1f)]
    public float vibrationIntensity = 0.2f;
    [Tooltip("In seconds.")]
    public float vibrationDuration = 0.2f;
    public GameObject arrowObjectPrefab;
    public AudioBank bowPhysicalSFX;
    [Tooltip("Automatically load the arrow on to the bow.")]
    public bool autoNocking;
    [Tooltip("Necessary if Auto Nocking is true.")]
    public BowHandle handle;

    private AudioSource audioSource;
    private Outline outline;
    private Collider col;
    private float spawnDelayTimer;

    // cache grabbing controller
    private VRTK_InteractGrab grabbingController;
    private Collider controllerCol;

    private readonly int[] kGrabAudios = { 5, 7, 16, 33 };

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        outline = GetComponent<Outline>();
        col = GetComponent<Collider>();

        outline.enabled = false;
    }

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

    private void OnTriggerEnter(Collider collider)
    {
        VRTK_InteractGrab grab = collider.gameObject.GetComponent<VRTK_InteractGrab>() ? collider.gameObject.GetComponent<VRTK_InteractGrab>() : collider.gameObject.GetComponentInParent<VRTK_InteractGrab>();

        if (grab)
        {
            outline.enabled = true;
            grabbingController = grab;
            controllerCol = collider;

            audioSource.PlayOneShot(bowPhysicalSFX.audios[10].clip, 0.3f);
            StartCoroutine(HapticPulse(vibrationIntensity, vibrationDuration, VRTK_DeviceFinder.IsControllerRightHand(grabbingController.gameObject)));
        }
    }

    private void OnTriggerStay(Collider collider)
    {
        if (CanGrab(grabbingController) && Time.time >= spawnDelayTimer)
        {
            // spawn an arrow on hand
            GameObject newArrow = Instantiate(arrowObjectPrefab);
            grabbingController.GetComponent<VRTK_InteractTouch>().ForceStopTouching();
            grabbingController.GetComponent<VRTK_InteractTouch>().ForceTouch(newArrow);
            grabbingController.AttemptGrab();

            spawnDelayTimer = Time.time + spawnDelay;
            PlayGrabAudio(0.5f);

            if (autoNocking)
            {
                newArrow.GetComponent<ArrowObject>().NockingArrow(handle);
            }
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (ReferenceEquals(collider, controllerCol))
        {
            outline.enabled = false;
            grabbingController = null;
            controllerCol = null;
        }
    }

    private void OnEnable()
    {
        col.enabled = true;
        outline.enabled = autoNocking;
    }

    private void OnDisable()
    {
        col.enabled = false;
        outline.enabled = false;
    }

    private bool CanGrab(VRTK_InteractGrab grabbingController)
    {
        return (grabbingController && grabbingController.GetGrabbedObject() == null && grabbingController.IsGrabButtonPressed());
    }

    // === Play sound

    private void PlayGrabAudio(float volumeScale)
    {
        var grabAudio = kGrabAudios[Random.Range(0, kGrabAudios.Length)];
        audioSource.PlayOneShot(bowPhysicalSFX.audios[grabAudio].clip, volumeScale);
    }

    // ===
}
