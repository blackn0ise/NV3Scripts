using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedASO : MonoBehaviour
{
    [SerializeField] internal AudioSource aso = default;

    internal void SetClip(AudioClip clip)
    {
        aso.clip = clip;
        aso.Play();
        Destroy(gameObject, clip.length);
    }

    internal void SetVolume(float volume)
    {
        aso.volume = volume;
    }
}
