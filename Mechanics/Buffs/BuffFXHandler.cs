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

        private void OnValidate()
        {
            buffInfo = GetComponent<BuffInfo>();
        }
        void GetDependencies()
        {
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
            PlayStartingSound();
            PlayStartingParticles();
            AdjustRadiusParticle();
            
            void PlayStartingSound()
            {
                if (buffInfo.effects.startingSound == null) return;
                if (buffInfo.hostInfo == null) return;
                if (buffInfo.hostInfo.SoundEffect() == null) return;

                buffInfo.hostInfo.SoundEffect().PlaySound(buffInfo.effects.startingSound);
            }

            void PlayStartingParticles()
            {
                foreach (var particle in buff.effects.startingParticles)
                {
                    particle.Play(true);
                }
                
            }

            void AdjustRadiusParticle()
            {
                if (buffInfo.effects.radiusParticle == null) return;
                var buffRadius = buffInfo.properties.radius;
                var scalePortions = buffInfo.effects.scalePortions;
                var radiusParticles = buffInfo.effects.radiusParticle;

                if (scalePortions.x == 0)
                {
                    scalePortions.x = buffRadius * 2;
                }

                if (scalePortions.y == 0)
                {
                    scalePortions.y = buffRadius * 2;
                }

                if (scalePortions.z == 0)
                {
                    scalePortions.z = buffRadius * 2;
                }


                radiusParticles.transform.localScale = scalePortions;
                

                
            }
        }

        public void OnBuffInterval(BuffInfo buff)
        {

        }

        public void OnBuffEnd(BuffInfo buff)
        {
            var lights = GetComponentsInChildren<Light>();
            StopStartingParticle();
            ShrinkOnEnd();
            
            foreach (var light in lights)
            {
                DimLight(light);
            }


            void StopStartingParticle()
            {
                if (buffInfo.effects.startingParticles == null) return;


                var maxDuration = 0f;

                if (buffInfo.effects.startingParticles.Count > 0)
                {
                    maxDuration = buffInfo.effects.startingParticles.Max(particle => particle.main.duration);
                }

                if (maxDuration > 0)
                {
                    buffInfo.expireDelay = maxDuration;
                }

                foreach (var particle in buffInfo.effects.startingParticles)
                {
                    particle.Stop(true);
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
