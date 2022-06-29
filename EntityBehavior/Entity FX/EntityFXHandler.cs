using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Linq;
using System.Threading.Tasks;
namespace Architome
{
    public class EntityFXHandler : EntityProp
    {
        // Start is called before the first frame update
        
        CharacterBodyParts bodyParts;
        EntitySpeech speech;
        ParticleManager particleManager;
        AudioManager voiceFX;
        AudioManager soundFX;
        EntityFXPack entityFX;
        CatalystManager catalystManager;
        CharacterBodyParts characterBodyPart;

        public Dictionary<EntityEvent, List<EntityFXPack.EntityEffect>> effectMap;
        new void GetDependencies()
        {
            base.GetDependencies();

            particleManager = GetComponentInChildren<ParticleManager>();

            var mixer = GMHelper.Mixer();



            foreach (var soundManager in GetComponentsInChildren<AudioManager>())
            {
                if (soundManager.mixerGroup == mixer.SoundEffect)
                {
                    soundFX = soundManager;
                }

                if (soundManager.mixerGroup == GMHelper.Mixer().Voice)
                {
                    voiceFX = soundFX;
                }
            }

            catalystManager = CatalystManager.active;

            if (entityInfo)
            {
                entityFX = entityInfo.entityFX;
                characterBodyPart = entityInfo.GetComponentInChildren<CharacterBodyParts>();

                if (entityFX == null) return;

                bodyParts = entityInfo.GetComponentInChildren<CharacterBodyParts>();
                speech = entityInfo.Speech;

                entityInfo.OnLevelUp += OnLevelUp;
                entityInfo.OnReviveThis += OnRevive;
                entityInfo.OnDeath += OnDeath;


                entityInfo.OnDamageTaken += OnDamageTaken;

                var threatManager = entityInfo.ThreatManager();

                if (threatManager)
                {
                    threatManager.OnFirstThreat += OnFirstThreat;
                }

            }
        }

        public void FillMap()
        {
            if (entityFX == null) return;
            effectMap = new();
            foreach (var effect in entityFX.effects)
            {
                if (effectMap.ContainsKey(effect.trigger))
                {
                    effectMap[effect.trigger].Add(effect);
                }
                else
                {
                    effectMap.Add(effect.trigger, new() { effect });
                }
            }
        }

        void Start()
        {
            GetDependencies();
            FillMap();
        }
        void OnFirstThreat(ThreatManager.ThreatInfo info)
        {
            if (entityInfo.rarity != EntityRarity.Player)
            {
                HandleEffect(EntityEvent.OnDetectPlayer);
            }

            if (entityInfo.entityFX == null) return;
            try
            {
                var role = Random.Range(0, 100);

                //if (role > 25) return;

                var phrases = entityInfo.entityFX.detectedPlayerPhrases;

                if (phrases.Count == 0) return;
                var randomPhrase = phrases[Random.Range(0, phrases.Count)];

                speech.Yell(randomPhrase);
            }
            catch
            {
                throw;
            }
        }
        void OnDeath(CombatEventData eventData)
        {
            HandleEffect(EntityEvent.OnDeath);
            try
            {
                if (!Entity.IsPlayer(eventData.source.gameObject)) return;
                var role = Random.Range(0, 100);
                //if (role > 25) return;

                var phrases = entityInfo.entityFX.deathPhrases;


                if (phrases.Count == 0) return;
                var randomPhrase = phrases[Random.Range(0, phrases.Count)];

                speech.Yell(randomPhrase);
            }
            catch
            {

            }
        }
        void OnLevelUp(int level)
        {
            HandleEffect(EntityEvent.OnLevelUp);
            //try
            //{
            //    var particle = entityInfo.entityFX.leveUpParticles;
            //    var sound = entityInfo.entityFX.levelUpSound;
            //    soundFX.PlaySound(sound);
            //    particleManager.PlayOnceAt(particle, bodyParts.BodyPartTransform(BodyPart.Root), 3);
            //}
            //catch
            //{
            //    throw;
            //}
        }
        void OnRevive(CombatEventData eventData)
        {
            HandleEffect(EntityEvent.OnRevive);

            //try
            //{
            //    var particle = entityInfo.entityFX.reviveParticles;
            //    var sound = entityInfo.entityFX.reviveSound;
            //    soundFX.PlaySound(sound);
            //    particleManager.PlayOnceAt(particle, bodyParts.BodyPartTransform(BodyPart.Root), 3);
            //}
            //catch
            //{
            //    throw;
            //}
        }
        public void OnDamageTaken(CombatEventData eventData)
        {
            if (entityInfo.entityFX == null) return;
            if (entityInfo.entityFX.hurtSounds == null) return;
            try
            {
                
                var hurtSounds = entityInfo.entityFX.hurtSounds;
                voiceFX.PlayRandomSound(hurtSounds);

            }
            catch
            {
                throw;
            }
        }

        public void HandleEffect(EntityEvent eventTrigger)
        {
            if (effectMap == null) return;
            if (!effectMap.ContainsKey(eventTrigger)) return;
            var effects = effectMap[eventTrigger];

            foreach (var effect in effects)
            {
                HandleParticle(effect);
                HandleAudio(effect);
                HandlePhrases(effect);
            }
        }

        public void HandleParticle(EntityFXPack.EntityEffect effect)
        {
            if (effect.particleEffect == null) return;
            if (particleManager == null) return;

            var particle = particleManager.Play(effect.particleEffect, true);

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
            if (soundFX == null) return;
            if (effect.audioClip == null) return;


            soundFX.PlaySound(effect.audioClip);
        }

        public void HandlePhrases(EntityFXPack.EntityEffect effect)
        {
            if (effect.phrases == null || effect.phrases.Count == 0) return;
            if (speech == null) return;
            var randomPhrase = effect.phrases[Random.Range(0, effect.phrases.Count)];

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