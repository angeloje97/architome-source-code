using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;
using Architome;

[Serializable]
public class CombatReactions
{
    // Start is called before the first frame update
    public Animator animator;
    public EntityInfo entityInfo;


    public void ProcessData(EntityInfo entity, Anim anim)
    {
        entity.OnDamageTaken += OnDamageTaken;
        entity.OnLifeChange += OnLifeChange;
        entity.combatEvents.OnStatesChange += OnStatesChange;

        this.animator = anim.anim;
    }

    public void OnDamageTaken(CombatEventData eventData)
    {
        animator.SetTrigger("TakeDamage");
    }

    public void OnLifeChange(bool isAlive)
    {
        if(!isAlive)
        {
            animator.SetTrigger("Die");
        }

        animator.SetBool("IsAlive", isAlive);
    }

    public void OnStatesChange(List<EntityState> previous, List<EntityState> states)
    {
        if (states.Contains(EntityState.Stunned))
        {
            animator.SetInteger("EntityState", 1);
            return;
        }

        animator.SetInteger("EntityState", 0);
    }
}
