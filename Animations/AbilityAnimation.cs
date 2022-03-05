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
    public Anim anim;
    public CatalystInfo currentCatalyst;

    public void ProcessData(EntityInfo entity, Anim anim)
    {
        var abilityManager = entity.AbilityManager();
        this.entityInfo = entity;
        this.animator = anim.anim;
        abilityManager = entityInfo.AbilityManager();
        abilityManager.OnCastStart += OnCastStart;
        abilityManager.OnCastRelease += OnCastRelease;
        abilityManager.OnCastReleasePercent += OnCastReleasePercent;
        abilityManager.OnCastChannelStart += OnCastChannelStart;
        abilityManager.OnCastChannelInterval += OnCastChannelInterval;
        abilityManager.OnCastChannelEnd += OnCastChannelEnd;
        abilityManager.OnCancelCast += OnCancelCast;
        abilityManager.OnCancelChannel += OnCancelChannel;
        abilityManager.OnCastChange += OnCastChange;

        this.anim = anim;
        this.animator = anim.anim;
    }

    public void OnCastChange(AbilityInfo info, bool isCasting)
    {
    }
    public void OnCastStart(AbilityInfo ability)
    {
        var catalystInfo = ability.catalyst ? ability.catalyst.GetComponent<CatalystInfo>() : null;
        if(catalystInfo == null) { return; }
        currentCatalyst = catalystInfo;

        animator.SetTrigger("CancelAttack");
        animator.SetTrigger("CancelAbility");

        if (ability.isAttack)
        {
            Attack();
        }
        else
        {
            SetAbilityAnimation(catalystInfo, 0, true);
        }

        SetCast(true);
    }

    public void OnCastReleasePercent(AbilityInfo ability)
    {
        animator.SetTrigger("ReleaseAbility");
        SetCast(false);
    }
    public void OnCastRelease(AbilityInfo ability)
    {
        if (ability.isAttack) { SetCast(false); }
    }
    public void OnCastChannelStart(AbilityInfo ability)
    {
        //SetAnimTrigger(currentCatalyst, 2, false);

        animator.SetInteger("AbilityIndex", currentCatalyst.animationSequence[2]);
        animator.SetTrigger("ActivateAbility");

    }
    public void OnCastChannelInterval(AbilityInfo ability)
    {
        animator.SetTrigger("Repeat");
    }
    public void OnCastChannelEnd(AbilityInfo ability)
    {
        //SetAnimTrigger(currentCatalyst, 3, false);
        SetCast(false);
    }
    public void OnCancelCast(AbilityInfo ability)
    {
        animator.SetTrigger("CancelAbility");
        animator.SetTrigger("CancelAttack");
        SetCast(false);
    }
    public void OnCancelChannel(AbilityInfo ability)
    {
        animator.SetTrigger("CancelAbility");
        SetCast(false);
    }

    public void SetAnimTrigger(CatalystInfo catalyst, int index, bool val)
    {
        
        if(catalyst == null || catalyst.animationSequence == null) { return; }
        if(index >= catalyst.animationSequence.Count) { return; }


    }

    public void SetAbilityAnimation(CatalystInfo catalyst, int abilityIndex, bool val)
    {
        if(abilityIndex >=  catalyst.animationSequence.Count) { return; }

        var xStyle = (int)catalyst.catalystStyle.x;
        var yStyle = (int)catalyst.catalystStyle.y;

        animator.SetTrigger("ActivateAbility");
        animator.SetInteger("AbilityX", xStyle);
        animator.SetInteger("AbilityY", yStyle);
        animator.SetInteger("AbilityIndex", catalyst.animationSequence[abilityIndex]);
    }

    public void Attack()
    {
        var weapon = entityInfo.CharacterInfo().WeaponItem(EquipmentSlotType.MainHand);
        animator.SetFloat("AttackSpeed", entityInfo.stats.attackSpeed);
        if(weapon == null)
        {
            animator.SetInteger("AttackStyleX", 0);
            animator.SetInteger("AttackStyleY", 0);
            animator.SetTrigger("Attack");
            return;
        }

        animator.SetInteger("AttackStyleX", (int)weapon.weaponAttackStyle.x);
        animator.SetInteger("AttackStyleY", (int)weapon.weaponAttackStyle.y);
        animator.SetTrigger("Attack");

    }

    public void SetCast(bool val)
    {
        animator.SetBool("IsCasting", val);
    }
}
