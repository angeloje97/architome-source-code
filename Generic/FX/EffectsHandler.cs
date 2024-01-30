using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using Architome.Enums;
using Architome.Events;

namespace Architome.Effects
{
    #region Effects Handler
    [Serializable]
    public class EffectsHandler<T, E> where T: Enum where E: EventItemHandler<T>
    {
        #region Common Data
        [Header("Required Fields")]
        public Transform defaultParticleSpawnTarget;
        public AudioManager audioManager;
        public ParticleManager particleManager;
        public ChatBubblesManager chatBubbleManager;

        public List<E> effects;
        Dictionary<T, List<EventItemHandler<T>>> subsets;

        #endregion

        #region Initiation
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
                    subsets.Add(fx.trigger, new());
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
        #endregion

        #region OnValidate
        public void Validate()
        {
            effects ??= new();

            foreach(var fx in effects)
            {
                fx.Validate();
            }
        }
        #endregion

        #region Live Functions

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
        #endregion
    }
    #endregion  

    #region Event Item
    [Serializable]
    public class EventItemHandler<T> where T: Enum
    {
        #region Common Data
        [HideInInspector] public string name;
        public float coolDown;
        bool onCoolDown;

        [Header("Components")]
        protected Transform defaultTarget;
        protected ParticleManager particleManager;
        protected AudioManager audioManager;
        protected ChatBubblesManager chatBubbleManager;

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
        public float fadeDuration;

        [Header("Audio Chance")]
        public bool useAudioChance;
        [Range(0f, 100f)] public float audioChance;

        [Header("Audio Looping")]
        public bool loopAudio;
        public float loopFadeDuration = .5f;
        

        [Header("Phrases")]
        [Multiline]
        public List<string> phrases;
        public Vector3 phrasePositionOffset;
        public bool followTarget;
        public SpeechType phraseType;

        public Action<EventItemHandler<T>, List<bool>> OnCanPlayCheck;


        bool overrideContinueCondition;
        float defaultDuration = 5;
        Func<bool> canContinuePlaying = () => {
            return false;
        };

        public Transform availableTarget { get {
                if (particleTarget == null)
                {
                    return defaultTarget;
                }

                return particleTarget;
            } }

        #endregion

        #region Initiation

        public void SetEventItem(Transform defaultTarget, AudioManager audioManager, ParticleManager particleManager) 
        {
            this.defaultTarget = defaultTarget;
            SetSoundManager(audioManager);
            this.particleManager = particleManager;
            chatBubbleManager = ChatBubblesManager.active;
        }

        public void SetSoundManager(AudioManager audioManager)
        {
            this.audioManager = audioManager;
        }

        public void SetCanContinuePredicate(Func<bool> predicate)
        {
            this.canContinuePlaying = predicate;
            overrideContinueCondition = true;
        }
        #endregion

        #region On Validate
        public void Validate()
        {
            this.name = trigger.ToString();
        }
        #endregion

        //Will continue the duration until the comparison is true.

        #region Live 
        public async void ActivateEffect()
        {
            if (!CanPlay()) return;

            var eventData = new EffectEventData<T>(this);



            HandleParticle(eventData);
            HandleAudioClip(eventData);
            HandlePhrase(eventData);
            var startCoolDown = HandleCoolDown();

            await Task.Delay(62);

            //await eventData.UntilStopActive();
            await eventData.UntilDone(EffectEventField.Active);

            startCoolDown();

        }
        protected bool CanPlay()
        {
            var values = new List<bool>();

            OnCanPlayCheck?.Invoke(this, values);
            if (values.ValidateLogic(false, LogicType.Exists)) return false;

            return onCoolDown == false;
        }
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
        Action HandleCoolDown()
        {
            var coolDownStarted = false;
            HandleMain();
            return StartCooldown;

            void StartCooldown()
            {
                coolDownStarted = true;
            }

            async void HandleMain()
            {
                if (coolDown <= 0f) return;

                onCoolDown = true;

                await ArchAction.WaitUntil(() => coolDownStarted, true);
                await World.Delay(coolDown);

                onCoolDown = false;
            }
        }
        
        #region Particle

        protected async virtual void HandleParticle(EffectEventData<T> eventData)
        {
            if (particleObject == null) return;
            if (particleManager == null) return;

            var target = availableTarget;

            var (system, gameObj) = particleManager.Play(particleObject);
            var trans = gameObj.transform;
            
            eventData.particlePlaying = true;
            eventData.particleActive = true;
            eventData.SetParticle(gameObj, system);


            trans.SetParent(target);
            Debugger.System(8701, $"Setting Particle Target: {target}");
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

        public virtual void HandleParticleExtension(EffectEventData<T> eventData) { }
        #endregion

        #region Audio
        protected async virtual void HandleAudioClip(EffectEventData<T> eventData)
        {
            if (audioManager == null) return;
            var audioSources = new List<AudioSource>();
            if (!CanPlayAudio()) return;

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
                playingTasks.Add(HandleAudioLoopDuration(source));

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
                if (source == null) return;
                if (!source.isPlaying) return;
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

        protected async virtual Task HandleAudioLoopDuration(AudioSource source)
        {
            if (!loopAudio) return;

            await World.Delay(5f);
            var startingVolume = source.volume;
            await ArchCurve.Smooth((float lerpValue) => {
                source.volume = Mathf.Lerp(startingVolume, 0f, lerpValue);
            }, CurveType.Linear, loopFadeDuration);

            source.Stop();
        }

        public virtual void HandleAudioExtension(EffectEventData<T> eventData) { }
        protected bool CanPlayAudio()
        {
            return PassedChance();
            bool PassedChance()
            {
                if (!useAudioChance) return true;

                return ArchGeneric.RollSuccess(audioChance);
            }
        }
        #endregion

        #region Phrases
        protected async virtual void HandlePhrase(EffectEventData<T> eventData) 
        {
            if (chatBubbleManager == null) return;
            if (phrases.Count == 0) return;


            var randomItem = ArchGeneric.RandomItem(phrases);
            var target = availableTarget;

            var chatBubble = chatBubbleManager.ProcessSpeech(target, randomItem, phraseType);

            eventData.SetArchChatBubble(chatBubble);

            HandlePhraseExtension(eventData);

            await chatBubble.UntilDoneShowing();
        }

        public virtual void HandlePhraseExtension(EffectEventData<T> eventData)
        {

        }

        #endregion

        #endregion
    }
    #endregion

    #region Effect Event System

    #region Enums

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

    #endregion

    public class EffectEventData<T> where T: Enum
    {
        #region Common Data
        public EventItemHandler<T> itemHandler;
        public T trigger { get; set; }

        public bool durationStarted;
        
        public bool playing { get { return particlePlaying || audioPlaying; } }

        public bool active { get { return particleActive || audioActive; } }
        #endregion

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

        #region Particle
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

        #endregion

        #region Audio
        public List<AudioSource> audioSources { get; private set; }
        public bool audioActive { get; set; }
        public bool audioPlaying { get; set; }
        public void SetAudioSources(List<AudioSource> audioSources)
        {
            this.audioSources = audioSources;
            audioActive = true;
        }

        #endregion

        #region Phrase

        public ArchChatBubble chatBubble;

        public bool chatBubblePlaying { get; set; }

        public async void SetArchChatBubble(ArchChatBubble chatBubble)
        {
            chatBubblePlaying = true;
            await chatBubble.UntilActiveDone();
            chatBubblePlaying = false;

        }
        #endregion
    }

    #endregion
}