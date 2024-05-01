using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class PortalFXHandler : MonoActor
    {
        public enum PortalEvents
        {
            OnEnter,
            OnExit,
            OnAllPartyMembersInPortal,
        }

        [Serializable]
        public class PortalFX
        {
            public PortalEvents trigger;

            [Header("Particle")]
            public GameObject particle;
            public ParticleTarget target;
            public BodyPart bodyPart;
            public Vector3 positionOffset, scaleOffset, rotationOffset;

            [Header("Audio")]
            public AudioClip clip;
            [Range(0, 1)]
            public float volume = 1f;
        }

        public List<PortalFX> effects;
        public Dictionary<PortalEvents, List<PortalFX>> effectsMap;
        [SerializeField] PortalInfo portal;
        [SerializeField] ParticleManager particleManager;
        [SerializeField] AudioManager audioManager;

        EntityInfo currentEntity;

        void Start()
        {
            GetDependencies();
            UpdateEffectMap();
        }

        private void OnValidate()
        {
            portal = GetComponentInParent<PortalInfo>();
            particleManager = GetComponentInChildren<ParticleManager>();
            audioManager = GetComponentInChildren<AudioManager>();
        }

        void UpdateEffectMap()
        {
            effectsMap = new();

            foreach (var effect in effects)
            {
                if (effectsMap.ContainsKey(effect.trigger))
                {
                    effectsMap[effect.trigger].Add(effect);
                }
                else
                {
                    effectsMap.Add(effect.trigger, new() { effect });
                }
            }
        }

        // Update is called once per frame
        void GetDependencies()
        {
            if (portal == null)
            {
                portal = GetComponentInParent<PortalInfo>();

                if (portal == null) return;
            }

            ArchAction.Delay(() => {
                portal.AddListenerPortal(ePortalEvent.OnEnter, OnPortalEnter, this);
                portal.AddListenerPortal(ePortalEvent.OnExit, OnPortalExit, this);
                portal.AddListenerPortal(ePortalEvent.OnAllPartyMembersInside, OnAllPartyMembersInPortal, this);
            }, 3f);
        }

        private void OnAllPartyMembersInPortal(PortalEventData eventData)
        {
            HandleEffects(PortalEvents.OnAllPartyMembersInPortal);
        }

        void OnPortalEnter(PortalEventData eventData)
        {
            //var info = entity.GetComponent<EntityInfo>();
            var info = eventData.targetEntity;
            currentEntity = info;
            HandleEffects(PortalEvents.OnEnter);
            ClearEntity(info);
        }

        void OnPortalExit(PortalEventData eventData)
        {
            var info = eventData.targetEntity;
            currentEntity = info;
            HandleEffects(PortalEvents.OnExit);
            ClearEntity(info);
             
        }



        void ClearEntity(EntityInfo entity)
        {
            if (currentEntity != entity) return;
            currentEntity = null;
        }


        void HandleEffects(PortalEvents trigger)
        {
            if (!effectsMap.ContainsKey(trigger)) return;

            foreach (var effect in effectsMap[trigger])
            {
                HandleParticles(effect);
                HandleAudio(effect);
            }

        }

        void HandleParticles(PortalFX fx)
        {
            if (particleManager == null) return;
            var particle = fx.particle;
            if (particle == null) return;

            var (particleSys, particleObj) = particleManager.Play(particle, true);

            HandleTarget();
            HandleOffsets();

            void HandleTarget()
            {
                if (fx.target == ParticleTarget.BodyPart)
                {
                    var bodyParts = currentEntity.GetComponentInChildren<CharacterBodyParts>();
                    var trans = bodyParts.BodyPartTransform(fx.bodyPart);

                    if (trans)
                    {
                        particleObj.transform.SetParent(trans);
                        return;
                    }
                }

                if (fx.target == ParticleTarget.Self)
                {
                    particleObj.transform.position = portal.transform.position;
                    return;
                }

                particleObj.transform.position = currentEntity.transform.position;

            }

            void HandleOffsets()
            {
                particleObj.transform.position += fx.positionOffset;
                particleObj.transform.localScale += fx.scaleOffset;
                particleObj.transform.eulerAngles += fx.rotationOffset;
            }
        }

        void HandleAudio(PortalFX fx)
        {
            if (audioManager == null) return;
            var audioClip = fx.clip;
            if (audioClip == null) return;

            audioManager.PlaySound(audioClip);
        }
    }
}
