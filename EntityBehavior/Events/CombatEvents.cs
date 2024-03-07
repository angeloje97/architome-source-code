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

        //Deaths
        OnGetRevive,
        OnGiveRevive,
        OnDeath,
        OnKill,
        OnKillPlayer,
        OnNewCombatTarget,

        //Can Help Can Attack

        OnCanAttackCheck,
        OnCanHelpCheck,
        OnCanBeAttackedCheck,
        OnCanBeHelpedCheck,

        //Summoning
        OnSummonEntity,

        //Fixation
        OnFixateChange,
    }

    public class CombatEvent
    {
        #region Common Data
        public EntityInfo source { get; private set; }
        public EntityInfo target { get; private set; }
        public EntityInfo previousTarget { get; set; }
        public CatalystInfo catalyst { get; private set; }

        public Augment augment { get; private set; }
        public AbilityInfo ability { get; private set; }
        public BuffInfo buff { get; private set; }

        public bool fixated { get; private set; }

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

        public CombatEvent(EntityInfo target, EntityProp sourceProp)
        {
            this.target = target;
            this.source = sourceProp.entityInfo;
        }

        public CombatEvent(EntityInfo target)
        {
            this.target = target;
        }

        public CombatEvent(EntityInfo source, EntityInfo target)
        {
            this.target = target;
            this.source = source;
        }

        public CombatEvent(EntityInfo source, EntityInfo previousTarget, EntityInfo target)
        {
            this.source = source;
            this.previousTarget = previousTarget;
            this.target = target;
        }

        public CombatEvent(CombatEventData eventData)
        {
            catalyst = eventData.catalyst;
            buff = eventData.buff;
            ability = eventData.ability;
            source = eventData.source;
            target = eventData.target;
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

        public virtual void SetFixate(bool fixate)
        {
            fixated = fixate;
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
        OnImmuneDamage,
        OnDamagePreventedFromShields,
    }

    public class HealthEvent : CombatEvent
    {
        public DamageType damageType { get; private set; }
        public float value { get; private set; }

        public float shieldValue { get; private set; }

        public float percentValue { get; private set; }

        public bool critical { get; private set; }

        public bool lethalDamage { get; set; }

        public HealthEvent(CatalystInfo catalyst, EntityInfo source, float value) : base(catalyst, source)
        {
            this.value = value;
        }

        public HealthEvent(BuffInfo buffInfo, float value) : base(buffInfo)
        {
            this.value = value;
        }

        public HealthEvent(CombatEventData data) : base(data)
        {

        }

        public DamageType DataDamageType()
        {
            if (buff)
            {
                return buff.damageType;
            }

            if (catalyst)
            {
                return catalyst.damageType;
            }

            return DamageType.True;
        }

        public void SetValue(float newValue)
        {
            value = newValue;
        }

        public void SetCritical(bool critical)
        {
            this.critical = critical;
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

    #region Threat Event

    public enum eThreatEvent
    {
        OnPingThreat,
        OnGenerateThreat,
        OnFirstThreatWithPlayer,
    }

    public class ThreatEvent : CombatEvent
    {

        public ThreatManager.ThreatInfo threatInfo { get; private set; }

        public float threatValue;

        //Attacker = Source
        //Owner = Target

        public ThreatEvent(ThreatManager.ThreatInfo threat)
        {
            this.threatInfo = threat;
            SetSource(threat.threatInfo);
            SetTarget(threat.sourceInfo);
            threatValue = threat.threatValue;
        }

        public ThreatEvent(ThreatManager.ThreatInfo threat, float newValue)
        {
            this.threatInfo = threat;
            SetSource(threat.threatInfo);
            SetTarget(threat.sourceInfo);
            threatValue = newValue;
        }

        public ThreatEvent(EntityInfo source, EntityInfo target, float value) : base(source, target)
        {
            this.threatValue = value;
        }
    }

    #endregion
    #endregion

    #region Events
    public struct CombatEvents
    {

        #region General Combat Events
        ArchEventHandler<eCombatEvent, CombatEvent> general { get; set; }

        public Action AddListenerGeneral(eCombatEvent trigger, Action<CombatEvent> action, MonoActor actor) => general.AddListener(trigger, action, actor);

        public Action AddListenerGeneralCheck(eCombatEvent trigger, Action<CombatEvent, List<bool>> action, MonoActor actor)
        {
            return general.AddListenerCheck(trigger, action, actor);
        }

        public bool InvokeCheckGeneral(eCombatEvent trigger, CombatEvent eventData)
        {
            return general.InvokeCheck(trigger, eventData);
        }

        public void InvokeGeneral(eCombatEvent trigger, CombatEvent eventData) => general.Invoke(trigger, eventData);

        #endregion

        #region Health Events
        ArchEventHandler<eHealthEvent, HealthEvent> healthEvents { get; set; }

        public Action AddListenerHealth(eHealthEvent trigger, Action<HealthEvent> action, MonoActor actor) => healthEvents.AddListener(trigger, action, actor);

        public void InvokeHealthChange(eHealthEvent trigger, HealthEvent data) => healthEvents.Invoke(trigger, data);

        #endregion

        #region State Events
        ArchEventHandler<eStateEvent, StateChangeEvent> stateChange { get; set; }

        public Action AddListenerStateEvent(eStateEvent trigger, Action<StateChangeEvent> action, MonoActor actor) => stateChange.AddListener(trigger, action, actor);

        public void InvokeStateEvent(eStateEvent trigger, StateChangeEvent data) => stateChange.Invoke(trigger, data);
        public bool InvokeStateCheck(eStateEvent trigger, StateChangeEvent data) => stateChange.InvokeCheck(trigger, data);

        #endregion

        #region Threat Events

        ArchEventHandler<eThreatEvent, ThreatEvent> threatEvents;

        public Action AddListenerThreat(eThreatEvent trigger, Action<ThreatEvent> action, MonoActor actor) => threatEvents.AddListener(trigger, action, actor);

        public void InvokeThreat(eThreatEvent trigger, ThreatEvent threatEvent) => threatEvents.Invoke(trigger, threatEvent);

        #endregion
        public void Initiate(EntityInfo source)
        {
            general = new(source);
            healthEvents = new(source);
            stateChange = new(source);
            threatEvents = new(source);
        }

        #region Health Change Events
        public Action<CombatEventData> OnImmuneDamage { get; set; }
        public Action<CombatEventData> BeforeDamageTaken { get; set; }

        public Action<EntityInfo, List<Func<float>>> OnUpdateShield { get; set; }

        public Action<EntityInfo, List<Func<float>>> OnUpdateHealAbsorbShield { get; set; }
        #endregion

        #region Generic

        public Action<EntityInfo> OnSummonEntity { get; set; }
        #endregion
    }

    #endregion
}