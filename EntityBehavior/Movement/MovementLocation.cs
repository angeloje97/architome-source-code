using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    public class MovementLocation : EntityProp
    {
        // Start is called before the first frame update
        public float spaceRange;
        Movement movement;

        [Serializable]
        public struct ParticleFX
        {
            public Transform bin;
            public ParticleSystem tryMove;
        }

        public ParticleFX particles;

        new void GetDependencies()
        {
            base.GetDependencies();
            movement = GetComponentInParent<Movement>();


        }


        void Start()
        {
            transform.localScale = new Vector3(spaceRange, spaceRange, spaceRange);

            GetDependencies();
            StopAllParticles();
        }


        // Update is called once per frame
        void Update()
        {

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