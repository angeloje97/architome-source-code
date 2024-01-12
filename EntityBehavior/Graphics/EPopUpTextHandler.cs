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
        // Start is called before the first frame update

        public PopupTextManager popUpManager;
        public List<EntityState> previousStates;

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
                        entityInfo.combatEvents.OnFixate += OnFixate;
                        entityInfo.OnLevelUp += OnLevelUp;
                        entityInfo.OnNewBuff += OnNewBuff;
                    }
                    else
                    {
                        entityInfo.OnDamageTaken += OnDamageTaken;
                        entityInfo.combatEvents.OnImmuneDamage += OnImmuneDamage;

                    }

                    entityInfo.infoEvents.OnRarityChange += OnRarityChange;

                    entityInfo.combatEvents.OnStateNegated += OnStateNegated;
                    entityInfo.combatEvents.OnStatesChange += OnStatesChange;
                    entityInfo.combatEvents.OnAddImmuneState += OnAddImmuneState;
                    entityInfo.combatEvents.OnRemoveImmuneState += OnRemoveImmuneState;
                }

            
        }
        
        void OnRarityChange(EntityRarity before, EntityRarity after)
        {
            if (before == EntityRarity.Player)
            {
                entityInfo.OnLifeChange -= OnLifeChange;
                entityInfo.combatEvents.OnFixate -= OnFixate;
                entityInfo.OnLevelUp -= OnLevelUp;
                entityInfo.OnNewBuff -= OnNewBuff;

                entityInfo.OnDamageTaken += OnDamageTaken;
                entityInfo.combatEvents.OnImmuneDamage += OnImmuneDamage;
            }

            if (after == EntityRarity.Player)
            {
                entityInfo.OnLifeChange += OnLifeChange;
                entityInfo.combatEvents.OnFixate += OnFixate;
                entityInfo.OnLevelUp += OnLevelUp;
                entityInfo.OnNewBuff += OnNewBuff;

                entityInfo.OnDamageTaken -= OnDamageTaken;
                entityInfo.combatEvents.OnImmuneDamage -= OnImmuneDamage;
            }


            
        }


        async void OnStatesChange(List<EntityState> previous, List<EntityState> after)
        {
            if (popUpManager == null) return;

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

        public void OnFixate(CombatEventData eventData, bool fixated)
        {
            if (popUpManager == null) return;

            var fixate = fixated ? "Fixated" : "Fixated Faded";
            
            popUpManager.StateChangePopUp(new(transform, fixate));
        }

        // Update is called once per frame

        Dictionary<DamageType, PopupText> popUpDamageType;
        Dictionary<DamageType, float> popUpDamageValues;
        bool initializedDamageType = false;

        void OnDamageTaken(CombatEventData eventData)
        {
            if (eventData.value <= 0) return;
            var value = eventData.value;


            var damageType = eventData.DataDamageType();


            HandleInitialization();
            HandleDynamicDamage();

            void HandleDynamicDamage()
            {

                

                var currentPopup = popUpDamageType[damageType];
                if (currentPopup != null && currentPopup.ContainsState(PopupText.eAnimatorState.EnableRepeat))
                {
                    popUpDamageValues[damageType] += value;
                    var updatedValue = ArchString.FloatToSimple(popUpDamageValues[damageType], 0);
                    popUpDamageType[damageType].UpdatePopUp(new(transform, updatedValue) { trigger = PopupText.eAnimatorTrigger.HealthChangeRepeat });
                    return;
                }

                var currentValue = popUpDamageValues[damageType] = value;
                popUpDamageType[damageType] = popUpManager.DamagePopUp(new(transform, $"{ArchString.FloatToSimple(currentValue, 0)}"), damageType);


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

        void OnImmuneDamage(CombatEventData eventData)
        {
            popUpManager.DamagePopUp(new(transform, $"Immune"), DamageType.True);
        }

        void OnStateNegated(List<EntityState> currentState, EntityState negatedState)
        {
            if (popUpManager == null) return;

            popUpManager.StateChangeImmunityPopUp(new( transform, negatedState.ToString() ));
        }

        void OnAddImmuneState(EntityState newState)
        {
            if (popUpManager == null) return;
            popUpManager.StateChangePopUp(new(transform, $"Immune to {newState}"));
        }

        void OnRemoveImmuneState(EntityState removedState)
        {
            if (popUpManager == null) return;
            popUpManager.StateChangePopUp(new(transform, $"Removed immunity to {removedState}"));
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

    }

}
