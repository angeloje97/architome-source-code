using Architome.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Events;
using System.Linq;

namespace Architome
{
    #region Combat Events

    #region Base Combat Event

    public enum eCombatEvent
    {
        OnCombatChange,
        OnGetRevive,
        OnGiveRevive,
        OnDeath,
        OnKill,
        OnNewCombatTarget,
        OnCanAttackCheck,
        OnCanHelpCheck,
        OnCanBeAttackedCheck,
        OnCanBeHelpedCheck,
    }

    public class CombatEvent
    {
        #region Common Data
        public EntityInfo source { get; private set; }
        public EntityInfo target { get; private set; }
        public CatalystInfo catalyst { get; private set; }

        public Augment augment { get; private set; }
        public AbilityInfo ability { get; private set; }
        public BuffInfo buff { get; private set; }

        #endregion

        #region Properties

        public bool sourceInCombat => source.isInCombat;
        public bool targetInCombat => target.isInCombat;
        public bool bothInCombat => target.isInCombat && source.isInCombat;

        #endregion

        #region Constructors
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
            this.target = buff.hostInfo;
        }

        public CombatEvent(Augment augment)
        {
            this.augment = augment;
            this.source = augment.entity;
            this.ability = augment.ability;
        }

        public CombatEvent(EntityInfo source)
        {
            this.source = source;
        }

        #endregion

        public virtual void SetTarget(EntityInfo target)
        {
            this.target = target;
        }

        public virtual void SetSource(EntityInfo source)
        {
            this.source = source;
        }
    }
    #endregion

    #region Health Change Event

    public enum eHealthEvent
    {
        OnDamageTaken,
        OnDamageDone,
        OnHealingTaken,
        OnHealingDone,
        BeforeDamageTaken,
        BeforeDamageDone,
        BeforeHealingTaken,
        BeforeHealingDone,
        OnDamagePreventedFromShields,
    }

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

    public enum eStateEvent
    {
        OnStatesChange,
        OnStatesNegated,
        OnAddImmuneState,
        OnRemoveImmuneState,
    }
    
    public class StateChangeEvent : CombatEvent
    {
        public List<EntityState> beforeEventStates { get; private set; }
        public List<EntityState> afterEventStates { get; private set; }
        public List<EntityState> statesInAction { get; private set; }

        public StateChangeEvent(BuffInfo buffInfo, List<EntityState> statesInAction ) : base(buffInfo)
        {
            beforeEventStates = target.states.ToList();
            this.statesInAction = statesInAction;
        }

        public StateChangeEvent(Augment augment, EntityInfo target, List<EntityState> statesInAction) : base(augment)
        {
            beforeEventStates = target.states.ToList();
            this.statesInAction = statesInAction;
        }

        public void SetAfterStates(List<EntityState> afterEventStates)
        {
            this.afterEventStates = afterEventStates.ToList();
        }

        public override void SetTarget(EntityInfo target)
        {
            base.SetTarget(target);
            beforeEventStates = target.states;
        }
    }
    #endregion

    #endregion

    #region Events
    public struct CombatEvents
    {

        #region General Combat Events
        public ArchEventHandler<eCombatEvent, CombatEvent> general { get; set; }

        public Action AddListenerGeneral(eCombatEvent trigger, Action<CombatEvent> action, MonoActor actor) => general.AddListener(trigger, action, actor);
        public void InvokeGeneral(eCombatEvent trigger, CombatEvent eventData) => general.Invoke(trigger, eventData);

        #endregion

        #region Health Events
        public ArchEventHandler<eHealthEvent, HealthCombatEvent> healthEvents { get; set; }

        public Action AddListenerHealth(eHealthEvent trigger, Action<HealthCombatEvent> action, MonoActor actor) => healthEvents.AddListener(trigger, action, actor);

        public void InvokeHealthChange(eHealthEvent trigger, HealthCombatEvent data) => healthEvents.Invoke(trigger, data);

        #endregion

        #region State Events
        public ArchEventHandler<eStateEvent, StateChangeEvent> stateChange { get; set; }

        public Action AddListenerStateEvent(eStateEvent trigger, Action<StateChangeEvent> action, MonoActor actor) => stateChange.AddListener(trigger, action, actor);

        public void InvokeStateEvent(eStateEvent trigger, StateChangeEvent data) => stateChange.Invoke(trigger, data);
        #endregion
        public void Initiate(EntityInfo source)
        {
            general = new(source);
            healthEvents = new(source);
            stateChange = new(source);
        }

        public Action<CombatEventData, bool> OnFixate;

        public Action<ThreatManager.ThreatInfo, float> OnGenerateThreat;
        public Action<EntityInfo, float> OnPingThreat { get; set; }
        public Action<EntityInfo> OnSummonEntity;

        #region State Change Events
        public Action<List<EntityState>, List<EntityState>> OnStatesChange { get; set; }

        #endregion

        #region Health Change Events
        public Action<CombatEventData> OnImmuneDamage { get; set; }
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