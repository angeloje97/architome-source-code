using Architome.Enums;
using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public class TeleporterFX : MonoBehaviour
    {
        [SerializeField] ParticleManager particleManager;
        [SerializeField] AudioManager audioManager;
        [SerializeField] Teleporter teleporter;

        public enum TransformType
        {
            Self,
            BetweenTargets,
            Target
        }

        [System.Serializable]
        public class Effect
        {
            public Teleporter.Event eventTrigger;

            public GameObject particle;
            public TransformType particleType;
            public BodyPart bodyPartTarget;

            public AudioClip audioClip;
            public float volume = 1;
        }

        public List<Effect> effects;


        private void Start()
        {
            if (effects == null) return;
            foreach (var effect in effects)
            {
                teleporter.AddEventAction(effect.eventTrigger, (EntityInfo entity, Vector3 location) => {
                    HandleEffect(effect, entity, location);
                });
            }
        }

        void HandleEffect(Effect effect, EntityInfo entity, Vector3 position)
        {

            HandleParticle();
            HandleAudio();
            
            void HandleParticle()
            {
                if (particleManager == null) return;
                if (effect.particle == null) return;

                var (particle, particleObj) = particleManager.Play(effect.particle, true);

                HandleTransform();

                void HandleTransform()
                {
                    if (effect.particleType == TransformType.Self)
                    {
                        return;
                    }

                    if (effect.particleType == TransformType.Target)
                    {
                        var bodyParts = entity.BodyParts();
                        var bodyPart = bodyParts.BodyPartTransform(effect.bodyPartTarget);
                    }

                    if (effect.particleType == TransformType.BetweenTargets)
                    {
                        HandleBetweenTargets();
                    }
                    
                    async void HandleBetweenTargets()
                    {

                        while (particle.isPlaying)
                        {
                            await Task.Yield();
                            particleObj.transform.LookAt(entity.transform);
                        }
                    }
                }
            }

            void HandleAudio()
            {
                if (audioManager == null) return;
                if (effect.audioClip == null) return;

                audioManager.PlaySound(effect.audioClip, effect.volume);
            }
        }


    }
}
