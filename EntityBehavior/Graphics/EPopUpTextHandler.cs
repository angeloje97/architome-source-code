using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Architome
{
    public class EPopUpTextHandler : EntityProp
    {
        #region Common Data

        public PopupTextManager popUpManager;
        public List<EntityState> previousStates;
        #endregion

        #region Initiation

        public override async Task GetDependenciesTask()
        {
            popUpManager = PopupTextManager.active;

            if (popUpManager == null) return;


            await Task.Delay(250);
            if (entityInfo)
            {

                if (entityInfo.rarity == EntityRarity.Player)
                {
                    entityInfo.OnLifeChange += OnLifeChange;
                    OnBeforeIsPlayer += combatEvents.AddListenerGeneral(eCombatEvent.OnFixateChange, OnFixate, this);

                    entityInfo.OnLevelUp += OnLevelUp;
                    entityInfo.OnNewBuff += OnNewBuff;
                }
                else
                {
                    OnAfterIsPlayer += combatEvents.AddListenerHealth(eHealthEvent.OnDamageTaken, OnDamageTaken, this);
                    //combatEvents.OnImmuneDamage += OnImmuneDamage;
                    OnAfterIsPlayer += combatEvents.AddListenerHealth(eHealthEvent.OnImmuneDamage, OnImmuneDamage, this);

                }

                entityInfo.infoEvents.OnRarityChange += OnRarityChange;


                combatEvents.AddListenerStateEvent(eStateEvent.OnStatesChange, OnStatesChange, this);
                combatEvents.AddListenerStateEvent(eStateEvent.OnStatesNegated, OnStateNegated, this);
                combatEvents.AddListenerStateEvent(eStateEvent.OnAddImmuneState, OnAddImmuneState, this);
                combatEvents.AddListenerStateEvent(eStateEvent.OnRemoveImmuneState, OnRemoveImmuneState, this);
            }

            
        }

        #endregion

        Action OnBeforeIsPlayer;
        Action OnAfterIsPlayer;

        #region Event Listeners
        void OnRarityChange(EntityRarity before, EntityRarity after)
        {
            if (before == EntityRarity.Player)
            {
                InvokePlayerChange(false);
                OnAfterIsPlayer += combatEvents.AddListenerHealth(eHealthEvent.OnDamageTaken, OnDamageTaken, this);
                OnAfterIsPlayer +=  combatEvents.AddListenerHealth(eHealthEvent.OnImmuneDamage, OnImmuneDamage, this);

                entityInfo.OnLifeChange -= OnLifeChange;
                entityInfo.OnLevelUp -= OnLevelUp;
                entityInfo.OnNewBuff -= OnNewBuff;


            }

            if (after == EntityRarity.Player)
            {
                InvokePlayerChange(true);
                OnBeforeIsPlayer += combatEvents.AddListenerGeneral(eCombatEvent.OnFixateChange, OnFixate, this);

                entityInfo.OnLifeChange += OnLifeChange;
                entityInfo.OnLevelUp += OnLevelUp;
                entityInfo.OnNewBuff += OnNewBuff;

            }
        }

        public void InvokePlayerChange(bool afterIsPlayer)
        {
            if (afterIsPlayer)
            {
                OnAfterIsPlayer?.Invoke();
                OnAfterIsPlayer = null;
            }
            else
            {
                OnBeforeIsPlayer?.Invoke();
                OnBeforeIsPlayer = null;
            }
        }


        async void OnStatesChange(StateChangeEvent eventData)
        {
            if (popUpManager == null) return;
            var previous = eventData.beforeEventStates;
            var after = eventData.afterEventStates;

            var removedStates = previous.Except(after).ToList();
            var newStates = after.Except(previous).ToList();

            foreach (var removed in removedStates)
            {
                var toString = removed.ToString();
                popUpManager.StateChangePopUp(new(transform, $"{toString} faded"));
                await Task.Delay(250);
            }

            foreach (var state in newStates)
            {
                var toString = state.ToString();
                popUpManager.StateChangePopUp(new(transform, $"{toString}"));
                await Task.Delay(250);
            }
        }

        async void OnNewBuff(BuffInfo buff, EntityInfo source)
        {
            if (!buff.GetComponent<BuffShield>()) return;
            await Task.Delay(125);
            var shieldValue = buff.GetComponent<BuffShield>().value;

            buff.OnBuffEnd += OnBuffEnd;

            popUpManager.DamagePopUp(new(transform, $"Shielded {ArchString.FloatToSimple(shieldValue, 0)}"));
        }

        void OnBuffEnd(BuffInfo buff)
        {
            if (buff.GetComponent<BuffShield>())
            {
                popUpManager.DamagePopUp(new(transform, $"Shield Faded"));
            }

            buff.OnBuffEnd -= OnBuffEnd;
        }

        public void OnFixate(CombatEvent eventData)
        {
            if (popUpManager == null) return;

            var fixate = eventData.fixated ? "Fixated" : "Fixated Faded";
            
            popUpManager.StateChangePopUp(new(transform, fixate));
        }

        // Update is called once per frame

        Dictionary<DamageType, PopupText> popUpDamageType;
        Dictionary<DamageType, float> popUpDamageValues;
        bool initializedDamageType = false;
        float previousAngle = 0f;

        void OnDamageTaken(HealthEvent eventData)
        {
            if (eventData.value <= 0) return;
            var value = eventData.value;


            var damageType = eventData.DataDamageType();


            HandleInitialization();
            HandleDynamicDamage();

            void HandleDynamicDamage()
            {
                if (CanUpdate()) return;

                var currentValue = popUpDamageValues[damageType] = value;
                var newPopup = popUpManager.DamagePopUp(new(transform, $"{ArchString.FloatToSimple(currentValue, 0)}") { previousAngle = previousAngle }, damageType);
                popUpDamageType[damageType] = newPopup;

                previousAngle = newPopup.previousAngle;
                

                bool CanUpdate()
                {
                    var currentPopup = popUpDamageType[damageType];

                    if (currentPopup == null) return false;
                    if (popUpManager.damagePopUpType == eDamagePopUpType.Once) return false;
                    if (currentPopup.blockUpdate) return false;

                    popUpDamageValues[damageType] += value;
                    var updatedValue = ArchString.FloatToSimple(popUpDamageValues[damageType], 0);
                    popUpDamageType[damageType].UpdatePopUp(new(transform, updatedValue)
                    {
                        trigger = PopupText.eAnimatorTrigger.HealthChangeRepeat,
                    });
                    return true;
                }
            }

            void HandleInitialization()
            {
                if (initializedDamageType) return;
                initializedDamageType = true;
                popUpDamageType = new();
                popUpDamageValues = new();

                var enums = Enum.GetValues(typeof(DamageType));

                foreach (DamageType value in enums)
                {
                    popUpDamageType.Add(value, null);
                    popUpDamageValues.Add(value, 0);
                }
            }
        }

        void OnImmuneDamage(HealthEvent eventData)
        {
            popUpManager.DamagePopUp(new(transform, $"Immune"), DamageType.True);
        }

        async void OnStateNegated(StateChangeEvent eventData)
        {
            if (popUpManager == null) return;
            Debugger.UI(8342, $"{eventData.statesInAction.Count}");
            foreach(var state in eventData.statesInAction)
            {
                popUpManager.StateChangeImmunityPopUp(new( transform, state.ToString() ));
                await Task.Delay(250);

            }
        }

        async void OnAddImmuneState(StateChangeEvent eventData)
        {
            if (popUpManager == null) return;
            foreach(var state in eventData.statesInAction)
            {
                popUpManager.StateChangePopUp(new(transform, $"Immune to {state}"));

                await Task.Delay(250);
            }
        }


        async void OnRemoveImmuneState(StateChangeEvent eventData)
        {

            if (popUpManager == null) return;
            
            foreach(var state in eventData.statesInAction)
            {
                popUpManager.StateChangePopUp(new(transform, $"Removed immunity to {state}"));
                await Task.Delay(250);
            }
        }

        void OnLifeChange(bool isAlive)
        {
            if (!Entity.IsPlayer(entityInfo.gameObject)) return;

            if (isAlive)
            {
                popUpManager.StateChangePopUp(new( transform, "Revived"));
                return;
            }
            else
            {
                popUpManager.StateChangePopUp(new( transform, "Dead"));
            }
            
        }

        void OnLevelUp(int level)
        {
            if (popUpManager == null) return;

            popUpManager.StateChangePopUp(new(transform, $"Level {level}"));
        }

        #endregion
    }

}
