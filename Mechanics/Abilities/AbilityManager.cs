using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;
using System.Linq;

namespace Architome
{
    public class AbilityManager : MonoBehaviour
    {
        // Start is called before the first frame update
        public GameObject entityObject;
        public EntityInfo entityInfo;

        public List<AbilityInfo> abilities;
        public List<AbilityInfo> usableItems;

        public AbilityInfo attackAbility;
        public AbilityInfo currentlyCasting;
        public bool wantsToCastAbility;

        public GameObject target;
        public Vector3 location;

        //Events
        public Action<AbilityInfo> OnAbilityStart;
        public Action<AbilityInfo> WhileAbilityActive;
        public Action<AbilityInfo> OnAbilityEnd;
        public Action<AbilityInfo> OnTryCast;
        public Action<AbilityInfo> OnCastStart;
        public Action<AbilityInfo> OnCastRelease;
        public Action<AbilityInfo> OnCastEnd;
        public Action<AbilityInfo> OnCastReleasePercent;
        public Action<AbilityInfo> OnGlobalCoolDown;
        //public Action<AbilityInfo> OnCastChannelStart;
        //public Action<AbilityInfo> OnCastChannelInterval;
        //public Action<AbilityInfo> OnCastChannelEnd;
        public Action<AbilityInfo> OnChannelStart;
        public Action<AbilityInfo> OnChannelInterval;
        public Action<AbilityInfo> OnChannelEnd;
        public Action<AbilityInfo> OnCancelCast;
        public Action<AbilityInfo> OnCancelChannel;
        public Action<AbilityInfo> OnNewAbility;
        public Action<AbilityInfo, CatalystInfo> OnCatalystRelease;
        public Action<AbilityInfo> WhileCasting;
        public Action<AbilityInfo> WhileChanneling;
        public Action<AbilityInfo> OnDeadTarget;
        public Action<AbilityInfo, bool> OnWantsToCastChange;
        public Action<AbilityInfo, bool> OnCastChange;

        //Event Handlers
        bool isCasting;

        public void GetDependencies()
        {
            if (GetComponentInParent<EntityInfo>())
            {
                entityInfo = GetComponentInParent<EntityInfo>();
                entityObject = entityInfo.gameObject;

                entityInfo.OnLifeChange += OnLifeChange;
                entityInfo.combatEvents.OnStatesChange += OnStatesChange;
                entityInfo.OnChangeStats += OnChangeStats;

                if(entityInfo.Movement())
                {
                    entityInfo.Movement().OnStartMove += OnStartMove;
                }
            }

            if (abilities.Count == 0)
            {
                foreach (Transform child in transform)
                {
                    if (child.GetComponent<AbilityInfo>())
                    {
                        if (child.GetComponent<AbilityInfo>().isAttack)
                        {
                            attackAbility = child.GetComponent<AbilityInfo>();
                        }
                        else
                        {
                            abilities.Add(child.GetComponent<AbilityInfo>());
                        }
                        child.GetComponent<AbilityInfo>().active = true;
                    }
                }
            }



        }
        void Start()
        {
            GetDependencies();
        }

        private void Update()
        {
            if (isCasting != (currentlyCasting != null))
            {
                isCasting = currentlyCasting != null;
                OnCastChange?.Invoke(currentlyCasting, isCasting);
            }
        }

        public void OnLifeChange(bool isAlive)
        {
            SetAbilities(isAlive, isAlive);
        }

        public void OnStatesChange(List<EntityState> previous, List<EntityState> states)
        {
            if(!entityInfo.isAlive) { return; }
            var interruptStates = new List<EntityState>() 
            {
            EntityState.Stunned,
            EntityState.Silenced
            };

            var intersection = states.Intersect(interruptStates).ToList();

            if (intersection.Count > 0)
            {
                if (currentlyCasting != null) { currentlyCasting.CancelCast("State changed to interrupted state"); }
                
                if(states.Contains(EntityState.Silenced))
                {
                    SetAbilities(false);
                }

                if (states.Contains(EntityState.Stunned))
                {
                    SetAbilities(false, false);
                }


                return;
            }

            SetAbilities(true);
        }

        public void SetAbilities(bool val, bool canAutoAttack = true)
        {
            var abilities = GetComponentsInChildren<AbilityInfo>();

            foreach(var ability in abilities)
            {
                if(ability == attackAbility)
                {
                    ability.active = canAutoAttack;
                    continue;
                }
                ability.active = val;
            }
        }

        public void OnChangeStats(EntityInfo entity)
        {
            foreach (var ability in GetComponentsInChildren<AbilityInfo>())
            {
                ability.UpdateAbility();
            }
        }

        public void OnStartMove(Movement movement)
        {
            if(currentlyCasting && currentlyCasting.cancelCastIfMoved)
            {
                currentlyCasting.CancelCast("Moved on non movable ability");
            }
        }

        public bool IsCasting()
        {
            if(currentlyCasting == null) { return false; }
            if(currentlyCasting.isAttack) { return false; }


            return true;
        }

        public AbilityInfo Ability(int num)
        {
            if (abilities.Count <= num) { return null; }

            return abilities[num];

        }
        public void Cast(AbilityInfo ability)
        {
            if (!entityInfo.isAlive) { return; }
            if (abilities.Contains(ability))
            {
                Debugger.InConsole(7829, $"{ability}, {abilities.IndexOf(ability)}");
                if (entityInfo && entityInfo.PlayerController())
                {
                    entityInfo.PlayerController().HandlePlayerTargetting();
                    Cast(abilities.IndexOf(ability));
                }
            }
        }

        // Update is called once per frame
        public void Cast(int value)
        {
            if (!entityInfo.isAlive) { return; }
            if (value >= abilities.Count)
            {
                return;
            }


            Debugger.InConsole(1115, $"Your code made it this far");

            if (target) { abilities[value].target = target; }
            else { abilities[value].target = null; }

            abilities[value].location = location;
            abilities[value].Cast();
        }
        public void Attack()
        {
            if (target) { attackAbility.target = target; }
            else { attackAbility.target = null; }

            if (location != null) { attackAbility.location = location; }
            attackAbility.Cast();
        }
        public AbilityInfo CreateNewAbility(GameObject abilityObject)
        {
            if (!abilityObject.GetComponent<AbilityInfo>()) return null;

            var newAbility = Instantiate(abilityObject, transform);
            abilities.Add(newAbility.GetComponent<AbilityInfo>());
            OnNewAbility?.Invoke(newAbility.GetComponent<AbilityInfo>());

            return newAbility.GetComponent<AbilityInfo>();
        }

        public AbilityInfo Ability(AbilityType2 type)
        {
            if (type == AbilityType2.AutoAttack)
            {
                return attackAbility;
            }

            foreach (AbilityInfo ability in abilities)
            {
                if (type == ability.abilityType2)
                {
                    return ability;
                }
            }

            return null;
        }
    }
}

