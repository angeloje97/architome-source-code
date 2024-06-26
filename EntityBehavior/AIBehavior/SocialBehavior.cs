using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;
using Architome.Events;

public enum eSocialEvent
{
    OnReceiveInteraction,
    OnReactToInteraction,
    OnTalkTo
}

public class SocialBehavior : EntityProp
{
    #region Common Data
    public Movement movement;
    public LineOfSight lineOfSight;
    public CharacterInfo character;

    public bool isListening;
    bool isInCombat;
    bool isMoving;

    #endregion


    protected override void Awake()
    {
        base.Awake();
    }

    public override void GetDependencies()
    {
        
        if(entityInfo)
        {
            movement = entityInfo.Movement();
            lineOfSight = entityInfo.LineOfSight();
            character = entityInfo.CharacterInfo();

            entityInfo.OnCombatChange += OnCombatChange;
            infoEvents.AddListenerSocial(eSocialEvent.OnReceiveInteraction, OnReceiveInteraction, this);
            infoEvents.AddListenerSocial(eSocialEvent.OnReactToInteraction, OnReactToInteraction, this);
            
        }

        if (movement)
        {
            movement.AddListener(eMovementEvent.OnEndMove, OnEndMove, this);
            movement.AddListener(eMovementEvent.OnStartMove, OnStartMove, this);
        }

        StartCoroutine(SocialRoutine());

    }

    public void OnCombatChange(bool val)
    {
        isInCombat = val;
        
    }

    void OnStartMove()
    {
        isMoving = true;
    }

    void OnEndMove()
    {
        isMoving = false;
    }
    public IEnumerator SocialRoutine()
    {
        while(true)
        {
            
            yield return new WaitForSeconds(UnityEngine.Random.Range(4f, 10f));
            if (isInCombat) { continue; }
            if (isMoving) { continue; }
            if(isListening) 
            {
                isListening = false;
                continue; 
            }

            var entities = lineOfSight.EntitiesLOSInRange(5f);

            Debugger.Social(4290, $"Entities around {entities.Count}");

            var allies = new List<EntityInfo>();

            foreach (var entity in entities)
            {
                if (entity.npcType != entityInfo.npcType) continue;
                if (!entity.GetComponentInChildren<SocialBehavior>()) continue;

                allies.Add(entity);
            }


            Debugger.Social(4291, $"allies around {allies.Count}");
            if (allies.Count == 0) { continue; }
            var target = ArchGeneric.RandomItem(allies);

            Debugger.Social(4292, $"{entityInfo.entityName} is talking to {target.entityName}");
            TalkToAlly(ArchGeneric.RandomItem(allies));

        }
    }

    public void TalkToAlly(EntityInfo ally)
    {
        if (ally.isInCombat) { return; }
        LookAt(ally);
        SociallyInteractWith(ally);
    }

    public void OnReceiveInteraction(SocialEventData eventData)
    {
        isListening = true;
        LookAt(eventData.source);
    }

    public void OnReactToInteraction(SocialEventData eventData)
    {
        isListening = true;
        LookAt(eventData.source);
    }

    public void LookAt(EntityInfo entity)
    {
        var allyPosition = entity.transform.position;
        character.LookAt(allyPosition);
    }
    
    public void SociallyInteractWith(EntityInfo entity)
    {
        var layerMask = LayerMasksData.active.entityLayerMask;
        //Collider[] listeners = Physics.OverlapSphere(entityInfo.transform.position, 10, layerMask);
        var listeners = Entity.EntitiesWithinLOS(transform.position, 10f);

        var newInteraction = new SocialEventData(entityInfo, entity);

        infoEvents.InvokeSocial(eSocialEvent.OnTalkTo, newInteraction);

        foreach(var info in listeners)
        {
            if (info == null) continue;
            info.ReactToSocial(newInteraction);
        }
    }
}

public struct SocialEvents
{
    public Action<SocialEventData> OnReceiveInteraction, OnReactToInteraction, OnTalkTo;
}