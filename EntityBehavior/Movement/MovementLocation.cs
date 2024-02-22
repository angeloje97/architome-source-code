using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace Architome
{
    public class MovementLocation : EntityProp
    {
        #region Common Data
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

        #endregion

        #region Initiation
        public override void GetDependencies()
        {
            transform.localScale = new Vector3(spaceRange, spaceRange, spaceRange);
            StopAllParticles();

            movement = GetComponentInParent<Movement>();

            movement.AddListener(eMovementEvent.OnTryMove, PlayTryMove, this);

        }
        #endregion

        #region Class Actions
        void StopAllParticles()
        {
            foreach (var particle in GetComponentsInChildren<ParticleSystem>())
            {
                particle.Stop();
            }
        }

        public void PlayTryMove(MovementEventData eventData)
        {
            if (particles.tryMove == null) return;
            if (entityInfo.rarity != Enums.EntityRarity.Player) return;
            if (!movement.TargetIsMovementLocation(eventData.target.gameObject)) return;
            particles.tryMove.Play(true);

        }

        #endregion

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