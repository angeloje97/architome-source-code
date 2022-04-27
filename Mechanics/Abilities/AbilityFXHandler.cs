using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Threading.Tasks;

namespace Architome
{
    public class AbilityFXHandler : EntityProp
    {
        // Start is called before the first frame update
        AbilityManager ability;
        CharacterInfo character;
        CharacterBodyParts bodyPart;
        AudioManager audioManager;
        ParticleManager particleManager;

        [Serializable]
        public struct Info
        {
            public List<GameObject> CastingParticles, ChannelingParticles, AbilityParticles, ReleaseParticles;
        }

        public Info info;

        void Start()
        {
            info.CastingParticles = new();
            info.ChannelingParticles = new();
            info.AbilityParticles = new();
            info.ReleaseParticles = new();
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {

        }

        new void GetDependencies()
        {
            base.GetDependencies();

            ability = GetComponent<AbilityManager>();
            character = entityInfo.GetComponentInChildren<CharacterInfo>();
            if (character)
            {
                bodyPart = character.GetComponentInChildren<CharacterBodyParts>();
            }


            particleManager = entityInfo.GetComponentInChildren<ParticleManager>();
            audioManager = entityInfo.SoundEffect();

            if (ability)
            {
                ability.OnCastStart += OnCastStart;
                ability.OnCastEnd += OnCastEnd;
                ability.OnChannelStart += OnChannelStart;
                ability.OnChannelEnd += OnChannelEnd;

                ability.OnAbilityStart += OnAbilityStart;
                ability.OnAbilityEnd += OnAbilityEnd;

                ability.OnCatalystRelease += OnCatalystRelease;
            }
        }

        void OnCatalystRelease(AbilityInfo ability, CatalystInfo catalyst)
        {
            HandleEffects(ability.catalystInfo, AbilityTrigger.OnRelease, true);
        }

        private void OnAbilityStart(AbilityInfo ability)
        {

            HandleEffects(ability.catalystInfo, AbilityTrigger.OnAbility, true);

        }

        private void OnAbilityEnd(AbilityInfo ability)
        {

            HandleEffects(ability.catalystInfo, AbilityTrigger.OnAbility, false);
        }

        private void OnCastEnd(AbilityInfo ability)
        {

            HandleEffects(ability.catalystInfo, AbilityTrigger.OnCast, false);
        }

        private void OnCastStart(AbilityInfo ability)
        {
            HandleEffects(ability.catalystInfo, AbilityTrigger.OnCast, true);
        }

        private void OnChannelStart(AbilityInfo ability)
        {
            HandleEffects(ability.catalystInfo, AbilityTrigger.OnChannel, true);
        }

        private void OnChannelEnd(AbilityInfo ability)
        {

            HandleEffects(ability.catalystInfo, AbilityTrigger.OnChannel, false);
            
        }

        void HandleEffects(CatalystInfo catalyst, AbilityTrigger trigger, bool status)
        {
            if (catalyst == null) return;
            foreach (var effect in catalyst.effects.abilityEffects)
            {
                if (effect.trigger != trigger) continue;

                HandleSound(effect, status);
                HandleParticle(effect, status);
            }
        }

        

        void HandleSound(CatalystInfo.CatalystEffects.Ability effect, bool status)
        {
            if (effect.audioClip == null) return;
            if (audioManager == null) return;

            if (status)
            {
                if (effect.loops)
                {
                    audioManager.PlaySoundLoop(effect.audioClip);
                }
                else
                {
                    audioManager.PlaySound(effect.audioClip);
                }

                return;
            }

            var source = audioManager.AudioSourceFromClip(effect.audioClip);

            if (source) source.Stop();
        }

        void HandleParticle(CatalystInfo.CatalystEffects.Ability effect, bool status)
        {
            if (effect.particle == null) return;
            if (particleManager == null) return;
            if (effect.particle.GetComponent<ParticleSystem>() == null) return;

            var gameObjects = info.AbilityParticles;

            switch (effect.trigger)
            {
                case AbilityTrigger.OnCast:
                    gameObjects = info.CastingParticles;
                    break;
                case AbilityTrigger.OnChannel:
                    gameObjects = info.ChannelingParticles;
                    break;
                case AbilityTrigger.OnRelease:
                    gameObjects = info.ReleaseParticles;
                    break;
            }


            if (status)
            {
                var newParticle = particleManager.Play(effect.particle);

                
                gameObjects.Add(newParticle.gameObject);


                HandleParticleTransform(effect, newParticle.GetComponent<ParticleSystem>());
                newParticle.transform.localPosition = new();

                newParticle.transform.localScale += effect.offsetScale;
                newParticle.transform.localPosition += effect.offsetPosition;
                newParticle.transform.eulerAngles += effect.offsetRotation;

                if (effect.trigger == AbilityTrigger.OnRelease)
                {
                    ArchAction.Delay(() => {
                        DestroyParticles();
                    }, newParticle.main.duration + 2f);
                }

                return;
            }

            DestroyParticles();

            void DestroyParticles()
            {
                for (int i = 0; i < gameObjects.Count; i++)
                {
                    gameObjects[i].GetComponent<ParticleSystem>().Stop();
                    var current = gameObjects[i];

                    gameObjects.RemoveAt(i);

                    i--;

                    ArchAction.Delay(() => Destroy(current), 2f);

                }
            }
        }

        async void HandleParticleTransform(CatalystInfo.CatalystEffects.Ability effect, ParticleSystem particle)
        {
            if (effect.target == CatalystParticleTarget.Self)
            {
                particle.transform.SetParent(transform);
            }

            if (effect.target == CatalystParticleTarget.Ground)
            {
                particle.transform.position = V3Helper.GroundPosition(transform.position, GMHelper.LayerMasks().walkableLayer);
                particle.transform.SetParent(CatalystManager.active.transform);
            }

            if (effect.target == CatalystParticleTarget.BodyPart)
            {
                particle.transform.SetParent(bodyPart.BodyPartTransform(effect.bodyPart));
            }

            if (!effect.looksAtTarget && effect.target != CatalystParticleTarget.BetweenBodyParts) return;

            Transform bodyPart1 = bodyPart.BodyPartTransform(effect.bodyPart);
            Transform bodyPart2 = bodyPart.BodyPartTransform(effect.bodyPart2);

            while (particle.isPlaying)
            {
                await Task.Yield();

                if (effect.target == CatalystParticleTarget.BetweenBodyParts)
                {
                    particle.transform.position = V3Helper.MidPoint(bodyPart2.position, bodyPart1.position);
                }

                if (!CanContinueEffect(effect)) break;

                if (effect.looksAtTarget)
                {
                    particle.transform.LookAt(ability.currentlyCasting.target ? ability.currentlyCasting.targetLocked.transform.position : ability.currentlyCasting.locationLocked);
                }


            }
        }

        bool CanContinueEffect(CatalystInfo.CatalystEffects.Ability effect)
        {
            if (ability.currentlyCasting == null)
            {
                return false;
            }

            if (!ability.currentlyCasting.isCasting && effect.trigger == AbilityTrigger.OnCast)
            {
                return false;
            }

            if (!ability.currentlyCasting.channel.active && effect.trigger == AbilityTrigger.OnChannel)
            {
                return false;
            }

            return true;
        }
    }

}