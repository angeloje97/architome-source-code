using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace Architome
{
    public class MovementLocation : EntityProp
    {
        // Start is called before the first frame update
        public float spaceRange;
        Movement movement;

        [SerializeField] bool enableLocationSpread;

        [Serializable]
        public struct ParticleFX
        {
            public Transform bin;
            public ParticleSystem tryMove;
        }

        public ParticleFX particles;

        public override void GetDependencies()
        {
            transform.localScale = new Vector3(spaceRange, spaceRange, spaceRange);
            StopAllParticles();

            movement = GetComponentInParent<Movement>();
        }

        void StopAllParticles()
        {
            foreach (var particle in GetComponentsInChildren<ParticleSystem>())
            {
                particle.Stop();
            }
        }

        //void OnTryMove(Movement movement)
        //{
        //    if (particles.tryMove == null) return;
        //    particles.tryMove.Play(true);
        //}

        public void PlayTryMove()
        {
            if (particles.tryMove == null) return;
            if (entityInfo.rarity != Enums.EntityRarity.Player) return;
            particles.tryMove.Play(true);
        }


        private void OnTriggerEnter(Collider other)
        {
            if (!enableLocationSpread) return;
            if (other.GetComponent<MovementLocation>())
            {
                Debugger.InConsole(1562, $"Movement locations collided");
                var direction = -V3Helper.Direction(other.transform.position, transform.position);
                var displaceMent = direction * spaceRange;
                transform.position += new Vector3(displaceMent.x, 0, displaceMent.z);
            }
        }
    }

}