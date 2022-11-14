using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class AugmentKillAllPartyMembers : AugmentType
    {
        [Header("Kill All Party Members Property")]

        [SerializeField] List<PartyInfo> partyTargets;
        void Start()
        {
            GetDependencies();

        }

        new async void GetDependencies()
        {
            await base.GetDependencies();

            partyTargets = new();

            var playableParties = GameManager.active.playableParties;

            foreach(var party in playableParties)
            {
                partyTargets.Add(party);
            }


            EnablePlayableParty();
            EnableCatalyst();
        }

        public override void HandleNewPlayableParty(PartyInfo party, int index)
        {
            if (partyTargets.Contains(party)) return;

            partyTargets.Add(party);
        }

        public override void HandleNewCatlyst(CatalystInfo catalyst)
        {
            if (partyTargets == null) return;
            foreach(var party in partyTargets)
            {
                if (party.members == null) continue;

                foreach(var member in party.members)
                {
                    member.KillSelf(augment.entity);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
