using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;

[CreateAssetMenu(fileName = "New EntityFX Pack", menuName = "Architome/Entity/EntityFX")]
public class EntityFXPack : ScriptableObject
{

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
        [Multiline]
        public List<string> phrases;
        public SpeechType phraseType;

        //AudioClip
        public AudioClip audioClip;
        public List<AudioClip> randomClips;


        public bool useChance;
        [Range(0, 100)]
        public float chance;
        public float coolDown;

        
    }

    public List<EntityEffect> effects;
}
