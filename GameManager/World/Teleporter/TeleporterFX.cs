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
            BetweenPosition,
            Target,
            Location
        }

        [System.Serializable]
        public class Effect
        {
            public Teleporter.Event eventTrigger;

            public GameObject particle;
            public TransformType particleType;
            public BodyPart bodyPartTarget;
            public bool setBodyPartAsParent;

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

                    if (effect.particleType == TransformType.Location)
                    {
                        particleObj.transform.position = position;
                        return;
                    }

                    if (effect.particleType == TransformType.Target)
                    {
                        var bodyParts = entity.BodyParts();
                        var bodyPart = bodyParts.BodyPartTransform(effect.bodyPartTarget);
                        if (effect.setBodyPartAsParent)
                        {
                            particleObj.transform.SetParent(bodyPart);
                            return;
                        }
                        particleObj.transform.position = bodyPart.transform.position;


                    }

                    if (effect.particleType == TransformType.BetweenTargets)
                    {
                        HandleBetweenTargets();
                    }

                    if (effect.particleType == TransformType.BetweenPosition)
                    {
                        HandleBetweenPositions();
                    }
                    
                    async void HandleBetweenTargets()
                    {

                        while (particle.isPlaying)
                        {
                            await Task.Yield();
                            particleObj.transform.LookAt(entity.transform);
                            var distance = V3Helper.Distance(entity.transform.position, particleObj.transform.position);
                            var localScale = particleObj.transform.localScale;
                            particleObj.transform.localScale = new Vector3(localScale.x, localScale.y, distance);

                        }
                    }

                    void HandleBetweenPositions()
                    {
                        particleObj.transform.LookAt(position);
                        var distance = V3Helper.Distance(position, particleObj.transform.position);
                        var localScale = particleObj.transform.localScale;
                        particleObj.transform.localScale = new Vector3(localScale.x, localScale.y, distance);
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
