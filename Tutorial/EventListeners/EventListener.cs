using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.CompilerServices;
using UnityEditor;

namespace Architome.Tutorial
{
    public class EventListener : MonoBehaviour
    {
        public bool completed;
        public string title;
        [Multiline] public string description;
        [Multiline] public string tip;
        [SerializeField] Transform trailTarget;
        [SerializeField] bool enableTrail;

        public event Action<EventListener> OnSuccessfulEvent;
        public event Action<EventListener> OnFailEvent;
        public event Action<EventListener> OnStartEvent;
        public event Action<EventListener> OnEndEvent;

        public float extraSuccessfulTime = 0f;


        [Serializable]
        public struct UnityEvents
        {
            public UnityEvent OnSuccessfulEvent, OnFailEvent, OnStartEvent, OnEndEvent;
        }

        [Serializable]
        public struct Settings
        {
            public List<EntityInfo> preventEntitiesDeath;
            public List<EntityInfo> preventEntityDeathBeforeStart;
            public List<EntityInfo> preventEntitiesDamage;
            public List<EntityInfo> preventEntitiesTargetedBeforeStart;
            public List<AbilityInfo> preventAbilitiesUntilStart;
            public List<EntityInfo> waitEntityCombat;
            public float entityDeathPreventionDelay;
        }

        public Settings settings;

        public UnityEvents events;

        public bool listenOnStart;
        [SerializeField] protected bool simple;
        [SerializeField] protected bool initiated;


        protected KeyBindings keyBindData;
        protected ActionBarsInfo actionBarsManager;
        protected WorldActions actions;
        public void GetDependencies()
        {
            keyBindData = KeyBindings.active;
            actionBarsManager = ActionBarsInfo.active;
            actions = WorldActions.active;
        }

        public void HandleTrailEmission()
        {
            if (!enableTrail) return;
            if (trailTarget == null) return;

            var trailEmitter = TrailEmitter.activeTrailEmitter;
            if (trailEmitter == null) return;

            OnStartEvent += (EventListener listener) => {
                trailEmitter.SetTrail(trailTarget);
                trailEmitter.SetEmission(true);
            };

            OnEndEvent += (EventListener listener) => {
                trailEmitter.SetEmission(false);
            };
        }


        protected void HandleStart()
        {
            if (initiated) return;
            if (listenOnStart)
            {
                StartEventListener();
            }

            HandleSettings();
        }

        void HandleSettings()
        {
            if (settings.preventEntitiesDeath != null)
            {
                foreach (var entity in settings.preventEntitiesDeath)
                {
                    PreventEntityDeath(entity);
                }
            }

            if (settings.preventEntityDeathBeforeStart != null)
            {
                foreach(var entity in settings.preventEntityDeathBeforeStart)
                {
                    PreventEntityDeathBeforeStart(entity);
                }
            }

            if(settings.preventEntitiesDamage != null)
            {
                foreach (var entity in settings.preventEntitiesDamage)
                {
                    PreventEntityDamage(entity);
                }
            }

            if (settings.preventAbilitiesUntilStart != null)
            {
                foreach (var ability in settings.preventAbilitiesUntilStart)
                {
                    ArchAction.Delay(() => {
                        PreventAbilityBeforeStart(ability);
                    }, 3f);
                }
            }

            if(settings.preventEntitiesTargetedBeforeStart != null)
            {
                foreach(var entity in settings.preventEntitiesTargetedBeforeStart)
                {
                    PreventEntityTargetedBeforeStart(entity);
                }
            }

        }

        public void PreventAbilityBeforeStart(AbilityInfo ability)
        {
            ability.active = false;

            Action<AbilityInfo, bool> action = delegate (AbilityInfo current, bool active)
            {
                if (active)
                {
                    current.active = false;
                }
            };

            ability.OnActiveChange += action;

            OnStartEvent += (EventListener listener) =>
            {
                ability.OnActiveChange -= action;
                ability.active = true;
            };
        }

