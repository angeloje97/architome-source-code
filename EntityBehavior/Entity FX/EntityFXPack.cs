using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;

[CreateAssetMenu(fileName = "New EntityFX Pack", menuName = "EntityFX")]
public class EntityFXPack : ScriptableObject
{
    // Start is called before the first frame update
    [Header("Audio")]
    public List<AudioClip> detectedPlayerSounds;
    public List<AudioClip> hurtSounds;
    public List<AudioClip> levelUpSounds;
    public AudioClip footStepSound;
    public AudioClip levelUpSound;
    public AudioClip reviveSound;

    [Header("Particle Effects")]
    public GameObject leveUpParticles;
    public GameObject reviveParticles;

    [Header("Phrases")]
    public List<string> deathPhrases;
    public List<string> detectedPlayerPhrases;

    [Serializable]
    public struct EntityEffect
    {
        public EntityEvent trigger;
        //Particle Effect
        public GameObject particleEffect;
        public BodyPart bodyPart;
        public BodyPart bodyPart2;
        public CatalystParticleTarget target;
        public Vector3 positionOffset, scaleOffset, rotationOffset;

        //Phrases
        public List<string> phrases;
        public SpeechType phraseType;

        //AudioClip
        public AudioClip audioClip;
    }

    public List<EntityEffect> effects;
}
