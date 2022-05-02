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
        AbilityInfo currentAbility;
        CharacterInfo character;
        CharacterBodyParts bodyPart;
        AudioManager audioManager;
        ParticleManager particleManager;
        LayerMasksData layers;

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

                ability.OnCancelCast += OnCancelCast;
                ability.OnCancelChannel += OnCancelChannel;
            }
            layers = GMHelper.LayerMasks();
        }

        void OnCatalystRelease(AbilityInfo ability, CatalystInfo catalyst)
        {
            HandleEffects(ability.catalystInfo, AbilityEvent.OnRelease, true);
        }

        private void OnAbilityStart(AbilityInfo ability)
        {

            HandleEffects(ability.catalystInfo, AbilityEvent.OnAbility, true);
            currentAbility = ability;

        }

        private void OnAbilityEnd(AbilityInfo ability)
        {

            HandleEffects(ability.catalystInfo, AbilityEvent.OnAbility, false);
        }

        private void OnCastEnd(AbilityInfo ability)
        {

            HandleEffects(ability.catalystInfo, AbilityEvent.OnCast, false);
        }

        private void OnCastStart(AbilityInfo ability)
        {
            HandleEffects(ability.catalystInfo, AbilityEvent.OnCast, true);
        }

        private void OnChannelStart(AbilityInfo ability)
        {
            HandleEffects(ability.catalystInfo, AbilityEvent.OnChannel, true);
        }

        private void OnChannelEnd(AbilityInfo ability)
        {

            HandleEffects(ability.catalystInfo, AbilityEvent.OnChannel, false);
            
        }
        
        void OnCancelCast(AbilityInfo ability)
        {

            CancelAudios(ability.catalystInfo, AbilityEvent.OnCast);
        }

        void OnCancelChannel(AbilityInfo ability)
        {
            CancelAudios(ability.catalystInfo, AbilityEvent.OnChannel);
        }

        void CancelAudios(CatalystInfo catalyst, AbilityEvent trigger)
        {
            if (catalyst == null) return;
            foreach (var effect in catalyst.effects.abilityEffects)
            {
                if (effect.audioClip == null) continue;

                var source = audioManager.AudioSourceFromClip(effect.audioClip);

                if (source)
                {
                    source.Stop();
                }
            }

        }

        void HandleEffects(CatalystInfo catalyst, AbilityEvent trigger, bool status)
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

            if (!effect.loops) return;

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
                case AbilityEvent.OnCast:
                    gameObjects = info.CastingParticles;
                    break;
                case AbilityEvent.OnChannel:
                    gameObjects = info.ChannelingParticles;
                    break;
                case AbilityEvent.OnRelease:
                    gameObjects = info.ReleaseParticles;
                    break;
            }


            if (status)
            {
                var newParticle = particleManager.Play(effect.particle);

                
                gameObjects.Add(newParticle.gameObject);


                HandleParticleTransform(effect, newParticle.GetComponent<ParticleSystem>());
                

                newParticle.transform.localScale += effect.offsetScale;
                newParticle.transform.localPosition += effect.offsetPosition;
                newParticle.transform.eulerAngles += effect.offsetRotation;

                if (effect.trigger == AbilityEvent.OnRelease)
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

        public Vector3 NewScale(CatalystInfo.CatalystEffects.Ability effect)
        {
            if (effect.manifestRadius == RadiusType.None)
            {
                return effect.offsetScale;
            }
            else
            {
                var value = currentAbility.Radius(effect.manifestRadius);
                return new Vector3(value, value, value);
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
                particle.transform.position = V3Helper.GroundPosition(transform.position, layers.walkableLayer);
                particle.transform.SetParent(CatalystManager.active.transform, true);
            }

            if (effect.target == CatalystParticleTarget.BodyPart)
            {
                particle.transform.SetParent(bodyPart.BodyPartTransform(effect.bodyPart));
            }

            if (!effect.looksAtTarget && effect.target != CatalystParticleTarget.BetweenBodyParts && effect.target != CatalystParticleTarget.Location) return;

            Transform bodyPart1 = bodyPart.BodyPartTransform(effect.bodyPart);
            Transform bodyPart2 = bodyPart.BodyPartTransform(effect.bodyPart2);

            while (particle.isPlaying)
            {

                if (effect.target == CatalystParticleTarget.BetweenBodyParts)
                {
                    particle.transform.position = V3Helper.MidPoint(bodyPart2.position, bodyPart1.position);
                }

                if (effect.target == CatalystParticleTarget.Location)
                {
                    if (ability.currentlyCasting)
                    {
                        particle.transform.position = ability.currentlyCasting.locationLocked;
                    }

                    if (effect.secondTarget == CatalystParticleTarget.Ground)
                    {
                        particle.transform.position = V3Helper.GroundPosition(particle.transform.position, layers.walkableLayer, 1, .0625f);
                    }
                }


                if (effect.looksAtTarget)
                {
                    particle.transform.LookAt(ability.currentlyCasting.target ? ability.currentlyCasting.targetLocked.transform.position : ability.currentlyCasting.locationLocked);
                }

                

                await Task.Yield();
                if (!CanContinueEffect(effect)) break;

            }
        }
        bool CanContinueEffect(CatalystInfo.CatalystEffects.Ability effect)
        {
            if (ability.currentlyCasting == null)
            {
                return false;
            }

            if (!ability.currentlyCasting.isCasting && effect.trigger == AbilityEvent.OnCast)
            {
                return false;
            }

            if (!ability.currentlyCasting.channel.active && effect.trigger == AbilityEvent.OnChannel)
            {
                return false;
            }

            return true;
        }
    }

}