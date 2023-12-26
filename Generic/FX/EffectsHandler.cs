using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace Architome
{
    [Serializable]
    public class EffectsHandler<T> where T: Enum
    {
        [Header("Required Fields")]
        public Transform defaultParticleSpawnTarget;
        public AudioManager audioManager;
        public ParticleManager particleManager;

        public List<EventItemHandler> effects;
        Dictionary<T, List<EventItemHandler>> subsets;

        public void InitiateItemEffects(Action<EventItemHandler> handleItem)
        {
            effects ??= new();
            subsets = new();
            foreach(var fx in effects)
            {
                fx.SetEventItem(defaultParticleSpawnTarget, audioManager, particleManager);
                handleItem(fx);

                if (!subsets.ContainsKey(fx.trigger))
                {
                    subsets[fx.trigger] ??= new();
                }

                subsets[fx.trigger].Add(fx);
            }
        }

        public void Activate(T triggerName)
        {
            if (!subsets.ContainsKey(triggerName)) return;
            foreach (var fx in subsets[triggerName])
            {
                fx.ActivateEffect();
            }
        }

        [Serializable]
        public class EventItemHandler
        {
            public T trigger;
            [Header("Particle Properties")]
            public GameObject particleObject;
            public Transform particleTarget;
            public Vector3 positionOffset;
            public Vector3 scaleOffset;

            [Header("Audio Properties")]
            public AudioClip audioClip;



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
                HandleParticle();
                HandleAudioClip();

            }

            async void HandleParticle()
            {
                if (particleObject == null) return;
                if (particleManager == null) return;

                var target = particleTarget ?? defaultTarget;

                var (system, gameObj) = particleManager.Play(particleObject);
                var trans = gameObj.transform;

                trans.SetParent(target);

                HandlePosition();
                HandleScale();


                await HandleDuration();

                particleManager.StopParticle(gameObj);

                void HandlePosition()
                {
                    trans.localPosition += positionOffset;
                }

                void HandleScale()
                {
                    trans.localScale += scaleOffset;
                }

            }

            void HandleAudioClip()
            {
                if (audioClip == null || audioManager == null) return;
            }


            bool durationStarted;
            async Task HandleDuration()
            {
                if (durationStarted)
                {
                    await ArchAction.WaitUntil(() => durationStarted, false);
                    return;
                }

                durationStarted = true;
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
                durationStarted = false;
            }
        }
    }
}