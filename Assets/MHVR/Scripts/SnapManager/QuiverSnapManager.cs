using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class QuiverSnapManager : MonoBehaviour
{
    public VRTK_SnapDropZone VRTK_SnapDropZone;
    public SoundBank bowPhysicalSFX;

    private AudioSource source;
    private Collider objectCollider;

    private void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public void Snap()
    {
        objectCollider = VRTK_SnapDropZone.GetCurrentSnappedObject().GetComponentInChildren<MeshCollider>();
        objectCollider.isTrigger = true;
        PlaySheathSound(0.3f);
    }

    public void Unsnap()
    {
        objectCollider.isTrigger = false;
    }

    private void PlaySheathSound(float volumeScale)
    {
        source.PlayOneShot(bowPhysicalSFX.audio[10].clip, volumeScale);
    }
}
