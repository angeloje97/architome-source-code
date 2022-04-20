using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Linq;

namespace Architome
{
    public class EntityFXHandler : EntityProp
    {
        // Start is called before the first frame update
        
        CharacterBodyParts bodyParts;
        AbilityManager abilityManager;
        EntitySpeech speech;
        ParticleManager particleManager;
        AudioManager voiceFX;
        AudioManager soundFX;
        new void GetDependencies()
        {
            base.GetDependencies();

            particleManager = GetComponentInChildren<ParticleManager>();

            

            foreach (var soundManager in GetComponentsInChildren<AudioManager>())
            {
                if (soundManager.mixerGroup == GMHelper.Mixer().SoundEffect)
                {
                    soundFX = soundManager;
                }

                if (soundManager.mixerGroup == GMHelper.Mixer().Voice)
                {
                    voiceFX = soundFX;
                }
            }

            if (entityInfo)
            {
                if (entityInfo.entityFX == null) return;
                abilityManager = entityInfo.AbilityManager();
                bodyParts = entityInfo.GetComponentInChildren<CharacterBodyParts>();
                speech = entityInfo.Speech;

                entityInfo.OnLevelUp += OnLevelUp;
                entityInfo.OnReviveThis += OnRevive;
                entityInfo.OnDeath += OnDeath;


                entityInfo.OnDamageTaken += OnDamageTaken;

                abilityManager.OnCastStart += OnCastStart;
                abilityManager.OnCastEnd += OnCastEnd;

                abilityManager.OnAbilityStart += OnAbilityStart;
                abilityManager.OnAbilityEnd += OnAbilityEnd;

                if (entityInfo.AIBehavior())
                {
                    entityInfo.AIBehavior().events.OnDetectedEnemy += OnDetectedEnemy;
                }

            }
        }


        void Start()
        {
            GetDependencies();
        }
        private void OnDetectedEnemy(GameObject obj)
        {
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
            try
            {
                var particle = entityInfo.entityFX.leveUpParticles;
                var sound = entityInfo.entityFX.levelUpSound;
                soundFX.PlaySound(sound);
                particleManager.PlayOnceAt(particle, bodyParts.BodyPartTransform(BodyPart.Root), 3);
            }
            catch
            {
                throw;
            }
        }

        void OnRevive(CombatEventData eventData)
        {
            try
            {
                var particle = entityInfo.entityFX.reviveParticles;
                var sound = entityInfo.entityFX.reviveSound;
                soundFX.PlaySound(sound);
                particleManager.PlayOnceAt(particle, bodyParts.BodyPartTransform(BodyPart.Root), 3);
            }
            catch
            {
                throw;
            }
        }
        void OnAbilityStart(AbilityInfo ability)
        {

        }
        private void OnAbilityEnd(AbilityInfo ability)
        {

        }


        void OnDamageTaken(CombatEventData eventData)
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

        void OnCastStart(AbilityInfo ability)
        {
            try
            {
                soundFX.PlayRandomSound(ability.catalystInfo.effects.startCastSounds);
                soundFX.PlayRandomLoop(ability.catalystInfo.effects.castingSounds);
            }
            catch
            {
                throw;
            }
        }

        void OnCastEnd(AbilityInfo ability)
        {
            try
            {
                foreach (var sound in ability.catalystInfo.effects.castingSounds)
                {
                    soundFX.AudioSourceFromClip(sound).Stop();
                }
            }
            catch
            {
                throw;
            }
        }


    }

}