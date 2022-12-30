using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Architome
{
    public class EPopUpTextHandler : EntityProp
    {
        // Start is called before the first frame update

        public PopupTextManager popUpManager;
        public List<EntityState> previousStates;

        new void GetDependencies()
        {
            base.GetDependencies();

            popUpManager = PopupTextManager.active;

            if (popUpManager == null) return;


            ArchAction.Delay(() => {
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
            }, .25f);
        }


        void Start()
        {
            GetDependencies();
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
                popUpManager.StateChangePopUp(transform, $"{toString} faded");
                await Task.Delay(250);
            }

            foreach (var state in newStates)
            {
                var toString = state.ToString();
                popUpManager.StateChangePopUp(transform, $"{toString}");
                await Task.Delay(250);
            }
        }

        async void OnNewBuff(BuffInfo buff, EntityInfo source)
        {
            if (!buff.GetComponent<BuffShield>()) return;
            await Task.Delay(125);
            var shieldValue = buff.GetComponent<BuffShield>().shieldAmount;

            buff.OnBuffEnd += OnBuffEnd;

            popUpManager.DamagePopUp(transform, $"Shielded {ArchString.FloatToSimple(shieldValue)}");
        }

        void OnBuffEnd(BuffInfo buff)
        {
            if (buff.GetComponent<BuffShield>())
            {
                popUpManager.DamagePopUp(transform, $"Shield Faded");
            }

            buff.OnBuffEnd -= OnBuffEnd;
        }

        public void OnFixate(CombatEventData eventData, bool fixated)
        {
            if (popUpManager == null) return;

            var fixate = fixated ? "Fixated" : "Fixated Faded";
            
            popUpManager.StateChangePopUp(transform, fixate);
        }

        // Update is called once per frame
        void OnDamageTaken(CombatEventData eventData)
        {
            if (eventData.value <= 0) return;
            var value = eventData.value;


            var damageType = eventData.DataDamageType();

            popUpManager.DamagePopUp(transform, $" {ArchString.FloatToSimple(value,0)}", damageType);
        }

        void OnImmuneDamage(CombatEventData eventData)
        {
            popUpManager.DamagePopUp(transform, $"Immune", DamageType.True);
        }

        void OnStateNegated(List<EntityState> currentState, EntityState negatedState)
        {
            if (popUpManager == null) return;

            popUpManager.StateChangeImmunityPopUp(transform, negatedState.ToString());
        }

        void OnAddImmuneState(EntityState newState)
        {
            if (popUpManager == null) return;
            popUpManager.StateChangePopUp(transform, $"Immune to {newState}");
        }

        void OnRemoveImmuneState(EntityState removedState)
        {
            if (popUpManager == null) return;
            popUpManager.StateChangePopUp(transform, $"Removed immunity to {removedState}");
        }

        void OnLifeChange(bool isAlive)
        {
            if (!Entity.IsPlayer(entityInfo.gameObject)) return;

            if (isAlive)
            {
                popUpManager.StateChangePopUp(transform, "Revived");
                return;
            }
            else
            {
                popUpManager.StateChangePopUp(transform, "Dead");
            }
            
        }

        void OnLevelUp(int level)
        {
            if (popUpManager == null) return;

            popUpManager.StateChangePopUp(transform, $"Level {level}");
        }

    }

}
