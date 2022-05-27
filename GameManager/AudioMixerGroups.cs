using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerGroups : MonoBehaviour
{
    public static AudioMixerGroups active;

    private void Awake()
    {
        active = this;
    }

    public AudioMixerGroup SoundEffect;
    public AudioMixerGroup Ambience;
    public AudioMixerGroup Music;
    public AudioMixerGroup Voice;
    public AudioMixerGroup UI;
}
