using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class AugmentFXHandler : MonoBehaviour
    {
        Augment augment;

        ParticleManager particleManager;
        AudioManager audioManager;
        CatalystManager catalystManager;

        

        [Serializable]
        public struct AugmentFX
        {
            public AugmentEvent eventTrigger;

            [Header("Particles")]
            public GameObject particleObject;
            public ParticleTarget particleTarget;
            public BodyPart bodyPartTarget;
            public bool loopParticle;

            [Header("Audio")]
            public AudioClip audioClip;
            public bool loopAudio;
        }

        public List<AugmentFX> augmentFX;

        void Start()
        {
            GetDependencies();
        }
        void Update()
        {
        
        }

        async void GetDependencies()
        {
            augment = GetComponent<Augment>();
            while (augment.dependenciesAcquired)
            {
                await Task.Yield();
            }

            catalystManager = CatalystManager.active;
            particleManager = augment.entity.GetComponentInChildren<ParticleManager>();
            audioManager = augment.entity.SoundEffect();

            augment.OnAugmentTrigger += OnAugmentEvent;
            augment.OnAugmentActivate += OnAugmentEvent;
        }

        private void OnAugmentEvent(Augment.AugmentEventData eventData)
        {
            if (!eventData.active) return;
            HandleEffect(eventData);
        }

        // Update is called once per frame

        public void HandleEffect(Augment.AugmentEventData eventData)
        {
            if (augmentFX == null) return;

            foreach (var fx in augmentFX)
            {
                if (fx.eventTrigger != eventData.eventTrigger) continue;
                HandleAudio(fx);
                HandleParticle(fx);
            }

            void HandleAudio(AugmentFX fx)
            {
                if (fx.audioClip == null) return;
                if (audioManager == null) return;
            }

            void HandleParticle(AugmentFX fx)
            {
                if (fx.particleObject == null) return;
                if (particleManager == null) return;

                var particle = particleManager.Play(fx.particleObject, !fx.loopParticle);
                
                if (particle == null) return;
                particle.transform.SetParent(catalystManager.transform);

                HandleTransform();
                HandleDuration();


                void HandleTransform()
                {

                    switch(fx.particleTarget)
                    {
                        case ParticleTarget.Catalyst:
                            HandleCatalyst();
                            break;
                        case ParticleTarget.Target:
                            HandleTarget();
                            break;
                    }

                    void HandleCatalyst()
                    {
                        var catalyst = eventData.augmentType.activeCatalyst;
                        if (catalyst == null) return;

                        particle.transform.SetParent(catalyst.transform);

                        catalyst.OnCatalystDestroy += (CatalystDeathCondition condition) => {
                            if (particle != null)
                            {
                                particle.transform.SetParent(catalystManager.transform);
                            }
                        };
                    }

                    void HandleTarget()
                    {
                        if (fx.particleTarget != ParticleTarget.Target) return;
                        var catalyst = eventData.augmentType.activeCatalyst;
                        if (catalyst == null) return;
                        if (catalyst.target == null) return;
                        var target = catalyst.target;

                        var characterBodyParts = target.GetComponentInChildren<CharacterBodyParts>();
                        if (characterBodyParts == null) return;

                        var bodyPart = characterBodyParts.BodyPartTransform(fx.bodyPartTarget);
                        if (bodyPart == null) return;

                        particle.transform.SetParent(bodyPart);

                    }
                }

                async void HandleDuration()
                {
                    if (!fx.loopParticle) return;
                    var particleSystem = particle.GetComponentInChildren<ParticleSystem>();
                    if (particleSystem == null) return;
                    while (eventData.active)
                    {
                        await Task.Yield();
                    }

                    particleSystem.Stop(true);

                    await Task.Delay(2000);

                    Destroy(particle);
                }
            }
        }

    }
}
