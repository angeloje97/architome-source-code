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
        entity.OnStateChange += OnStateChange;

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

    public void OnStateChange(EntityState previous, EntityState current)
    {
        if(current == EntityState.Active)
        {
            animator.SetInteger("EntityState", 0);
        }
        if(current == EntityState.Stunned)
        {
            animator.SetInteger("EntityState", 1);
        }
    }
}
