using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Linq;
using System.Threading.Tasks;
using System;
using Architome.Effects;
using static UnityEngine.ParticleSystem;

namespace Architome
{
    public class EntityFXHandler : EntityProp
    {
        [SerializeField] List<EntityFXPack> effectPacks;
        
        EntitySpeech speech;
        ParticleManager particleManager;
        AudioManager audioManager;
        
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
            audioManager = GetComponentInChildren<AudioManager>();

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

            abilityManager = entityInfo.AbilityManager();
            characterBodyPart = entityInfo.BodyParts();
            speech = entityInfo.Speech();


            HandleEffectsHandler(entityInfo.entityFX);

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
            effectPacks ??= new();

            foreach(var pack in effectPacks)
            {
                //foreach(var fx in pack.effects)
                //{
                //    entityInfo.AddEventTrigger(() =>
                //    {
                //        HandleEffect(fx);
                //        //Debugger.Error(6549, $"{entityInfo} FX Handler needs an update to move from effect pack to effect handler for {fx.trigger}");
                //    }, fx.trigger);
                //}
                HandleEffectsHandler(pack);
            }
        }


        void HandleEffectsHandler(EntityFXPack fxPack)
        {
            if (fxPack == null) return;
            var effectsHandler = Instantiate(fxPack).effectsHandler;
            if (effectsHandler == null) return;

            effectsHandler.SetDefaults(entityInfo.transform, audioManager, particleManager);

            effectsHandler.InitiateItemEffects((data) => {
                data.SetProperties(entityInfo, catalystManager);

                if (audioTypeDict.ContainsKey(data.audioType))
                {
                    data.SetSoundManager(audioTypeDict[data.audioType]);
                }

                data.OnCanPlayCheck += (data, list) => {
                    list.Add(active);
                };

                entityInfo.AddEventTrigger(() => {

                    data.ActivateEffect();
                }, data.trigger);
            });
        }

        void OnLoadScene(ArchSceneManager sceneManager)
        {
            DetermineActive(sceneManager.sceneToLoad);
        }

        public AudioManager AudioManager(AudioMixerType mixerType)
        {
            if (!audioTypeDict.ContainsKey(mixerType)) return null;
            return audioTypeDict[mixerType];
        }

        #region FXData
        [Serializable]
        public class FXData : EventItemHandler<EntityEvent>
        {
            [Header("Entity Properties")]
            EntitySpeech speech;
            CatalystManager catalystManager;
            CharacterBodyParts characterBodyParts;


            [Header("Custom Particles")]
            public CatalystParticleTarget target;
            public BodyPart bodyPart;
            public BodyPart bodyPart2;

            public void SetProperties(EntityInfo entity, CatalystManager catalystManager)
            {
                speech = entity.Speech();
                characterBodyParts = entity.BodyParts();
                this.catalystManager = catalystManager;
            }

            public override async void HandleParticleExtension(EffectEventData<EntityEvent> eventData)
            {
                var particle = eventData.particleSystem;

                Action updates = () => { };

                if (target == CatalystParticleTarget.Ground)
                {
                    var walkableLayer = GMHelper.LayerMasks().walkableLayer;
                    particle.transform.SetParent(catalystManager != null ? catalystManager.transform : null);
                    particle.transform.position = V3Helper.GroundPosition(particle.transform.position, walkableLayer, 3, 0);
                }

                if (target == CatalystParticleTarget.BodyPart && characterBodyParts != null)
                {

                    var bodyPart = characterBodyParts.BodyPartTransform(this.bodyPart);

                    if (bodyPart)
                    {
                        particle.transform.SetParent(bodyPart);
                        particle.transform.localPosition = new();
                    }
                }

                if(target == CatalystParticleTarget.BetweenBodyParts && characterBodyParts != null)
                {
                    if (characterBodyParts == null) return;
                    var bodyPart1 = characterBodyParts.BodyPartTransform(bodyPart);
                    var bodyPart2 = characterBodyParts.BodyPartTransform(this.bodyPart2);

                    updates += () => {
                        particle.transform.position = V3Helper.MidPoint(bodyPart1.position, bodyPart2.position);
                        particle.transform.position += positionOffset;
                    };
                }

                await eventData.UntilDone(() => {
                    updates?.Invoke();
                }, EffectEventField.ParticlePlaying);
            }

            protected override void HandlePhrase(EffectEventData<EntityEvent> eventData)
            {
                if (speech == null) return;
                if (phrases == null || phrases.Count == 0) return;
                var randomPhrase = ArchGeneric.RandomItem(phrases);

                speech.Interperate(randomPhrase, phraseType);
            }
        }
        #endregion
    }

}