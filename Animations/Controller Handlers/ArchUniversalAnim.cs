using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class ArchUniversalAnim : EntityProp
    {
        // Start is called before the first frame update
        public Animator animator;
        public CharacterInfo character;
        public AbilityManager abilityManager;
        public Movement movement;

        new void GetDependencies()
        {
            base.GetDependencies();

            character = GetComponentInParent<CharacterInfo>();
            animator = GetComponent<Animator>();

            if (entityInfo)
            {
                movement = entityInfo.Movement();
                abilityManager = entityInfo.AbilityManager();

                if (abilityManager)
                {
                    abilityManager.OnAbilityStart += OnAbilityStart;
                    abilityManager.OnAbilityEnd += OnAbilityEnd;
                    abilityManager.OnCastStart += OnCastStart;
                    abilityManager.OnCastReleasePercent += OnCastReleasePercent;
                    abilityManager.OnChannelInterval += OnChannelInterval;
                }

                entityInfo.OnLifeChange += OnLifeChange;
                entityInfo.OnDamageTaken += OnDamageTaken;
                entityInfo.OnCombatChange += OnCombatChange;
                
            }
        }

        

        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {
            if (animator == null) return;
            
            UpdateMetrics();
        }

        void UpdateMetrics()
        {
            if (movement)
            {
                animator.SetFloat("Speed", movement.speed);
            }

            
        }

        private void OnDamageTaken(CombatEventData eventData)
        {
            animator.SetTrigger("TakeDamage");
        }

        void OnAbilityStart(AbilityInfo ability)
        {
            animator.SetBool("IsCasting", true);
            animator.SetTrigger("CancelAttack");
            animator.SetTrigger("CancelAbility");
        }

        void OnAbilityEnd(AbilityInfo ability)
        {

            animator.SetBool("IsCasting", false);
            animator.SetInteger("AbilityX", 0);
            animator.SetInteger("AttackX", 0);
        }

        void OnLifeChange(bool isAlive)
        {
            animator.SetBool("IsAlive", isAlive);
        }

        void OnCombatChange(bool isInCombat)
        {
            animator.SetBool("IsInCombat", isInCombat);
            animator.SetTrigger("ResetMovement");
        }

        void OnCastStart(AbilityInfo ability)
        {
            if (ability.catalyst == null) return;

            if (ability.isAttack)
            {
                var weapon = character.WeaponItem(Enums.EquipmentSlotType.MainHand);

                if (weapon)
                {
                    animator.SetFloat("AttackSpeed", entityInfo.stats.attackSpeed);
                    animator.SetInteger("AttackX", (int)weapon.weaponAttackStyle.x);
                }
                else
                {
                    animator.SetFloat("AttackSpeed", entityInfo.stats.attackSpeed);
                    animator.SetInteger("AttackX", 1);
                    
                    
                }
            }
            else
            {
                animator.SetInteger("AbilityX", (int)ability.catalystInfo.catalystStyle.x);

            }
        }

        void OnCastReleasePercent(AbilityInfo ability)
        {
            animator.SetTrigger("AbilityRelease");
        }

        void OnChannelInterval(AbilityInfo ability)
        {

            animator.SetTrigger("AbilityRelease");
        }
    }

}