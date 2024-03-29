using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using Architome.Enums;
using System;


namespace Architome
{
    public class CatalystFXHandler : CatalystProp
    {
        // Start is called before the first frame update
        public AudioManager audioManager;
        public AudioManager activeAudioManager
        {
            get
            {
                if (audioManager == null)
                {
                    audioManager = catalyst.catalystAudio;
                }
                return audioManager;
            }
        }
        public ParticleManager particleManager;
        public ArchLightManager lightManager;

        public Dictionary<CatalystEvent, List<CatalystInfo.CatalystEffects.Catalyst>> effectMap;

        new void GetDependencies()
        {
            base.GetDependencies();

            audioManager = catalyst.catalystAudio;

            //if (catalyst)
            //{
            //    catalyst.OnCatalystDestroy += OnCatalystDestroy;
            //    catalyst.OnCatalystStop += OnCatalystStop;
            //    catalyst.OnHit += OnHit;
            //    catalyst.OnDamage += OnDamage;
            //    catalyst.OnHeal += OnHeal;
            //    catalyst.OnCatalingRelease += OnCatalingRelease;
            //    catalyst.OnInterval += OnInterval;
            //}

            particleManager = CatalystManager.active.GetComponent<ParticleManager>();
            lightManager = particleManager.GetComponent<ArchLightManager>();
        }
        void Start()
        {
            GetDependencies();
            //UpdateMap();
            AdjustDestroyDelay();
            HandleLight();
            HandleGrow();
            HandleStartFromGround();
            HandleEventActions();
            
            //ActivateAwakeEffects();
        }

        void HandleEventActions()
        {
            if (catalyst == null) return;
            if (catalyst.effects.catalystsEffects == null) return;

            foreach (var fx in catalyst.effects.catalystsEffects)
            {
                catalyst.AddEventAction(fx.playTrigger, () => { HandleEffects(fx); });
            }

            catalyst.AddEventAction(CatalystEvent.OnDestroy, () => {
                HandleCollapse();
            });
        }

        void HandleLight()
        {
            if (!catalyst.effects.light.enable) return;
            var lightEffect = catalyst.effects.light;
            lightManager.Light(transform, lightEffect.color, lightEffect.intensity, 5f);
        }
        void AdjustDestroyDelay()
        {
            var maxDuration = catalyst.effects.MaxCatalystDuration();

            if(deathCondition.destroyDelay < maxDuration)
            {
                deathCondition.destroyDelay = maxDuration;
            }
        }
        async void HandleStartFromGround()
        {
            if (!catalyst.effects.startFromGround) return;
            if (ability.abilityType != AbilityType.Spawn) return;

            var originalPosition = transform.position;

            var groundPos = V3Helper.GroundPosition(transform.position, GMHelper.LayerMasks().walkableLayer);

            transform.position = groundPos;

            await World.UpdateAction((float deltaTime) => {
                transform.position = Vector3.Lerp(transform.position, originalPosition, .0625f);

                if(Vector3.Distance(transform.position, originalPosition) <= .0125f)
                {
                    transform.position = originalPosition;
                }

                return transform.position != originalPosition && !catalyst.isDestroyed;
            });
        }
        async void HandleGrow()
        {
            if (!catalyst.effects.growsOnAwake)
            {
                return;
            }

            var originalScale = transform.localScale;

            transform.localScale = new();

            await World.UpdateAction((float deltaTime) => {
                if (transform.localScale == originalScale) return false;
                if (catalyst.isDestroyed) return false;
                if (gameObject == null) return false;

                transform.localScale = Vector3.Lerp(transform.localScale, originalScale, .0625f);



                return true;
            });
        }
        async void HandleCollapse()
        {
            try
            {
                if (!catalyst.effects.collapseOnDeath) return;

                while (transform.localScale != new Vector3())
                {
                    await Task.Yield();
                    if (this == null) break;
                    transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(), .125f);
                }
            }
            catch
            {

            }
        }


        public void HandleEffects(CatalystInfo.CatalystEffects.Catalyst effects)
        {
            HandleAudio(effects);
            HandleParticle(effects);
        }
        public void HandleEffects(CatalystEvent action)
        {

            if (!effectMap.ContainsKey(action)) return;
            //var effects = catalyst.effects.catalystsEffects;

            foreach (var effect in effectMap[action])
            {
                //if (effect.playTrigger != action) continue;

                HandleAudio(effect);
                HandleParticle(effect);
            }

        }
        void HandleAudio(CatalystInfo.CatalystEffects.Catalyst effect)
        {
            if (effect.audioClip == null) return;
            if (activeAudioManager == null) return;

            if (effect.loops)
            {
                var source = audioManager.PlaySoundLoop(effect.audioClip);

                HandleLoop(source);
            }
            else
            {
                audioManager.PlaySound(effect.audioClip);
            }

            async void HandleLoop(AudioSource source)
            {
                while (!catalyst.isDestroyed)
                {
                    await Task.Yield();
                }

                source.Stop();
            }
        }
        void HandleParticle(CatalystInfo.CatalystEffects.Catalyst effect)
        {
            if (effect.particleObj == null) return;
            if (particleManager == null) return;

            var (system, particleObj) = particleManager.Play(effect.particleObj, true);

            HandleParticleTransform(effect, particleObj);
            
            system.transform.position += effect.offsetPosition;
            system.transform.localScale += effect.offsetScale;
            system.transform.eulerAngles += effect.offsetRotation;
            
            HandleParticleRadius(system, effect);
        }

        void HandleParticleRadius(ParticleSystem particleTransform, CatalystInfo.CatalystEffects.Catalyst effect)
        {
            if (ability == null) return;
            if (effect.manifestRadius == RadiusType.None) return;
            var radius = ability.Radius(effect.manifestRadius);



            particleTransform.transform.localScale = effect.radiusProportion * radius * Vector3.one;
        }

        public void HandleParticleTransform(CatalystInfo.CatalystEffects.Catalyst effect, GameObject system)
        {
            if (effect.target == CatalystParticleTarget.Ground)
            {

                system.transform.position = V3Helper.GroundPosition(transform.position, GMHelper.LayerMasks().walkableLayer);

            }

            else if (effect.target == CatalystParticleTarget.BodyPart)
            {
                var entity = catalyst.lastTargetHit != null ? catalyst.lastTargetHit : catalyst.entityInfo;
                var bodyPart = entity.GetComponentInChildren<CharacterBodyParts>().BodyPartTransform(effect.bodyPart);
                
                system.transform.SetParent(bodyPart);
                system.transform.localPosition = new();
            }
            else
            {
                system.transform.position = transform.position;
            }

            if (effect.looksAtTarget)
            {
                if (ability.abilityType == AbilityType.LockOn)
                {
                    system.transform.rotation = V3Helper.LerpLookAt(system.transform, (catalyst.target ? catalyst.target.transform.position : catalyst.location), 1f);
                }
                else
                {
                    system.transform.rotation = V3Helper.LerpLookAt(system.transform, catalyst.location, 1f);
                }
            }
        }
    }

}
