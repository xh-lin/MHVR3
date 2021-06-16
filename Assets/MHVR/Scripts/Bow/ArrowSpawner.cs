// Mofidied from VRTK.Examples.Archery.ArrowSpawner

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ArrowSpawner : MonoBehaviour
{
    [Tooltip("Spawn cool down in seconds.")]
    public float spawnDelay = 0f;
    [Range(0f, 1f)]
    public float vibrationIntensity = 0.2f;
    [Tooltip("In seconds.")]
    public float vibrationDuration = 0.2f;
    [Tooltip("Automatically load the arrow on to the bow.")]
    public bool autoNocking;
    [Tooltip("Necessary if Auto Nocking is true.")]
    public BowHandle handle;
    public GameObject arrowPrefab;
    public SoundBank bowPhysicalSFX;

    private AudioSource audioSource;
    private Outline outline;
    private Collider col;
    private float spawnDelayTimer;

    // cache grabbing controller
    private VRTK_InteractGrab grabbingController;
    private Collider controllerCol;

    // sound variables
    private int[] grabArrowSounds;
    private int grabArrowSoundsIdx;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        outline = GetComponent<Outline>();
        col = GetComponent<Collider>();

        outline.enabled = false;
        spawnDelayTimer = 0f;

        grabArrowSounds = new int[] { 5, 7, 16, 33 };
        grabArrowSoundsIdx = 0;
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

            audioSource.PlayOneShot(bowPhysicalSFX.audio[10].clip, 0.3f);
            StartCoroutine(HapticPulse(vibrationIntensity, vibrationDuration, VRTK_DeviceFinder.IsControllerRightHand(grabbingController.gameObject)));
        }
    }

    private void OnTriggerStay(Collider collider)
    {
        if (CanGrab(grabbingController) && NoArrowNotched(grabbingController.gameObject) && Time.time >= spawnDelayTimer)
        {
            // spawn an arrow on hand
            GameObject newArrow = Instantiate(arrowPrefab);
            newArrow.name = "ArrowClone";
            grabbingController.GetComponent<VRTK_InteractTouch>().ForceStopTouching();
            grabbingController.GetComponent<VRTK_InteractTouch>().ForceTouch(newArrow);
            grabbingController.AttemptGrab();

            spawnDelayTimer = Time.time + spawnDelay;
            PlayGrabSound(0.5f);

            if (autoNocking)
            {
                newArrow.GetComponent<ArrowNotch>().NockingArrow(handle);
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

    private bool NoArrowNotched(GameObject controller)
    {
        BowAim aim = null;
        if (VRTK_DeviceFinder.IsControllerLeftHand(controller))
        {
            GameObject controllerRightHand = VRTK_DeviceFinder.GetControllerRightHand(true);
            aim = controllerRightHand.GetComponentInChildren<BowAim>();
            if (aim == null)
            {
                aim = VRTK_DeviceFinder.GetModelAliasController(controllerRightHand).GetComponentInChildren<BowAim>();
            }
        }
        else if (VRTK_DeviceFinder.IsControllerRightHand(controller))
        {
            GameObject controllerLeftHand = VRTK_DeviceFinder.GetControllerLeftHand(true);
            aim = controllerLeftHand.GetComponentInChildren<BowAim>();
            if (aim == null)
            {
                aim = VRTK_DeviceFinder.GetModelAliasController(controllerLeftHand).GetComponentInChildren<BowAim>();
            }
        }

        return (aim == null || !aim.HasArrow());
    }

    // === Play sound

    private void PlayGrabSound(float volumeScale)
    {
        audioSource.PlayOneShot(bowPhysicalSFX.audio[grabArrowSounds[grabArrowSoundsIdx]].clip, volumeScale);
        grabArrowSoundsIdx = (grabArrowSoundsIdx + 1) % grabArrowSounds.Length;
    }

    // ===
}
