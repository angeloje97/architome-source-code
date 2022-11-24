using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using Architome.Enums;

public class AudioMixerGroups : MonoBehaviour
{
    public static AudioMixerGroups active;

    public Dictionary<AudioMixerType, AudioMixerGroup> audioTypeDict;

    [SerializeField] List<MixerGroupPair> mixerGroupPairs;
    private void Awake()
    {
        active = this;
        CreateGroups();
    }
    
    void CreateGroups()
    {
        if (mixerGroupPairs == null) return;
        audioTypeDict = new();

        foreach(var pair in mixerGroupPairs)
        {
            audioTypeDict.Add(pair.mixerType, pair.mixerGroup);
        }
    }

    private void OnValidate()
    {
        if (!update) return;
        update = false;

        UpdateGroupPairs();

        void UpdateGroupPairs()
        {
            if (mixerGroupPairs == null) return;

            foreach(var pair in mixerGroupPairs)
            {
                pair.name = pair.mixerType.ToString();
            }
        }
    }

    public AudioMixerGroup MixerGroup(AudioMixerType mixerType)
    {
        if (!audioTypeDict.ContainsKey(mixerType)) return null;
        return audioTypeDict[mixerType];
    }


    public AudioMixerGroup SoundEffect;
    public AudioMixerGroup Ambience;
    public AudioMixerGroup Music;
    public AudioMixerGroup Voice;
    public AudioMixerGroup UI;

    [SerializeField] bool update;

    [Serializable]
    public class MixerGroupPair
    {
        [HideInInspector]public string name;
        public AudioMixerType mixerType;
        public AudioMixerGroup mixerGroup;

    }
}
