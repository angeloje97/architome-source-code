using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class AugmentKillAllPartyMembers : AugmentType
    {
        [Header("Kill All Party Members Property")]

        [SerializeField] List<PartyInfo> partyTargets;

        protected override void GetDependencies()
        {
            partyTargets = new();

            var playableParties = GameManager.active.playableParties;

            foreach (var party in playableParties)
            {
                partyTargets.Add(party);
            }


            EnablePlayableParty();
            EnableSuccesfulCast();
        }

        public override void HandleNewPlayableParty(PartyInfo party, int index)
        {
            if (partyTargets.Contains(party)) return;

            partyTargets.Add(party);
        }

        public override void HandleSuccessfulCast(AbilityInfo ability)
        {
            if (partyTargets == null) return;
            foreach (var party in partyTargets)
            {
                if (party.members == null) continue;

                foreach (var member in party.members)
                {
                    member.KillSelf(augment.entity);
                }
            }
        }

        protected override string Description()
        {
            return "Kills all party members upon successful cast.";
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