        public void PreventEntityDeath(EntityInfo entity)
        {
            entity.OnLifeChange += OnEntityLifeChange;

            OnSuccessfulEvent += (EventListener listener) => {
                entity.OnLifeChange -= OnEntityLifeChange;
            };

            void OnEntityLifeChange(bool isAlive)
            {
                ArchAction.Delay(() => {
                    if (completed) return;
                    actions.Revive(entity, entity.transform.position);
                
                }, settings.entityDeathPreventionDelay > 0 ? settings.entityDeathPreventionDelay : 2f);
            }
        }

        public void PreventEntityTargetedBeforeStart(EntityInfo entity)
        {
            if (initiated) return;
            Action<List<bool>> action = (List<bool> checks) => {
                checks.Add(false);
            };

            entity.combatEvents.OnCanBeAttackedCheck += action;
            entity.combatEvents.OnCanBeHelpedCheck += action;

            OnStartEvent += (EventListener listener) => {
                entity.combatEvents.OnCanBeAttackedCheck -= action;
                entity.combatEvents.OnCanBeHelpedCheck -= action;
            };
        }

        public void PreventEntityDamage(EntityInfo entity)
        {
            Action<CombatEventData> action = (CombatEventData eventData) => {
                eventData.value = 0f;
            };

            entity.combatEvents.BeforeDamageDone += action;
            entity.combatEvents.BeforeDamageTaken += action;

            OnEndEvent += (EventListener listener) => {
                entity.combatEvents.BeforeDamageTaken -= action;
                entity.combatEvents.BeforeDamageDone -= action;
            
            };
        }

        public void PreventEntityDeathBeforeStart(EntityInfo entity)
        {
            if (initiated) return;
            entity.OnLifeChange += OnEntityLifeChange;

            OnStartEvent += (EventListener listener) => {
                entity.OnLifeChange -= OnEntityLifeChange;
            };

            void OnEntityLifeChange(bool isAlive)
            {
                ArchAction.Delay(() => {
                    if (initiated) return;
                    actions.Revive(entity, entity.transform.position);

                }, settings.entityDeathPreventionDelay > 0 ? settings.entityDeathPreventionDelay : 2f);
            }
        }

        public int MemberIndex(EntityInfo entity)
        {
            int memberIndex = 0;
            var party = entity.GetComponentInParent<PartyInfo>();
            if (party == null) return memberIndex;
            var members = party.GetComponentsInChildren<EntityInfo>();

            for (int i = 0; i < members.Length; i++)
            {
                if (members[i] != entity) continue;
                memberIndex = i;

                break;
            }

            return memberIndex;
        }
        async Task EntityCombat()
        {
            if (settings.waitEntityCombat == null) return;

            while (true)
            {
                var isInCombat = false;

                foreach(var entity in settings.waitEntityCombat)
                {
                    if (entity.isInCombat)
                    {
                        isInCombat = true;
                    }
                }

                if (!isInCombat) break;
                await Task.Delay(500);
            }

        }

        protected async void CompleteEventListener()
        {
            if (completed) return;
            completed = true;

            await EntityCombat();

            OnSuccessfulEvent?.Invoke(this);
            events.OnSuccessfulEvent?.Invoke();
            OnEndEvent?.Invoke(this);
            events.OnEndEvent?.Invoke();

            Debugger.Environment(4325, $"Completed event {title}");
        } 

        protected void FailEventListeners()
        {
            OnFailEvent?.Invoke(this);
            events.OnFailEvent?.Invoke();

            OnEndEvent?.Invoke(this);
            events.OnEndEvent?.Invoke();
        }

        public virtual string Directions()
        {
            return description;
        }

        public virtual string Tips()
        {
            return tip;
        }


        public string NotificationDescription()
        {
            var stringList = new List<string>() {
                Directions(),
            };

            return ArchString.NextLineList(stringList, 1);
        }



        public virtual void StartEventListener()
        {
            initiated = true;
            OnStartEvent?.Invoke(this);
            events.OnStartEvent?.Invoke();
        }
    }
}
