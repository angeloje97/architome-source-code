using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Linq;
using System.Threading.Tasks;
using System;
using Architome.Effects;

namespace Architome
{
    public class EntityFXHandler : EntityProp
    {
        [SerializeField] List<EntityFXPack> effectPacks;
        [SerializeField] EffectsHandler<EntityEvent> effectHandler;
        
        EntitySpeech speech;
        ParticleManager particleManager;
        EntityFXPack entityFX;
        CatalystManager catalystManager;
        CharacterBodyParts characterBodyPart;
        AbilityManager abilityManager;

        public Dictionary<AudioMixerType, AudioManager> audioTypeDict;


        public HashSet<EntityEvent> recentTriggers;

        public List<ArchScene> activeScenes;
        bool active;

        private void Awake()
        {
            recentTriggers = new();
            audioTypeDict = new();
        }

        public override void GetDependencies()
        {
            DetermineActive(null);
            particleManager = GetComponentInChildren<ParticleManager>();

            var mixer = GMHelper.Mixer();

            var sound = mixer.MixerGroup(AudioMixerType.SoundFX);
            var voice = mixer.MixerGroup(AudioMixerType.Voice);
            var sceneManager = ArchSceneManager.active;

            if (sceneManager)
            {
                sceneManager.AddListener(SceneEvent.OnLoadScene, OnLoadScene, this);
            }

            foreach (var soundManager in GetComponentsInChildren<AudioManager>())
            {
                if (soundManager.mixerGroup == sound)
                {
                    audioTypeDict.Add(AudioMixerType.SoundFX, soundManager);
                }

                if (soundManager.mixerGroup == voice)
                {
                    audioTypeDict.Add(AudioMixerType.Voice, soundManager);

                }
            }

            catalystManager = CatalystManager.active;

            if (entityInfo)
            {
                entityFX = entityInfo.entityFX;
                abilityManager = entityInfo.AbilityManager();
                //characterBodyPart = entityInfo.GetComponentInChildren<CharacterBodyParts>();
                characterBodyPart = entityInfo.BodyParts();
                speech = entityInfo.Speech();

                if (entityFX == null) return;

                foreach (var fx in entityFX.effects)
                {
                    entityInfo.AddEventTrigger(() =>
                    {
                        HandleEffect(fx);
                    }, fx.trigger);
                }

            }

            HandleAdditiveEffects();

            
        }


        void DetermineActive(ArchSceneManager.SceneInfo sceneInfo)
        {
            var sceneManager = ArchSceneManager.active;
            if(sceneManager == null)
            {
                active = false;
                return;
            }

            sceneInfo ??= sceneManager.CurrentScene();

            if(sceneInfo == null)
            {
                active = true;
                return;
            }

            var scene = sceneInfo.scene;
            active = activeScenes.Contains(scene);

        }

        void HandleAdditiveEffects()
        {
            if (effectPacks == null) return;

            foreach(var pack in effectPacks)
            {
                foreach(var fx in pack.effects)
                {
                    entityInfo.AddEventTrigger(() =>
                    {
                        HandleEffect(fx);
                    }, fx.trigger);
                }
            }


        }

        void OnLoadScene(ArchSceneManager sceneManager)
        {
            DetermineActive(sceneManager.sceneToLoad);
        }

        public void HandleEffect(EntityFXPack.EntityEffect effect)
        {
            if (!active) return;

            if (effect.useChance)
            {
                var success = ArchGeneric.RollSuccess(effect.chance);

                if (!success) return;

            }
            if (!CanPlay(effect, true)) return;

            HandleParticle(effect);
            HandleAudio(effect);
            HandlePhrases(effect);
        }

        public bool CanPlay(EntityFXPack.EntityEffect effect, bool invoker)
        {
            if (effect.coolDown <= 0f) return true;

            if (!recentTriggers.Contains(effect.trigger))
            {
                if (invoker)
                {
                    recentTriggers.Add(effect.trigger);

                    ArchAction.Delay(() => {
                        recentTriggers.Remove(effect.trigger);
                    }, effect.coolDown);
                }
                return true;
            }

            return false;

        }

        public void HandleParticle(EntityFXPack.EntityEffect effect)
        {
            if (effect.particleEffect == null) return;
            if (particleManager == null) return;

            var (particle, particleObj) = particleManager.Play(effect.particleEffect, true);

            HandleTransform();
            HandleOffset();

            void HandleTransform()
            {
                if (effect.target == CatalystParticleTarget.Ground)
                {
                    var walkableLayer = GMHelper.LayerMasks().walkableLayer;
                    particle.transform.SetParent(catalystManager != null ? catalystManager.transform : null);
                    particle.transform.position = V3Helper.GroundPosition(particle.transform.position, walkableLayer, 3, 0);
                }

                if (effect.target == CatalystParticleTarget.BodyPart)
                {
                    if (characterBodyPart == null) return;
                    var bodyPartEnum = effect.bodyPart;

                    var bodyPart = characterBodyPart.BodyPartTransform(bodyPartEnum);

                    if (bodyPart)
                    {
                        particle.transform.SetParent(bodyPart);
                        particle.transform.localPosition = new();
                    }
                }

                HandleBetweenBodyParts();

                async void HandleBetweenBodyParts()
                {
                    if (effect.target != CatalystParticleTarget.BetweenBodyParts) return;
                    if (characterBodyPart == null) return;

                    var bodyPart1 = characterBodyPart.BodyPartTransform(effect.bodyPart);
                    var bodyPart2 = characterBodyPart.BodyPartTransform(effect.bodyPart2);

                    while (particle.isPlaying)
                    {
                        particle.transform.position = V3Helper.MidPoint(bodyPart1.position, bodyPart2.position);
                        particle.transform.position += effect.positionOffset;
                        await Task.Yield();
                    }
                }
            }

            void HandleOffset()
            {
                particle.transform.position += effect.positionOffset;
                particle.transform.localScale += effect.scaleOffset;
                particle.transform.eulerAngles += effect.rotationOffset;
            }
        }

        public void HandleAudio(EntityFXPack.EntityEffect effect)
        {
            var sfx = AudioManager(effect.audioType);
            if (sfx == null) return;
            HandleMainSound();
            HandleRandomSound();


            void HandleRandomSound()
            {
                if (effect.randomClips == null) return;
                if (effect.randomClips.Count <= 0) return;
                var randomClip = ArchGeneric.RandomItem(effect.randomClips);
                sfx.PlaySound(randomClip);
            }
            void HandleMainSound()
            {
                if (effect.audioClip == null) return;

                sfx.PlaySound(effect.audioClip);


            }
        }

        public AudioManager AudioManager(AudioMixerType mixerType)
        {
            if (!audioTypeDict.ContainsKey(mixerType)) return null;
            return audioTypeDict[mixerType];
        }

        public void HandlePhrases(EntityFXPack.EntityEffect effect)
        {
            if (effect.phrases == null || effect.phrases.Count == 0) return;
            if (speech == null) return;

            var randomPhrase = ArchGeneric.RandomItem(effect.phrases);
            //var randomPhrase = effect.phrases[Random.Range(0, effect.phrases.Count)];

            switch (effect.phraseType)
            {
                case SpeechType.Yell:
                    speech.Yell(randomPhrase);
                    break;
                case SpeechType.Whisper:
                    speech.Whisper(randomPhrase);
                    break;
                default:
                    speech.Say(randomPhrase);
                    break;
            }
        }

    }

}