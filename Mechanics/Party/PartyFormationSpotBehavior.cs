using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    public class PartyFormationSpotBehavior : MonoBehaviour
    {
        // Start is called before the first frame update
        [Serializable]
        public struct Particles
        {
            public ParticleSystem rotating;
        }

        public Particles particles;

        PartyFormation partyFormation;
        void GetDependencies()
        {
            partyFormation = GetComponentInParent<PartyFormation>();

            if (partyFormation)
            {
                partyFormation.OnHoldingChange += OnHoldingChange;
            }


        }
        void Start()
        {
            GetDependencies();
        }

        public void OnHoldingChange(bool isHolding)
        {
            if (particles.rotating == null) return;

            if (isHolding)
            {
                particles.rotating.Play(true);
            }
            else
            {
                particles.rotating.Stop();
            }

        }
    }

}