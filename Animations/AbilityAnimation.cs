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
    public CharacterInfo character;

    public void ProcessData(EntityInfo entity, Anim anim)
    {
        var abilityManager = entity.AbilityManager();
        this.entityInfo = entity;
        this.animator = anim.anim;
        character = entity.CharacterInfo();
        abilityManager = entityInfo.AbilityManager();
        abilityManager.OnCastStart += OnCastStart;
        abilityManager.OnCastRelease += OnCastRelease;
        abilityManager.OnCastReleasePercent += OnCastReleasePercent;
        abilityManager.OnChannelStart += OnChannelStart;
        abilityManager.OnChannelInterval += OnChannelInterval;
        abilityManager.OnChannelEnd += OnCastChannelEnd;
        abilityManager.OnCancelCast += OnCancelCast;
        abilityManager.OnCancelChannel += OnCancelChannel;
        abilityManager.OnCastChange += OnCastChange;

        abilityManager.OnAbilityStart += OnAbilityStart;
        abilityManager.OnAbilityEnd += OnAbilityEnd;

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
        if (ability.isAttack) return;
        //if (ability.HasChannel()) return;

        animator.SetTrigger("ReleaseAbility");
        //SetCast(false);
    }
    public void OnCastRelease(AbilityInfo ability)
    {
        if (ability.isAttack) { SetCast(false); }
    }
    public void OnChannelStart(AbilityInfo ability, AugmentChannel augment)
    {
        //SetAnimTrigger(currentCatalyst, 2, false);
        //if (2 >= currentCatalyst.animationSequence.Count) return;



        //animator.SetInteger("AbilityIndex", currentCatalyst.animationSequence[2]);
        //animator.SetTrigger("ActivateAbility");

    }
    public void OnChannelInterval(AbilityInfo ability, AugmentChannel augment)
    {
        //animator.SetTrigger("Repeat");
        animator.SetTrigger("ReleaseAbility");
    }
    public void OnCastChannelEnd(AbilityInfo ability, AugmentChannel augment)
    {
        //SetAnimTrigger(currentCatalyst, 3, false);
    }

    public void OnAbilityStart(AbilityInfo ability)
    {
        SetCast(true);

    }

    public void OnAbilityEnd(AbilityInfo ability)
    {
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

        //animator.SetTrigger("ActivateAbility");
        animator.SetInteger("AbilityX", xStyle);
        animator.SetInteger("AbilityY", yStyle);
        animator.SetInteger("AbilityIndex", catalyst.animationSequence[abilityIndex]);
    }

    public void Attack()
    {
        animator.SetFloat("AttackSpeed", entityInfo.stats.attackSpeed);

        if (UsesFixedAttackAnimation()) return;

        var weapon = entityInfo.CharacterInfo().WeaponItem(EquipmentSlotType.MainHand);
        if(weapon == null)
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
