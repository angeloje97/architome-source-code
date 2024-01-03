using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Architome
{
    public class ArchUniversalAnim : EntityProp
    {
        // Start is called before the first frame update
        public Animator animator;
        public CharacterInfo character;
        public AbilityManager abilityManager;
        public Movement movement;

        [Header("Settings")]
        [SerializeField] float walkSpeed = 1f;
        [SerializeField] float runSpeed = 1f;


        AbilityManager.Events abilityEvents
        {
            get
            {
                return entityInfo.abilityEvents;
            }
        }
        

        //protected override async void Start()
        //{
        //    await GetDependencies(DefaultExtension);
        //    ApplySettings();
        //}

        public override async Task GetDependencies()
        {
            character = GetComponentInParent<CharacterInfo>();
            animator = GetComponent<Animator>();

            if (entityInfo)
            {
                movement = entityInfo.Movement();
                abilityManager = entityInfo.AbilityManager();

                abilityEvents.OnCastStart += OnCastStart;
                abilityEvents.OnCastReleasePercent += OnCastReleasePercent;

                if (abilityManager)
                {
                    abilityManager.OnAbilityStart += OnAbilityStart;
                    abilityManager.OnAbilityEnd += OnAbilityEnd;
                    abilityManager.OnChannelInterval += OnChannelInterval;
                }

                entityInfo.OnLifeChange += OnLifeChange;
                entityInfo.OnDamageTaken += OnDamageTaken;
                entityInfo.OnCombatChange += OnCombatChange;

                HandleTaskAnimation();
            }


            ApplySettings();
        }

        void HandleTaskAnimation()
        {

            animator.SetInteger("TaskID", -1);
            entityInfo.taskEvents.OnStartTask += delegate (TaskEventData eventData) {
                animator.SetInteger("TaskID", eventData.task.effects.taskAnimationID);
            };

            entityInfo.taskEvents.OnLingeringStart += delegate (TaskEventData eventData)
            {
                animator.SetInteger("TaskID", -1);
                ArchAction.Delay(() => animator.SetInteger("TaskID", eventData.task.effects.lingeringAnimationID), .25f);
            };

            entityInfo.taskEvents.OnEndTask += delegate (TaskEventData eventData)  {
                animator.SetInteger("TaskID", -1);
            };

            entityInfo.taskEvents.OnLingeringEnd += delegate (TaskEventData eventData)
            {
                animator.SetInteger("TaskID", -1);
            };
        }
        // Update is called once per frame
        void Update()
        {
            if (animator == null) return;
            
            UpdateMetrics();
        }

        void ApplySettings()
        {

            animator.SetFloat("WalkSpeed", walkSpeed);
            animator.SetFloat("RunSpeed", runSpeed);
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

                animator.SetFloat("AttackSpeed", entityInfo.stats.attackSpeed);

                if (character.fixedAnimation.enabled)
                {
                    animator.SetInteger("AttackX", (int) character.fixedAnimation.attackStyle.x);
                }
                else if (weapon)
                {
                    animator.SetInteger("AttackX", (int)weapon.weaponAttackStyle.x);
                }
                else
                {
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
            animator.SetTrigger("ReleaseAbility");
        }

        void OnChannelInterval(AbilityInfo ability, AugmentChannel augment)
        {

            animator.SetTrigger("ReleaseAbility");
        }
    }

}