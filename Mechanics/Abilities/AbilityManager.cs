using System;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using JetBrains.Annotations;

namespace Architome
{
    [RequireComponent(typeof(AbilityFXHandler))]
    public class AbilityManager : MonoBehaviour
    {

        [Serializable]
        public struct Debugging
        {
            [SerializeField] bool abilities;

            public void Log(int id, string text)
            {
                if (!abilities) return;

                Debugger.Combat(id, text);
            }
        }

        [Serializable]
        public class ManagerSettings
        {
            public bool useWeaponAbility;
        }
        public ManagerSettings settings;

        public Debugging debugger;

        [Header("In Game Properties")]

        public GameObject entityObject;
        public EntityInfo entityInfo;

        public List<AbilityInfo> abilities;
        public HashSet<AbilityInfo> abilityMap;
        public List<AbilityInfo> usableItems;
        public CharacterInfo character;

        public AbilityInfo attackAbility;
        public AbilityInfo currentlyCasting;
        public AbilityInfo currentWantsToCast;
        public Weapon currentWeapon;
        public bool wantsToCastAbility;

        public EntityInfo target;
        public Vector3 location;

        public class Events
        {
            public Action<AbilityInfo> OnCastStart;
            public Action<AbilityInfo> OnCastEnd;
            public Action<AbilityInfo> OnCastReleasePercent { get; set; }
            public Action<AbilityInfo> OnCastRelease;
            public Action<AbilityInfo> OnAttack;

            public Action<EntityInfo, List<bool>> OnUseAuto;
        }

        public Events events
        {
            get
            {
                return entityInfo.abilityEvents;
            }
        }

        //Events
        public Action<AbilityInfo> OnAbilityStart;
        public Action<AbilityInfo> WhileAbilityActive;
        public Action<AbilityInfo> OnAbilityEnd;
        public Action<AbilityInfo> OnTryCast;
        //public Action<AbilityInfo> OnCastStart;
        //public Action<AbilityInfo> OnCastRelease { get; set; }
        //public Action<AbilityInfo> OnCastEnd;
        //public Action<AbilityInfo> OnCastReleasePercent;
        public Action<AbilityInfo> OnGlobalCoolDown { get; set; }
        //public Action<AbilityInfo> OnCastChannelStart;
        //public Action<AbilityInfo> OnCastChannelInterval;
        //public Action<AbilityInfo> OnCastChannelEnd;
        public Action<AbilityInfo, AugmentChannel> OnChannelStart { get; set; }
        public Action<AbilityInfo, AugmentChannel> OnChannelInterval;
        public Action<AbilityInfo, AugmentChannel> OnChannelEnd;
        public Action<AbilityInfo, AugmentChannel> OnCancelChannel;
        public Action<AbilityInfo> OnCancelCast;
        public Action<AbilityInfo> OnNewAbility { get; set; }
        public Action<AbilityInfo, CatalystInfo> OnCatalystRelease { get; set; }
        public Action<AbilityInfo> WhileCasting;
        public Action<AbilityInfo> WhileChanneling;
        public Action<AbilityInfo> OnDeadTarget;
        public Action<AbilityInfo, bool> OnWantsToCastChange;
        public Action<AbilityInfo, bool> OnCastChange;
        public Action<AbilityInfo, EntityInfo> OnTryAttackTarget { get; set; }
        public Action<AbilityInfo> OnAbilityUpdate { get; set; }

        //Event Handlers
        bool isCasting;
        EquipmentSlot[] equipmentSlots;

        public void GetDependencies()
        {
            entityInfo = GetComponentInParent<EntityInfo>();


            if (entityInfo)
            {
                entityObject = entityInfo.gameObject;

                entityInfo.OnLifeChange += OnLifeChange;
                entityInfo.combatEvents.OnStatesChange += OnStatesChange;
                entityInfo.OnChangeStats += OnChangeStats;
                character = entityInfo.GetComponentInChildren<CharacterInfo>();
                var movement = entityInfo.Movement();

            }

            equipmentSlots = new EquipmentSlot[0];

            if (character)
            {
                equipmentSlots = character.GetComponentsInChildren<EquipmentSlot>();
                character.OnChangeEquipment += OnChangeEquipment;

            }

            abilities.Clear();
            abilityMap = new();

            foreach (Transform child in transform)
            {
                var ability = child.GetComponent<AbilityInfo>();

                if (ability == null) continue;

                if (ability.isAttack || ability.abilityType2 == AbilityType2.AutoAttack)
                {
                    attackAbility = ability;
                }
                else
                {
                    abilities.Add(ability);
                    abilityMap.Add(ability);
                }

                ability.active = true;
            }



        }
        void Start()
        {
            GetDependencies();
            HandleUseWeaponAbility(null);
            HandleUseAuto();
        }

