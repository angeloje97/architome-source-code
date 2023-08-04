using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;
using Architome;

[Serializable]
public class AbilityAnimation
{
    // Start is called before the first frame update
    public EntityInfo entityInfo;
    public AbilityManager abilityManager;
    public Animator animator;
    //public Anim anim;
    public CatalystInfo currentCatalyst;
    public CharacterInfo character;

    public Weapon weapon;

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
        this.animator = animator;
        character = entity.CharacterInfo();
        abilityManager = entityInfo.AbilityManager();



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

        animator.SetTrigger("CancelAttack");
        animator.SetTrigger("CancelAbility");

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

        animator.SetTrigger("ReleaseAbility");
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
        animator.SetTrigger("ReleaseAbility");
    }
    public void OnCastChannelEnd(AbilityInfo ability, AugmentChannel augment)
    {
    }

    public async void OnAbilityStart(AbilityInfo ability)
    {
        SetCast(true);

        await ability.EndActivation();
        ZeroOut();
        SetCast(false);

        ArchAction.Delay(() => {
            animator.ResetTrigger("ReleaseAbility");
        }, .25f);
    }

    void ZeroOut()
    {
        animator.SetInteger("AbilityX", 0);
        animator.SetInteger("AbilityY", 0);
        animator.SetInteger("AbilityZ", 0);
        animator.SetInteger("AbilityIndex", 0);

        animator.SetInteger("AttackX", 0);
        animator.SetInteger("AttackY", 0);
        animator.SetInteger("AttackZ", 0);


    }
    public void OnCancelCast(AbilityInfo ability)
    {
        animator.SetTrigger("CancelAbility");
        animator.SetTrigger("CancelAttack");
    }
    public void OnCancelChannel(AbilityInfo ability, AugmentChannel augment)
    {
        animator.SetTrigger("CancelAbility");
    }


    public void SetAbilityAnimation(CatalystInfo catalyst)
    {
        //if(abilityIndex >=  catalyst.animationSequence.Count) { return; }

        var xStyle = (int)catalyst.catalystStyle.x;
        var yStyle = (int)catalyst.catalystStyle.y;
        var zStyle = (int)catalyst.catalystStyle.z;
        animator.SetInteger("AbilityX", xStyle);
        animator.SetInteger("AbilityY", yStyle);
        animator.SetInteger("AbilityZ", zStyle);
        animator.SetInteger("AbilityIndex", zStyle);
    }

    public void Attack()
    {
        animator.SetFloat("AttackSpeed", entityInfo.stats.attackSpeed);

        if (UsesFixedAttackAnimation()) return;

        //var weapon = character.WeaponItem(EquipmentSlotType.MainHand);

        if (weapon == null)
        {
            animator.SetInteger("AttackX", 1);
            animator.SetInteger("AttackY", 0);
            animator.SetInteger("AttackZ", 0);
            return;
        }

        animator.SetInteger("AttackX", (int)weapon.weaponAttackStyle.x);
        animator.SetInteger("AttackY", (int)weapon.weaponAttackStyle.y);
        animator.SetInteger("AttackZ", (int)weapon.weaponAttackStyle.z);

        bool UsesFixedAttackAnimation()
        {
            if (character == null) return false;
            if (!character.fixedAnimation.enabled) return false;

            var fixedAttack = character.fixedAnimation.attackStyle;

            animator.SetInteger("AttackX", (int)fixedAttack.x);
            animator.SetInteger("AttackY", (int)fixedAttack.y);
            animator.SetInteger("AttackZ", (int)fixedAttack.z);

            return true;
        }
    }

    public void SetCast(bool val)
    {
        animator.SetBool("IsCasting", val);
    }
}