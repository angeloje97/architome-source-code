using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;
using Architome.Effects;
using Architome;

[CreateAssetMenu(fileName = "New EntityFX Pack", menuName = "Architome/Entity/EntityFX")]
public class EntityFXPack : ScriptableObject
{
    public EffectsHandler<EntityEvent, EntityFXHandler.FXData> effectsHandler;

    

    [Serializable]
    public class EntityEffect
    {
        [HideInInspector] public string name;
        public EntityEvent trigger;
        [Header("Particles")]
        public GameObject particleEffect;
        public BodyPart bodyPart;
        public BodyPart bodyPart2;
        public CatalystParticleTarget target;
        public Vector3 positionOffset, scaleOffset, rotationOffset;

        [Header("Phrases")]
        [Multiline]
        public List<string> phrases;
        public SpeechType phraseType;

        [Header("Audio")]
        public AudioMixerType audioType;
        public AudioClip audioClip;
        public List<AudioClip> randomClips;


        public bool useChance;
        [Range(0, 100)]
        public float chance;
        public float coolDown;

        public void Update()
        {
            name = trigger.ToString();
        }
    }


    [SerializeField] bool update;
    private void OnValidate()
    {
        if (!update) return;
        update = false;

        foreach(var effect in effects)
        {
            effect.Update();
        }

        effectsHandler?.Validate();
    }

    public List<EntityEffect> effects;
}
