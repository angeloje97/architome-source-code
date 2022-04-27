using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using Architome.Enums;

namespace Architome
{
    public class CatalystFXHandler : CatalystProp
    {
        // Start is called before the first frame update
        public AudioManager audioManager;
        public ParticleManager particleManager;

        new void GetDependencies()
        {
            base.GetDependencies();

            audioManager = GetComponentInChildren<AudioManager>();

            if (catalyst)
            {
                catalyst.OnCatalystDestroy += OnCatalystDestroy;
                catalyst.OnCatalystStop += OnCatalystStop;
                catalyst.OnHit += OnHit;
                catalyst.OnDamage += OnDamage;
                catalyst.OnHeal += OnHeal;
                catalyst.OnCatalingRelease += OnCatalingRelease;
                catalyst.OnInterval += OnInterval;
            }

            particleManager = CatalystManager.active.GetComponent<ParticleManager>();
        }
        void Start()
        {
            GetDependencies();
            AdjustDestroyDelay();
            HandleGrow();
            HandleStartFromGround();
            ActivateAwakeEffects();
        }

        void AdjustDestroyDelay()
        {
            deathCondition.destroyDelay = catalyst.effects.MaxCatalystDuration();
        }

        async void HandleStartFromGround()
        {
            if (!catalyst.effects.startFromGround) return;
            if (ability.abilityType != AbilityType.Spawn) return;

            var originalPosition = transform.position;

            var groundPos = V3Helper.GroundPosition(transform.position, GMHelper.LayerMasks().walkableLayer);

            transform.position = groundPos;

            while (transform.position != originalPosition)
            {
                await Task.Yield();

                transform.position = Vector3.Lerp(transform.position, originalPosition, .0625f);
            }
        }

        async void HandleGrow()
        {
            if (!catalyst.effects.growsOnAwake)
            {
                return;
            }

            var originalScale = transform.localScale;

            transform.localScale = new();

            while (transform.localScale != originalScale)
            {
                await Task.Yield();
                if (gameObject == null) break;

                transform.localScale = Vector3.Lerp(transform.localScale, originalScale, .0625f);
            }
        }
        async void HandleCollapse()
        {
            try
            {
                if (!catalyst.effects.collapseOnDeath) return;

                while (transform.localScale != new Vector3())
                {
                    await Task.Yield();
                    if (gameObject == null) break;
                    transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(), .125f);
                }
            }
            catch
            {

            }
        }

        void OnInterval(CatalystInfo catalyst)
        {
            HandleEffects(ReleaseCondition.OnInterval);
        }

        void OnCatalingRelease(CatalystInfo catalyst, CatalystInfo cataling)
        {
            HandleEffects(ReleaseCondition.OnCatalingRelease);
        }

        void ActivateAwakeEffects()
        {
            HandleEffects(ReleaseCondition.OnAwake);
        }

        private void OnCatalystStop(CatalystKinematics kinematics)
        {
            HandleEffects(ReleaseCondition.OnStop);
        }

        private void OnCatalystDestroy(CatalystDeathCondition condition)
        {
            HandleEffects(ReleaseCondition.OnDestroy);
            HandleCollapse();
        }

        private void OnHit(GameObject target)
        {
            HandleEffects(ReleaseCondition.OnHit);
        }

        

        public void OnDamage(GameObject target)
        {
            HandleEffects(ReleaseCondition.OnHarm);
        }

        public void OnHeal(GameObject target)
        {
            HandleEffects(ReleaseCondition.OnHeal);
        }

        public void HandleEffects(ReleaseCondition action)
        {
            var effects = catalyst.effects.catalystsEffects;

            foreach (var effect in effects)
            {
                if (effect.playTrigger != action) continue;

                HandleAudio(effect);
                HandleParticle(effect);
            }

        }

        void HandleAudio(CatalystInfo.CatalystEffects.Catalyst effect)
        {
            if (effect.audioClip == null) return;
            if (audioManager == null) return;

            if (effect.loops)
            {
                audioManager.PlaySoundLoop(effect.audioClip);
            }
            else
            {
                audioManager.PlaySound(effect.audioClip);
            }
        }

        void HandleParticle(CatalystInfo.CatalystEffects.Catalyst effect)
        {
            if (effect.particleObj == null) return;
            if (particleManager == null) return;

            var system = particleManager.Play(effect.particleObj, true);

            if (effect.target == CatalystParticleTarget.Ground)
            {

                system.transform.position = V3Helper.GroundPosition(transform.position, GMHelper.LayerMasks().walkableLayer);
                
            }
            else
            {
                system.transform.position = transform.position;
            }

            if (effect.looksAtTarget)
            {
                system.transform.rotation = V3Helper.LerpLookAt(system.transform, (catalyst.target ? catalyst.target.transform.position : catalyst.location), 1f);
            }



            system.transform.position += effect.offsetPosition;
            system.transform.localScale += effect.offsetScale;
            system.transform.eulerAngles += effect.offsetRotation;
        }

        // Update is called once per frame
    }

}
