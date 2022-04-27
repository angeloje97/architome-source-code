using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

namespace Architome
{
    public class BuffFXHandler : MonoBehaviour
    {
        // Start is called before the first frame update
        public BuffInfo buffInfo;

        void GetDependencies()
        {
            buffInfo = GetComponent<BuffInfo>();
            buffInfo.OnBuffStart += OnBuffStart;
            buffInfo.OnBuffEnd += OnBuffEnd;
            buffInfo.OnBuffInterval += OnBuffInterval;
        }
        void Start()
        {
            GetDependencies();
        }
        public void OnBuffStart(BuffInfo buff)
        {
            CalculateDelayTime(buff);
            PlayStartingSound();
            PlayParticlePairs();
            PlayStartingParticles();
            AdjustRadiusParticle();

            
            
            void PlayStartingSound()
            {
                if (buffInfo.effects.startingSound == null) return;
                

                buffInfo.hostInfo.SoundEffect().PlaySound(buffInfo.effects.startingSound);
            }

            void PlayStartingParticles()
            {
                if (buff.effects.startingParticles.Count == 0) return;

                buff.expireDelay = buff.effects.startingParticles.Max(particle => particle.main.duration);

                foreach (var particle in buff.effects.startingParticles)
                {
                    particle.Play(true);
                }
                
            }

            async void PlayParticlePairs()
            {
                var bodyParts = buff.hostInfo.GetComponentInChildren<CharacterBodyParts>();
                
                foreach (var pair in buff.effects.startingParticlePair)
                {
                    pair.particle.Play();
                    //pair.particle.transform.SetParent(bodyParts.BodyPartTransform(pair.target));
                    pair.transform = bodyParts.BodyPartTransform(pair.target);
                }

                while (!buff.IsComplete)
                {
                    await Task.Yield();

                    foreach (var pair in buff.effects.startingParticlePair)
                    {
                        pair.particle.transform.position = pair.transform.position;
                    }
                }


            }

            void AdjustRadiusParticle()
            {
                if (buffInfo.effects.radiusParticle == null) return;
                var buffRadius = buffInfo.properties.radius;
                var scalePortions = buffInfo.effects.scalePortions;
                var radiusParticles = buffInfo.effects.radiusParticle;
                var multiplier = buffInfo.effects.scaleMultiplier != 0 ? buff.effects.scaleMultiplier : 1f;

                scalePortions.x = scalePortions.x == 0 ? buffRadius * multiplier : scalePortions.x;
                scalePortions.y = scalePortions.y == 0 ? buffRadius * multiplier: scalePortions.y;
                scalePortions.z = scalePortions.z == 0 ? buffRadius * multiplier: scalePortions.z;

                radiusParticles.transform.localScale = scalePortions;
            }
        }
        void CalculateDelayTime(BuffInfo buff)
        {
            var particles = GetComponentsInChildren<ParticleSystem>();
            if (particles.Length == 0) return;

            buff.expireDelay = particles.Max(particle => particle.main.duration);
        }
        public void OnBuffInterval(BuffInfo buff)
        {

        }
        public void OnBuffEnd(BuffInfo buff)
        {
            var lights = GetComponentsInChildren<Light>();
            StopStartingParticle();
            StopPairs();
            ShrinkOnEnd();
            
            foreach (var light in lights)
            {
                DimLight(light);
            }


            void StopStartingParticle()
            {
                if (buffInfo.effects.startingParticles == null) return;

                foreach (var particle in buffInfo.effects.startingParticles)
                {
                    particle.Stop(true);
                }
            }

            void StopPairs()
            {
                foreach (var pair in buff.effects.startingParticlePair)
                {
                    if (pair.particle == null) continue;
                    pair.particle.transform.SetParent(transform);
                    pair.particle.Stop();
                }
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
        
    }

}
