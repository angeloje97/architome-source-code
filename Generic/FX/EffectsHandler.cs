using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using Architome.Enums;
using System.Security.Cryptography.X509Certificates;
using Unity.IO.LowLevel.Unsafe;

namespace Architome.Effects
{
    #region Effects Handler
    [Serializable]
    public class EffectsHandler<T, E> where T: Enum where E: EventItemHandler<T>
    {
        [Header("Required Fields")]
        public Transform defaultParticleSpawnTarget;
        public AudioManager audioManager;
        public ParticleManager particleManager;

        public List<E> effects;
        Dictionary<T, List<EventItemHandler<T>>> subsets;

        public void InitiateItemEffects(Action<E> handleItem)
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

        public virtual List<E> DefaultItems()
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

        public void ListenTo(ArchEventHandler<T, object> eventHandler, Component listener)
        {
            InitiateItemEffects((effectItem) => {
                eventHandler.AddListener(effectItem.trigger, () => { }, listener);
            });
        }
    }
    #endregion  

    #region Event Item
    [Serializable]
    public class EventItemHandler<T> where T: Enum
    {
        [HideInInspector] public string name;

        [Header("Components")]
        protected Transform defaultTarget;
        protected ParticleManager particleManager;
        protected AudioManager audioManager;

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
        public float fadeDuration = 0f;

        [Header("Phrases")]
        [Multiline]
        public List<string> phrases;
        public SpeechType phraseType;


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

        public async void ActivateEffect()
        {
            var eventData = new EffectEventData<T>(this);

            HandleParticle(eventData);
            HandleAudioClip(eventData);
            HandlePhrase(eventData);

            await Task.Delay(62);

            //await eventData.UntilStopActive();
            await eventData.UntilDone(EffectEventField.Active);

            
        }

        protected async virtual void HandleParticle(EffectEventData<T> eventData)
        {
            if (particleObject == null) return;
            if (particleManager == null) return;

            var target = particleTarget ?? defaultTarget;

            var (system, gameObj) = particleManager.Play(particleObject);
            var trans = gameObj.transform;
            
            eventData.particlePlaying = true;
            eventData.particleActive = true;
            eventData.SetParticle(gameObj, system);

            trans.SetParent(target);
            trans.localPosition = new();


            HandleOffsets();

            HandleParticleExtension(eventData);


            await HandleDuration(eventData);

            particleManager.StopParticle(gameObj);

            eventData.particleActive = false;

            await Task.Delay(2000);

            UnityEngine.Object.Destroy(gameObj);

            eventData.particlePlaying = false;

            void HandleOffsets()
            {
                trans.localPosition += positionOffset;
                trans.localEulerAngles += rotationOffset;
                trans.localScale += scaleOffset;
            }
        }

        protected async virtual void HandleAudioClip(EffectEventData<T> eventData)
        {
            if (audioManager == null) return;
            var audioSources = new List<AudioSource>();

            if (audioClip != null)
            {
                audioSources.Add(audioManager.PlayAudioClip(audioClip));
            }

            if (randomClips != null && randomClips.Count > 0)
            {
                audioSources.Add(audioManager.PlayRandomSound(randomClips));
            }

            if (audioSources.Count == 0) return;

            eventData.audioPlaying = true;
            eventData.audioActive = true;
            eventData.SetAudioSources(audioSources);

            HandleAudioExtension(eventData);

            var playingTasks = new List<Task>();

            foreach(var source in audioSources)
            {
                playingTasks.Add(HandleAudioEnding(source));
            }

            await Task.WhenAll(playingTasks);
            await HandleDuration(eventData);



            eventData.audioActive = false;

            var tasks = new List<Task>();

            foreach (var source in audioSources)
            {
                tasks.Add(HandleFadeDuration(source));
            }

            await Task.WhenAll(tasks);

            eventData.audioPlaying = false;

            async Task HandleFadeDuration(AudioSource source)
            {
                if (fadeDuration <= 0f)
                {
                    source.Stop();
                    return;
                }

                var startingVolume = source.volume;
                await ArchCurve.Smooth((float lerpValue) => {

                    source.volume = Mathf.Lerp(startingVolume, 0f, lerpValue);
                }, CurveType.Linear, fadeDuration);

                source.Stop();
            }

            async Task HandleAudioEnding(AudioSource audioSource)
            {
                await ArchAction.WaitUntil(() => audioSource.isPlaying, false);
            }
        }

        protected async virtual void HandlePhrase(EffectEventData<T> eventData) { }

        protected async Task HandleDuration(EffectEventData<T> eventData)
        {
            if (eventData.durationStarted)
            {
                await ArchAction.WaitUntil(() => eventData.durationStarted, false);
                return;
            }

            eventData.durationStarted = true;
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
            eventData.durationStarted = false;
        }

        public virtual void HandleParticleExtension(EffectEventData<T> eventData) { }

        public virtual void HandleAudioExtension(EffectEventData<T> eventData) { }

    }
    #endregion

    #region Effect Event System

    public enum eEffectEvent
    {
        OnStartEffect,
        OnEndEffect,
    }

    public enum EffectEventField
    {
        Playing,
        Active,
        ParticlePlaying,
        ParticleActive,
        AudioPlaying,
        AudioActive,
    }

    public class EffectEventData<T> where T: Enum
    {
        public EventItemHandler<T> itemHandler;
        public T trigger { get; set; }

        public bool durationStarted;
        
        public bool playing { get { return particlePlaying || audioPlaying; } }

        public bool active { get { return particleActive || audioActive; } }

        public EffectEventData(EventItemHandler<T> itemHandler)
        {
            this.itemHandler = itemHandler;
            this.trigger = itemHandler.trigger;
        }

        public async Task UntilDone(Action action, EffectEventField field)
        {
            switch (field)
            {
                case EffectEventField.Active: await FieldTask(() => particleActive || audioActive); break;
                case EffectEventField.Playing: await FieldTask(() => particlePlaying || audioPlaying); break;
                case EffectEventField.ParticlePlaying: await FieldTask(() => particlePlaying); break;
                case EffectEventField.ParticleActive: await FieldTask(() => particleActive); break;
                case EffectEventField.AudioPlaying: await FieldTask(() => audioPlaying); break;
                case EffectEventField.AudioActive: await FieldTask(() => audioActive); break;

            }

            async Task FieldTask(Func<bool> predicate)
            {
                await ArchAction.WaitUntil(() =>
                {
                    action();
                    return predicate();
                }, false);
            }
        }

        public async Task UntilDone(EffectEventField field)
        {
            await UntilDone(() => { }, field);
        }

        public GameObject particleObject { get; private set; }

        public ParticleSystem particleSystem{ get; private set; }
        public bool particleActive { get; set; }
        public bool particlePlaying { get; set; }
        public void SetParticle(GameObject particleObject, ParticleSystem system)
        {
            this.particleSystem = system;
            this.particleObject = particleObject;
            
            particleActive = true;
            
        }

        public List<AudioSource> audioSources { get; private set; }
        public bool audioActive { get; set; }
        public bool audioPlaying { get; set; }
        public void SetAudioSources(List<AudioSource> audioSources)
        {
            this.audioSources = audioSources;
            audioActive = true;
        }

    }

    #endregion
}