using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using Architome.Enums;

namespace Architome
{
    public class BuffFXHandler : MonoBehaviour
    {
        // Start is called before the first frame update
        public BuffInfo buffInfo;
        public AudioManager audioManager;
        public ParticleManager particleManager;
        public CharacterBodyParts bodyParts;

        [System.Serializable]
        public struct Info
        {
            public List<ParticleSystem> savedParticles;
            public List<AudioSource> savedSources;
        }

        public Info info;

        void GetDependencies()
        {
            buffInfo = GetComponent<BuffInfo>();
            buffInfo.OnBuffStart += OnBuffStart;
            buffInfo.OnBuffEnd += OnBuffEnd;
            buffInfo.OnBuffInterval += OnBuffInterval;
            buffInfo.OnBuffCompletion += OnBuffCompletion;
            buffInfo.OnBuffCleanse += OnBuffCleanse;

            audioManager = buffInfo.hostInfo.SoundEffect();
            particleManager = buffInfo.hostInfo.ParticleManager();
            bodyParts = buffInfo.hostInfo.GetComponentInChildren<CharacterBodyParts>();

            info.savedParticles = new();
            info.savedSources = new();

            if (buffInfo.hostInfo)
            {
                buffInfo.hostInfo.OnDamageTaken += OnHostDamageTaken;
                buffInfo.hostInfo.combatEvents.OnImmuneDamage += OnHostImmuneDamage;

                buffInfo.OnBuffEnd += (BuffInfo buff) => {
                    buffInfo.hostInfo.OnDamageTaken -= OnHostDamageTaken;
                    buffInfo.hostInfo.combatEvents.OnImmuneDamage -= OnHostImmuneDamage;
                };
            }


        }
        void Start()
        {
            GetDependencies();
            
            
        }
        public void OnBuffStart(BuffInfo buff)
        {
            CalculateDelayTime(buff);
            HandleEffect(buffInfo, BuffEvents.OnStart);
        }


        void CalculateDelayTime(BuffInfo buff)
        {
            var particles = GetComponentsInChildren<ParticleSystem>();
            if (particles.Length == 0) return;

            buff.expireDelay = particles.Max(particle => particle.main.duration);
        }
        public void OnBuffInterval(BuffInfo buff)
        {
            HandleEffect(buff, BuffEvents.OnInterval);
        }
        
        public void OnBuffEnd(BuffInfo buff)
        {
            HandleEffect(buff, BuffEvents.OnEnd);
            StopAllSaved();
            var lights = GetComponentsInChildren<Light>();
            ShrinkOnEnd();
            
            foreach (var light in lights)
            {
                DimLight(light);
            }


            async void ShrinkOnEnd()
            {
                if (!buffInfo.effects.shrinkOnEnd) return;

                var smoothening =  .125f;

                while (transform.localScale != new Vector3())
                {
                    await Task.Yield();
                    if (this == null) return;
                    if (gameObject == null) return;
                    transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(), smoothening);
                }
            }

            
            
        }
        async void DimLight(Light light)
        {
            var smoothening = (1 / buffInfo.expireDelay) * .250f;

            while (light.range > 0 && light.intensity > 0)
            {
                await Task.Yield();
                if (light == null) return;
                light.intensity = Mathf.Lerp(light.intensity, 0, smoothening);
                light.range = Mathf.Lerp(light.range, 0, smoothening);
            }
        }

        public void OnHostDamageTaken(CombatEventData eventData)
        {
            HandleEffect(buffInfo, BuffEvents.OnDamageTaken);
        }

        public void OnHostImmuneDamage(CombatEventData eventData)
        {
            HandleEffect(buffInfo, BuffEvents.OnDamageImmune);
        }

        public void OnBuffCompletion(BuffInfo buff)
        {
            HandleEffect(buff, BuffEvents.OnComplete);
        }
        public void OnBuffCleanse(BuffInfo buff)
        {
            HandleEffect(buff, BuffEvents.OnCleanse);
        }
        public void HandleEffect(BuffInfo buff, BuffEvents trigger)
        {
            if (buff == null) return;
            foreach (var effect in buff.effects.effectsData)
            {
                if (effect.playTrigger != trigger) continue;
                HandleAudio(effect);
                HandleParticle(effect);
            }

        }

        public void HandleAudio(BuffInfo.BuffFX.EffectData effect)
        {
            if (effect.audioClip == null) return;
            if (audioManager == null) return;



            if (effect.loops && effect.playTrigger == BuffEvents.OnStart)
            {
                info.savedSources.Add(audioManager.PlaySoundLoop(effect.audioClip));
            }
            else
            {
                audioManager.PlaySound(effect.audioClip);
            }
                
            return;
            


        }

        public void HandleParticle(BuffInfo.BuffFX.EffectData effect)
        {
            if (effect.particle == null) return;
            if (particleManager == null) return;

            if (!effect.playForDuration)
            {
                var newParticle = particleManager.Play(effect.particle, true);
                HandleParticleTransform(effect, newParticle.gameObject);
            }
            else
            {
                var savedParticle = particleManager.Play(effect.particle);
                HandleParticleTransform(effect, savedParticle.gameObject);
                info.savedParticles.Add(savedParticle);
            }
        }

        public void StopAllSaved()
        {
            foreach (var particle in info.savedParticles)
            {
                if (!particle) continue;
                particle.Stop(true);

                ArchAction.Delay(() => { if(particle) Destroy(particle.gameObject); }, 2f);
            }

            foreach (var source in info.savedSources)
            {
                source.Stop();
            }
        }

        public void HandleParticleTransform(BuffInfo.BuffFX.EffectData effect, GameObject particleObject)
        {
            particleObject.transform.localScale = Scale(effect);
            if (effect.target == CatalystParticleTarget.BodyPart)
            {
                particleObject.transform.SetParent(bodyParts.BodyPartTransform(effect.bodyPart));
                
            }

            if (effect.target == CatalystParticleTarget.Self)
            {
                particleObject.transform.SetParent(transform);
            }

            HandleOffSets();

            void HandleOffSets()
            {
                if (effect.target != CatalystParticleTarget.Ground)
                {
                    particleObject.transform.localPosition = new();
                }
                particleObject.transform.position += effect.offset;
                particleObject.transform.eulerAngles += effect.offsetRotation;
            }
        }

        public Vector3 Scale(BuffInfo.BuffFX.EffectData effect)
        {
            if (effect.radiusType == RadiusType.None)
            {
                return effect.particle.transform.localScale + effect.offsetScale;
            }

            var newScale = new Vector3();
            var portion = effect.radiusPortion;
            var radius = buffInfo.properties.radius;

            newScale.x = portion.x == 0f ? radius : portion.x;
            newScale.y = portion.y == 0f ? radius : portion.y;
            newScale.z = portion.z == 0f ? radius : portion.z;

            return newScale;

        }




    }

}
