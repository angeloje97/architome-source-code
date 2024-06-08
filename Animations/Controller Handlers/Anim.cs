using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
using System.Linq;
using System.Threading.Tasks;

public class Anim : EntityProp
{
    // Start is called before the first frame update
    public CharacterInfo charInfo;

    public AbilityManager abilityManager;
    public Animator anim;
    public Movement movement;

    public Weapon mainHand;
    public Weapon offHand;

    public AbilityAnimation abilityAnimation;
    public AttackAnimation attackAnimation;
    public CombatReactions combatReactions;
    public TaskAnimation taskAnimations;


    public override void GetDependencies()
    {
        

        anim = GetComponent<Animator>();

        //entityInfo = GetComponentInParent<EntityInfo>();
        charInfo = GetComponentInParent<CharacterInfo>();


        if (entityInfo)
        {

            abilityAnimation = new();
            combatReactions = new();
            abilityAnimation.ProcessData(entityInfo, anim);
            attackAnimation = new(entityInfo, anim);
            combatReactions.ProcessData(entityInfo, this);

            if (entityInfo.Movement())
            {
                movement = entityInfo.Movement();
            }

            if (entityInfo.AbilityManager())
            {
                abilityManager = entityInfo.AbilityManager();
            }
        }

        if (charInfo)
        {
            charInfo.OnChangeEquipment += OnChangeEquipment;
            charInfo.OnChangeSheath += OnChangeSheath;

            if (charInfo.fixedAnimation.enabled && entityInfo)
            {
                entityInfo.OnCombatChange += HandleCombatChangedFixedAnimation;
            }
        }

        taskAnimations = new TaskAnimation(this);


        DetermineAnimation();

    }


    private void OnEnable()
    {
        Invoke("GetDependencies", .125f);
    }

    // Update is called once per frame
    public override void EUpdate()
    {
        HandleCharacterSpeed();
        taskAnimations.Update();

    }

    void HandleCombatChangedFixedAnimation(bool isInCombat)
    {
        anim.SetTrigger("ResetMovement");

        if (isInCombat)
        {
            anim.SetInteger("MovementStyle", charInfo.fixedAnimation.movementStyle);
        }
        else
        {
            anim.SetInteger("MovementStyle", 0);
        }
    }

    void HandleCharacterSpeed()
    {
        if (movement && anim)
        {
            anim.SetFloat("Speed", movement.speed);
        }
    }

    void OnChangeEquipment(EquipmentSlot slot, Equipment previousEquipment, Equipment newEquipment)
    {
        anim.SetTrigger("ResetMovement");
        if (slot.equipmentSlotType != EquipmentSlotType.MainHand && slot.equipmentSlotType != EquipmentSlotType.OffHand) { return; }

        if (newEquipment == null)
        {
            switch (slot.equipmentSlotType)
            {
                case EquipmentSlotType.MainHand:
                    mainHand = null;
                    break;
                case EquipmentSlotType.OffHand:
                    offHand = null;
                    break;
            }

            DetermineAnimation();

            return;
        }

        if (!Item.IsWeapon(newEquipment)) { return; }

        var newWeapon = (Weapon)newEquipment;

        switch (slot.equipmentSlotType)
        {
            case EquipmentSlotType.MainHand:
                mainHand = newWeapon;
                break;
            case EquipmentSlotType.OffHand:
                offHand = newWeapon;
                break;
        }

        DetermineAnimation();
    }
    void DetermineAnimation()
    {
        if (!anim) { return; }
        if (mainHand == null && offHand == null)
        {
            SetCombatAnim(false);
            return;
        }


        DetermineMainHand();
        DetermineOffHand();

        void DetermineMainHand()
        {
            if (UsesFixedAnimation()) return;

            if (mainHand == null)
            {
                anim.SetInteger("MovementStyle", 0);
            }
            else
            {
                anim.SetInteger("MovementStyle", mainHand.weaponMovementStyle);
            }

            bool UsesFixedAnimation()
            {
                if (charInfo == null) return false;
                if (!charInfo.fixedAnimation.enabled) return false;

                anim.SetInteger("MovementStyle", charInfo.fixedAnimation.movementStyle);
                anim.SetInteger("AttackX", (int) charInfo.fixedAnimation.attackStyle.x);
                anim.SetInteger("AttackY", (int) charInfo.fixedAnimation.attackStyle.y);
                anim.SetInteger("AttackZ", (int) charInfo.fixedAnimation.attackStyle.z);

                return true;
            }
        }

        void DetermineOffHand()
        {
            if (offHand == null) { return; }
        }
    }
    void SetCombatAnim(bool val)
    {
        if (val)
        {
            DetermineAnimation();
        }
        else
        {
            anim.SetInteger("AttackX", 0);
            anim.SetInteger("AttackY", 0);
            anim.SetInteger("MovementStyle", 0);
        }

        //HandleFixedAnimation();
    }

    void HandleFixedAnimation()
    {
        if (charInfo == null) return;
        if (!charInfo.fixedAnimation.enabled) return;

        anim.SetInteger("MovementStyle", charInfo.fixedAnimation.movementStyle);
        anim.SetInteger("AttackX", (int)charInfo.fixedAnimation.attackStyle.x);
        anim.SetInteger("AttackY", (int)charInfo.fixedAnimation.attackStyle.y);
        anim.SetInteger("AttackZ", (int)charInfo.fixedAnimation.attackStyle.z);
    }

    void OnChangeSheath(bool val)
    {
        anim.SetTrigger("ResetMovement");
        SetCombatAnim(!val);

    }
}

public class TaskAnimation
{
    public Animator animator;
    public EntityInfo entity;

    int currentAnimation = -1;
    int currentAnimationCheck = 0;
    public TaskAnimation(Anim anim)
    {
        animator = anim.anim;
        entity = anim.entityInfo;

        HandleEvents();


        //animator.SetInteger("TaskAnimation", -1);
        currentAnimation = -1;


    }

    public void Update()
    {
        if(currentAnimation != currentAnimationCheck)
        {
            OnAnimationChange(currentAnimationCheck, currentAnimation);
            currentAnimationCheck = currentAnimation;

        }
    }

    void OnAnimationChange(int previous, int current)
    {
        animator.SetInteger("TaskAnimation", current);
    }

    void HandleEvents()
    {
        if (!entity) return;

        var movement = entity.Movement();

        if (movement)
        {
            movement.AddListener(eMovementEvent.OnStartMove, () => {
                currentAnimation = -1;
            }, animator);
        }

        entity.taskEvents.OnStartTask += delegate (TaskEventData eventData)
        {
            currentAnimation = eventData.task.effects.taskAnimationID;
        };

        entity.taskEvents.OnLingeringStart += delegate (TaskEventData eventData)
        {
            currentAnimation = -1;
            ArchAction.Delay(() => {
                currentAnimation = eventData.task.effects.lingeringAnimationID;
            }, .125f);
        };
        entity.taskEvents.OnEndTask += delegate (TaskEventData eventData)
        {
            currentAnimation = -1;
        };

        entity.taskEvents.OnLingeringEnd += delegate (TaskEventData eventData)
        {
            currentAnimation = -1;
        };

    }
}