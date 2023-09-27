using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;
using Architome;
using System.Threading.Tasks;

[Serializable]
public class AbilityAnimation
{
    // Start is called before the first frame update
    public EntityInfo entityInfo;
    public AbilityManager abilityManager;
    //public Animator animator;
    //public Anim anim;
    public CatalystInfo currentCatalyst;
    public CharacterInfo character;

    public Weapon weapon;

    TaskQueueHandler taskHandler;

    float currentCastAnimation;

    enum ControlParam
    {
        CancelAttack,
        CancelAbility,
        ReleaseAbility,
        AbilityX,
        AbilityY,
        AbilityZ,
        AbilityIndex,
        AttackX,
        AttackY,
        AttackZ,
        AttackSpeed,
        IsCasting
    }

    AnimationHandler<ControlParam> animator;


    AbilityManager.Events abilityEvents
    {
        get
        {
            return entityInfo.abilityEvents;
        }
    }

    public void ProcessData(EntityInfo entity, Animator animator)
    {
        var abilityManager = entity.AbilityManager();
        entityInfo = entity;
        this.animator = new(animator) ;
        character = entity.CharacterInfo();
        abilityManager = entityInfo.AbilityManager();

        taskHandler = new(TaskType.Sequential);


        if (entity)
        {
            abilityEvents.OnCastStart += OnCastStart;
            abilityEvents.OnCastRelease += OnCastRelease;
            abilityEvents.OnCastReleasePercent += OnCastReleasePercent;
        }
        abilityManager.OnChannelStart += OnChannelStart;
        abilityManager.OnChannelInterval += OnChannelInterval;
        abilityManager.OnChannelEnd += OnCastChannelEnd;
        abilityManager.OnCancelCast += OnCancelCast;
        abilityManager.OnCancelChannel += OnCancelChannel;
        abilityManager.OnCastChange += OnCastChange;

        abilityManager.OnAbilityStart += OnAbilityStart;

        if (character)
        {
            character.OnChangeEquipment += OnChangeEquipment;
            var mainSlot = character.EquipmentSlot(EquipmentSlotType.MainHand);
            var equipment = mainSlot.equipment;

            if (equipment && Item.IsWeapon(equipment))
            {
                weapon = (Weapon)equipment;
            }
        }
    }

    public void OnChangeEquipment(EquipmentSlot slot, Equipment before, Equipment after)
    {
        if (slot.equipmentSlotType != EquipmentSlotType.MainHand) return;
        if (after == null)
        {
            weapon = null;
            return;
        }

        if (!Item.IsWeapon(after))
        {
            weapon = null;
            return;
        }

        weapon = (Weapon)after;

    }

    public void OnCastChange(AbilityInfo info, bool isCasting)
    {
    }
    public void OnCastStart(AbilityInfo ability)
    {
        var catalystInfo = ability.catalystInfo;
        if (catalystInfo == null) { return; }
        currentCatalyst = catalystInfo;

        Debugger.Combat(6418, $"{ability} animation");

        animator.SetTrigger(ControlParam.CancelAttack);
        animator.SetTrigger(ControlParam.CancelAbility);

        if (ability.isAttack)
        {
            Attack();
        }
        else
        {
            SetAbilityAnimation(catalystInfo);
        }

        //SetCast(true);
    }

    public void OnCastReleasePercent(AbilityInfo ability)
    {
        if (ability.isAttack) return;

        animator.SetTrigger(ControlParam.ReleaseAbility);
    }
    public void OnCastRelease(AbilityInfo ability)
    {
        if (ability.isAttack) { SetCast(false); }
    }
    public void OnChannelStart(AbilityInfo ability, AugmentChannel augment)
    {

    }
    public void OnChannelInterval(AbilityInfo ability, AugmentChannel augment)
    {
        animator.SetTrigger(ControlParam.ReleaseAbility);
    }
    public void OnCastChannelEnd(AbilityInfo ability, AugmentChannel augment)
    {
    }

    public async void OnAbilityStart(AbilityInfo ability)
    {
        var timeStamp = Time.time;
        currentCastAnimation = timeStamp;
        SetCast(true);

        await ability.EndActivation();
        ZeroOut();


        ArchAction.Delay(() =>
        {
            if (timeStamp != currentCastAnimation) return;
            SetCast(false);
            animator.ResetTrigger(ControlParam.ReleaseAbility);
        }, .25f);
    }

    void ZeroOut()
    {
        animator.SetInteger(ControlParam.AbilityX, 0);
        animator.SetInteger(ControlParam.AbilityY, 0);
        animator.SetInteger(ControlParam.AbilityZ, 0);
        animator.SetInteger(ControlParam.AbilityIndex, 0);

        animator.SetInteger(ControlParam.AttackX, 0);
        animator.SetInteger(ControlParam.AttackY, 0);
        animator.SetInteger(ControlParam.AttackZ, 0);


    }
    public void OnCancelCast(AbilityInfo ability)
    {
        animator.SetTrigger(ControlParam.CancelAbility);
        animator.SetTrigger(ControlParam.CancelAttack);
    }
    public void OnCancelChannel(AbilityInfo ability, AugmentChannel augment)
    {
        animator.SetTrigger(ControlParam.CancelAttack);
    }


    public void SetAbilityAnimation(CatalystInfo catalyst)
    {
        //if(abilityIndex >=  catalyst.animationSequence.Count) { return; }

        var xStyle = (int)catalyst.catalystStyle.x;
        var yStyle = (int)catalyst.catalystStyle.y;
        var zStyle = (int)catalyst.catalystStyle.z;
        animator.SetInteger(ControlParam.AbilityX, xStyle);
        animator.SetInteger(ControlParam.AbilityY, yStyle);
        animator.SetInteger(ControlParam.AbilityZ, zStyle);
        animator.SetInteger(ControlParam.AbilityIndex, zStyle);
    }

    public void Attack()
    {
        animator.SetFloat(ControlParam.AttackSpeed, entityInfo.stats.attackSpeed);

        if (UsesFixedAttackAnimation()) return;

        //var weapon = character.WeaponItem(EquipmentSlotType.MainHand);

        if (weapon == null)
        {
            animator.SetInteger(ControlParam.AttackX, 1);
            animator.SetInteger(ControlParam.AttackY, 0);
            animator.SetInteger(ControlParam.AttackZ, 0);
            return;
        }

        animator.SetInteger(ControlParam.AttackX, (int)weapon.weaponAttackStyle.x);
        animator.SetInteger(ControlParam.AttackY, (int)weapon.weaponAttackStyle.y);
        animator.SetInteger(ControlParam.AttackZ, (int)weapon.weaponAttackStyle.z);

        bool UsesFixedAttackAnimation()
        {
            if (character == null) return false;
            if (!character.fixedAnimation.enabled) return false;

            var fixedAttack = character.fixedAnimation.attackStyle;

            animator.SetInteger(ControlParam.AttackX, (int)fixedAttack.x);
            animator.SetInteger(ControlParam.AttackY, (int)fixedAttack.y);
            animator.SetInteger(ControlParam.AttackZ, (int)fixedAttack.z);

            return true;
        }
    }

    public void SetCast(bool val)
    {
        animator.SetBool(ControlParam.IsCasting, val);
    }
}