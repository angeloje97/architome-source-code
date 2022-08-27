using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace Architome
{
    public class BossBehavior : EntityProp
    {
        // Start is called before the first frame update
        public BossRoom bossRoom;

        public CombatBehavior combatBehavior;
        public Transform originalPosition { get; private set; }

        public List<Phase> phases;

        public Action<Phase> OnPhase;


        new void GetDependencies()
        {
            base.GetDependencies();

            BossRoom();

            originalPosition = bossRoom.bossPosition;

            if (entityInfo)
            {
                entityInfo.OnHealthChange += OnHealthChange;
                entityInfo.OnCombatChange += OnCombatChange;

                combatBehavior = transform.parent.GetComponentInChildren<CombatBehavior>();
            }

            
            
        }

        private void Awake()
        {
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public BossRoom BossRoom()
        {
            if (bossRoom == null)
            {
                var room = Entity.Room(transform.position);
                if (room.GetType() == typeof(BossRoom))
                {
                    bossRoom = (BossRoom)room;
                }
            }

            return bossRoom;

        }

        void OnCombatChange(bool isInCombat)
        {
            if (isInCombat)
            {

            }
            else
            {
                RevertPhases();
            }
        }

        void RevertPhases()
        {
            foreach (var phase in phases)
            {
                phase.activated = false;

                if (combatBehavior.specialAbilities.Contains(phase.phaseAbility))
                {
                    combatBehavior.specialAbilities.Remove(phase.phaseAbility);
                }
            }

            //for (int i = 0; i < combatBehavior.specialAbilities.Count; i++)
            //{
            //    var phase = phases.Find(phase => phase.phaseAbility.abilityIndex == combatBehavior.specialAbilities[i].abilityIndex);

            //    if (phase != null)
            //    {
            //        combatBehavior.specialAbilities.RemoveAt(i);
            //        i--;
            //    }
            //}
        }

        void OnHealthChange(float health, float shield, float maxHealth)
        {
            if (!entityInfo.isInCombat) return;
            foreach (var phase in phases)
            {
                if (phase.Activated(health / maxHealth))
                {
                    OnPhase?.Invoke(phase);
                }
            }
        }

        [Serializable]
        public class Phase
        {
            public bool activated, refillsMana;

            [Range(0, 1)]
            public float threshHold;
            [Multiline]
            public string activationPhrase;
            public SpecialAbility phaseAbility;
            public bool usesBossStation;

            public bool Activated(float percentHealth)
            {
                if (activated) return false;

                if (threshHold > percentHealth)
                {
                    activated = true;
                    return true;
                }

                return false;
            }
        }
    }

    
}