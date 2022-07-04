using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;
public class SocialBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    public EntityInfo entityInfo;
    public Movement movement;
    public LineOfSight lineOfSight;
    public CharacterInfo character;

    public bool isListening;
    
    public void GetDependencies()
    {
        if(GetComponentInParent<EntityInfo>())
        {
            entityInfo = GetComponentInParent<EntityInfo>();
            movement = entityInfo.Movement();
            lineOfSight = entityInfo.LineOfSight();
            character = entityInfo.CharacterInfo();

            entityInfo.OnCombatChange += OnCombatChange;
            entityInfo.OnReceiveInteraction += OnReceiveInteraction;
            entityInfo.OnReactToInteraction += OnReactToInteraction;
            movement.OnArrival += OnArrival;
        }
    }
    void Start()
    {
        GetDependencies();
        StartCoroutine(SocialRoutine());
    }

    // Update is called once per frame
    void Update()
    {
    }


    public void OnCombatChange(bool val)
    {
        
        
    }
    public void OnArrival(Movement movement, Transform location)
    {
        
    }

    public IEnumerator SocialRoutine()
    {
        while(true)
        {
            
            yield return new WaitForSeconds(Random.Range(4f, 10f));
            if (entityInfo.isInCombat) { continue; }
            if (movement.isMoving) { continue; }
            if(isListening) 
            {
                isListening = false;
                continue; 
            }
            var allies = lineOfSight.FilterWhiteList(entityInfo.npcType, lineOfSight.EntitiesLOSInRange(5f));
            Debugger.InConsole(4291, $"allies around {allies.Count}");
            if (allies.Count == 0) { continue; }

            TalkToAlly(allies[Random.Range(0, allies.Count)]);

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
        var allyPosition = entity.CharacterInfo().transform.position;
        allyPosition.y = entityInfo.CharacterInfo().transform.position.y;
        character.transform.LookAt(allyPosition);
    }
    
    public void SociallyInteractWith(EntityInfo entity)
    {
        Collider[] listeners = Physics.OverlapSphere(entityInfo.transform.position, 10, LayerMasksData.active.entityLayerMask);
        var structureLayer = LayerMasksData.active.structureLayerMask;

        foreach(Collider listener in listeners)
        {
            var info = listener.GetComponent<EntityInfo>();
            if (info == null) continue;

            var distance = Vector3.Distance(info.transform.position, transform.position);
            var direction = V3Helper.Direction(info.transform.position, transform.position);
            if (Physics.Raycast(entityInfo.transform.position, direction, distance, structureLayer)) continue;

            
            var newInteraction = new SocialEventData(entityInfo, entity);
            info.ReactToSocial(newInteraction);

        }
    }
}
