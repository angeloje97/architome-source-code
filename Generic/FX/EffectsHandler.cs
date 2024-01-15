using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using Architome.Enums;

namespace Architome.Effects
{
    [Serializable]
    public class EffectsHandler<T> where T: Enum
    {
        [Header("Required Fields")]
        public Transform defaultParticleSpawnTarget;
        public AudioManager audioManager;
        public ParticleManager particleManager;

        public List<EventItemHandler<T>> effects;
        Dictionary<T, List<EventItemHandler<T>>> subsets;

        public void InitiateItemEffects(Action<EventItemHandler<T>> handleItem)
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

        public void SetDefaults(Transform defaultTarget = null, AudioManager audioManager = null, ParticleManager particleManager = null)
        {
            if (defaultTarget != null) defaultParticleSpawnTarget = defaultTarget;
            if (audioManager != null) this.audioManager = audioManager;
            if (particleManager != null) this.particleManager = particleManager;
        }


        public virtual List<EventItemHandler<T>> DefaultItems()
        {
            effects ??= new();
            return effects;
        }

        public void Validate()
        {
            effects ??= new();

            foreach(var fx in effects)
            {
                fx.Validate();
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


    }

    [Serializable]
    public class EffectsHandler<T,E> : EffectsHandler<T> where T: Enum where E: EventItemHandler<T>
    {
        public new List<E> effects;
        Dictionary<T, List<E>> subsets;
        public void InitiateCustomEffects(Action<E> handleItem)
        {
            effects ??= new();
            subsets = new();
            foreach (var fx in effects)
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
    }

    [Serializable]
    public class EventItemHandler<T>
    {
        [HideInInspector] public string name;
        public T trigger;
        [Header("Particle Properties")]
        public GameObject particleObject;
        public Transform particleTarget;
        public Vector3 positionOffset;
        public Vector3 scaleOffset;
        public Vector3 rotationOffset;

        [Header("Audio Properties")]
        public AudioClip audioClip;
        public AudioMixerType audioType;
        public List<AudioClip> randomClips;

        [Header("Phrases")]
        [Multiline]
        public List<string> phrases;
        public SpeechType phraseType;

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

        public void Validate()
        {
            this.name = trigger.ToString();
        }

        //Will continue the duration until the comparison is true.
        public void SetCanContinuePredicate(Func<bool> predicate)
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

            HandleParticleExtension(particleObject);


            await HandleDuration();

            particleManager.StopParticle(gameObj);

            await Task.Delay(2000);

            UnityEngine.Object.Destroy(gameObj);

            void HandlePosition()
            {
                trans.localPosition += positionOffset;
            }

            void HandleScale()
            {
                trans.localScale += scaleOffset;
            }


        }

        

        public virtual void HandleParticleExtension(GameObject particleObject) { }

        async void HandleAudioClip()
        {
            if (audioManager == null) return;
            var audioSources = new List<AudioSource>();

            if(audioClip != null)
            {
                audioSources.Add(audioManager.PlayAudioClip(audioClip));
            }

            if(randomClips != null && randomClips.Count > 0)
            {
                audioSources.Add(audioManager.PlayRandomSound(randomClips));
            }

            HandleAudioExtension(audioSources);

            await HandleDuration();

            foreach(var source in audioSources)
            {
                //Need to change this to a better behavior
                source.Stop();
            }
        }

        public virtual void HandleAudioExtension(List<AudioSource> audioSource) { }

        


        bool durationStarted;
        async Task HandleDuration()
        {
            if (durationStarted)
            {
                await ArchAction.WaitUntil(() => durationStarted, false);
                return;
            }

            durationStarted = true;
            if (!overrideContinueCondition)
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