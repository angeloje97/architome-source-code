using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Architome
{
    [Serializable]
    public class ParticleEvents<T> where T: Enum
    {
        [Header("Required Fields")]
        public Transform defaultParticleSpawnTarget;
        public AudioManager audioManager;
        public ParticleManager particleManager;

        public List<EventItemHandler> effects;


        public void UpdateEffectItems()
        {
            effects ??= new();

            foreach(var fx in effects)
            {
                fx.SetEventItem(defaultParticleSpawnTarget, audioManager, particleManager);
            }
        }

        

        [Serializable]
        public class EventItemHandler
        {
            public T trigger;
            public GameObject particleObject;
            public AudioClip audioClip;

            public Transform particleTarget;
            Transform defaultTarget;
            ParticleManager particleManager;
            AudioManager audioManager;

            bool overrideContinueCondition;
            float defaultDuration = 5;
            Func<bool> canContinuePlaying = () => {
                return false;
            };
            
            public void SetEventItem(Transform defaultTarget, AudioManager audioManager, ParticleManager particleManager)
            {
                this.defaultTarget = defaultTarget;
                this.audioManager = audioManager;
                this.particleManager = particleManager;
            }

            public void SetPredicate(Func<bool> predicate)
            {
                this.canContinuePlaying = predicate;
                overrideContinueCondition = true;
            }

            public void ActivateEffect()
            {
            }

            async void HandleParticle()
            {
                if (particleObject == null) return;
                var target = particleTarget ?? defaultTarget;

                var activeParticle = particleManager.Play(particleObject).Item1;

                await HandleDuration();
                


            }

            void HandleAudioClip()
            {

            }

            async Task HandleDuration()
            {
                if(!overrideContinueCondition)
                {
                    float currentTimer = defaultDuration;

                    await ArchAction.WaitUntil((float deltaTime) => {
                        currentTimer -= deltaTime;
                        return currentTimer <= 0f;
                    }, true);
                }
                else
                {
                    await ArchAction.WaitUntil(canContinuePlaying, false);
                }
            }
        }
    }
}