        void HandleUseWeaponAbility(Item weaponItem)
        {

            if (!settings.useWeaponAbility) return;

            if (attackAbility)
            {
                Debugger.InConsole(4329, $"Destroying {attackAbility}");

                attackAbility.RemoveSelf();
            }

            if (!Item.IsWeapon(weaponItem)) return;

            var weapon = (Weapon) weaponItem;

            if (weapon.ability == null || 
                weapon.ability.GetComponent<AbilityInfo>() == null) return;

            var ability = weapon.ability;

            
            attackAbility = Instantiate(ability, transform).GetComponent<AbilityInfo>();

            attackAbility.usesWeaponAttackDamage = true;

            attackAbility.UpdateAbility();

            attackAbility.active = true;

            OnNewAbility?.Invoke(attackAbility);

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

            if (attackAbility)
            {
                attackAbility.isAutoAttacking = false;
            }
        }

        public async Task CastingEnd()
        {
            while (IsCasting())
            {
                await Task.Yield();
            }
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
                if (currentlyCasting != null) {
                    currentlyCasting.CancelCast("State changed to interrupted state");


                }
                
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

        void HandleUseAuto()
        {
            if (entityInfo == null) return;
            events.OnUseAuto += HandleTargetCheck;


            void HandleTargetCheck(EntityInfo target, List<bool> checks)
            {
                if (attackAbility == null) return;
                if (!attackAbility.CanCastAt(target)) return;
            }
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

                if (!val && ability == currentlyCasting)
                {
                    ability.OnInterrupt?.Invoke(ability);
                }

                ability.active = val;
            }
        }

        public void DisableAbilities()
        {
            SetAbilities(false, false);
        }

        public void OnChangeEquipment(EquipmentSlot slot, Equipment before, Equipment after)
        {
            if (slot.equipmentSlotType != EquipmentSlotType.MainHand) return;

            HandleUseWeaponAbility(after);


            if (Item.IsWeapon(after))
            {
                currentWeapon = (Weapon)after;
            }
            else
            {
                currentWeapon = null;
            }

        }

        public bool HasWeaponType(WeaponType weaponType)
        {
            if (equipmentSlots.Length == 0) return true;

            foreach (var slot in equipmentSlots)
            {
                var equipment = slot.equipment;

                if (equipment == null) continue;

                if (!Item.IsWeapon(equipment)) continue;

                var weapon = (Weapon)equipment;

                if (weapon.weaponType != weaponType) continue;

                return true;
            }

            return false;

        }

        public bool IsCasting()
        {
            if (currentlyCasting == null) return false;
            if (currentlyCasting.isAttack) return false;

            return true;
        }

        public bool IsOpen()
        {
            if (currentlyCasting)
            {
                if (!currentlyCasting.isAttack) return false;
            }

            if (currentWantsToCast)
            {
                if (!currentWantsToCast.isAttack) return false;
            }

            return true;
        }
        public void OnChangeStats(EntityInfo entity)
        {
            foreach (var ability in GetComponentsInChildren<AbilityInfo>())
            {
                ability.UpdateAbility();
            }
        }

        //public void OnStartMove(Movement movement)
        //{
        //    if(currentlyCasting && currentlyCasting.cancelCastIfMoved)
        //    {
        //        currentlyCasting.CancelCast("Moved on non movable ability");
        //    }
        //}

        public AbilityInfo Ability(int num)
        {
            if (abilities.Count <= num) { return null; }

            return abilities[num];

        }
        public async Task Cast(AbilityInfo ability, bool usePlayerController = false)
        {
            debugger.Log(4352, $"{entityInfo.name} tries casting {ability}");
            if (!entityInfo.isAlive) { return; }
            if (!abilityMap.Contains(ability)) return;

            if (!entityInfo.isAlive) { return; }

            if (usePlayerController && entityInfo && !entityInfo.states.Contains(EntityState.MindControlled))
            {
                var playerController = entityInfo.PlayerController();

                if (playerController)
                {
                    playerController.HandlePlayerTargetting(ability);
                }
            }

            Debugger.InConsole(1115, $"Your code made it this far");

            if (target) { ability.target = target; }
            else { ability.target = null; }

            ability.location = location;
            await ability.Cast();
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
            if (attackAbility == null) return;
            if (target) {
                attackAbility.target = target;
                //OnTryAttackTarget?.Invoke(attackAbility, target);
            }
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

