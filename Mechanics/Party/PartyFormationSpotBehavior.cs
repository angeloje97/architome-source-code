using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

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

        PartyInfo party;
        PartyFormation partyFormation;
        int index;
        Movement movement;
        EntityInfo entity;


        void GetDependencies()
        {
            partyFormation = GetComponentInParent<PartyFormation>();
            party = GetComponentInParent<PartyInfo>();

            if (partyFormation)
            {
                partyFormation.OnHoldingChange += OnHoldingChange;

                var spotBehaviors = partyFormation.GetComponentsInChildren<PartyFormationSpotBehavior>();
                for(int i = 0; i < spotBehaviors.Length; i++)
                {
                    var behavior = spotBehaviors[i];
                    if (behavior != this) continue;

                    index = i;
                }
            }




        }
        void Start()
        {
            GetDependencies();
        }

        public void OnHoldingChange(bool isHolding)
        {
            if (particles.rotating == null) return;
            if (!CanPlay()) return;

            if (isHolding)
            {
                particles.rotating.Play(true);
            }
            else
            {
                particles.rotating.Stop();
            }

        }

        bool CanPlay()
        {
            if (index >= party.members.Count) return false;
            
            if(entity != party.members[index])
            {
                entity = party.members[index];
                movement = entity.Movement(); 
            }

            if (!movement) return false;
            if (!movement.canMove) return false;

            

            return true;
        }
    }

}