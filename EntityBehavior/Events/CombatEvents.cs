using Architome.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    #region Combat Events
    #region Base Combat Event
    public class CombatEvent
    {
        public EntityInfo source { get; private set; }
        public EntityInfo target { get; private set; }
        public CatalystInfo catalyst { get; private set; }
        public AbilityInfo ability { get; private set; }
        public BuffInfo buff { get; private set; }

        public CombatEvent() { }

        public CombatEvent(CatalystInfo catalyst, EntityInfo source)
        {
            this.catalyst = catalyst;
            this.ability = catalyst.abilityInfo;
            this.source = source;
        }

        public CombatEvent(BuffInfo buff)
        {
            this.buff = buff;
            this.source = buff.sourceInfo;
            this.ability = buff.sourceAbility;
        }

        public CombatEvent(EntityInfo source)
        {
            this.source = source;
        }

        public void SetTarget(EntityInfo target)
        {
            this.target = target;
        }
    }
    #endregion

    #region Health Change Event

    public class HealthCombatEvent : CombatEvent
    {
        public float value { get; private set; }
        public DamageType damageType;

        public HealthCombatEvent(CatalystInfo catalyst, EntityInfo source, float value) : base(catalyst, source)
        {
            this.value = value;
        }

        public HealthCombatEvent(BuffInfo buffInfo, float value) : base(buffInfo)
        {
            this.value = value;
        }
    }


    #endregion

    #region State Change Event

    public class StateChangeEvent : CombatEvent
    {

    }
    #endregion

    #endregion

    #region Events
    public struct CombatEvents
    {
        public Action<CombatEventData, bool> OnFixate;

        public Action<ThreatManager.ThreatInfo, float> OnGenerateThreat;
        public Action<EntityInfo, float> OnPingThreat { get; set; }
        public Action<EntityInfo> OnSummonEntity;

        #region State Change Events
        public Action<List<EntityState>, List<EntityState>> OnStatesChange;
        public Action<List<EntityState>, EntityState> OnStateNegated;

        public Action<EntityState> OnAddImmuneState { get; set; }
        public Action<EntityState> OnRemoveImmuneState { get; set; }

        #endregion

        #region Health Change Events
        public Action<CombatEventData> OnImmuneDamage;
        public Action<CombatEventData> BeforeDamageTaken { get; set; }
        public Action<CombatEventData> BeforeDamageDone { get; set; }
        public Action<CombatEventData> BeforeHealingTaken { get; set; }
        public Action<CombatEventData> BeforeHealingDone { get; set; }
        public Action<CombatEventData> OnKillPlayer { get; set; }

        public Action<EntityInfo, List<Func<float>>> OnUpdateShield;

        public Action<EntityInfo, List<Func<float>>> OnUpdateHealAbsorbShield;
        #endregion


        public Action<EntityInfo, EntityInfo> OnNewCombatTarget { get; set; }
        public Action<EntityInfo, List<bool>> OnCanAttackCheck { get; set; }
        public Action<EntityInfo, List<bool>> OnCanHelpCheck { get; set; }
        public Action<List<bool>> OnCanBeAttackedCheck { get; set; }
        public Action<List<bool>> OnCanBeHelpedCheck { get; set; }
        public Action<ThreatManager.ThreatInfo> OnFirstThreatWithPlayer { get; set; }
       

        

    }

    #endregion
}