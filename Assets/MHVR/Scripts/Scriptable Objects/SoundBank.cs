using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Sound Bank", menuName = "Sound Bank")]
public class SoundBank : ScriptableObject
{
    [System.Serializable]
    public class Audio
    {
        public AudioClip clip;
        public string description;
    }
    
    public string description;
    public Audio[] audio;
}
