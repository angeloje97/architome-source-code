using Architome.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    
    public struct CombatEvents
    {
        public Action<CombatEventData, bool> OnFixate;
        public Action<List<EntityState>, List<EntityState>> OnStatesChange;
        public Action<List<EntityState>, EntityState> OnStateNegated;

        public Action<ThreatManager.ThreatInfo, float> OnGenerateThreat;
        public Action<EntityInfo, float> OnPingThreat { get; set; }
        public Action<CombatEventData> OnImmuneDamage;
        public Action<EntityInfo> OnSummonEntity;
        public Action<EntityInfo, EntityInfo> OnNewCombatTarget { get; set; }
        public Action<CombatEventData> BeforeDamageTaken { get; set; }
        public Action<CombatEventData> BeforeDamageDone { get; set; }
        public Action<CombatEventData> BeforeHealingTaken { get; set; }
        public Action<CombatEventData> BeforeHealingDone { get; set; }
        public Action<EntityInfo, List<bool>> OnCanAttackCheck { get; set; }
        public Action<EntityInfo, List<bool>> OnCanHelpCheck { get; set; }
        public Action<List<bool>> OnCanBeAttackedCheck { get; set; }
        public Action<List<bool>> OnCanBeHelpedCheck { get; set; }

        public Action<CombatEventData> OnKillPlayer { get; set; }

        public Action<ThreatManager.ThreatInfo> OnFirstThreatWithPlayer { get; set; }

        public Action<EntityState> OnAddImmuneState { get; set; }
        public Action<EntityState> OnRemoveImmuneState { get; set; }


    }
}