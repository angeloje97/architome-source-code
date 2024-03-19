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

    CombatEvents combatEvents => entityInfo.combatEvents;


    public void ProcessData(EntityInfo entity, Anim anim)
    {
        this.entityInfo = entity;
        entity.OnLifeChange += OnLifeChange;
        combatEvents.AddListenerHealth(eHealthEvent.OnDamageTaken, OnDamageTaken, anim);
        combatEvents.AddListenerStateEvent(eStateEvent.OnStatesChange, OnStatesChange, anim);
        this.animator = anim.anim;
    }

    public void OnDamageTaken(HealthEvent eventData)
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

    public void OnStatesChange(StateChangeEvent stateEventData)
    {
        if (stateEventData.afterEventStates.Contains(EntityState.Stunned))
        {
            animator.SetInteger("EntityState", 1);
            return;
        }

        animator.SetInteger("EntityState", 0);
    }
}
