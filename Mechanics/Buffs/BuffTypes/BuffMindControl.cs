using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;
using Architome.Enums;
public class BuffMindControl : BuffStateChanger
{
    // Start is called before the first frame update

    public AIBehaviorType originalBehaviorType;
    public NPCType originalNPCType;
    

    void GetDependencies()
    {
        if(!GetComponent<BuffInfo>()) return;

        buffInfo = GetComponent<BuffInfo>();
        buffInfo.OnBuffCleanse += OnBuffCleanse;
        buffInfo.OnBuffCompletion += OnBuffCompletion;


        
        ApplyBuff();
    }
    new void ApplyBuff()
    {
        var host = buffInfo.hostInfo;
        var source = buffInfo.sourceInfo;

        bool changedState = base.ApplyBuff();
        applied = changedState;

        if(changedState)
        {
            originalBehaviorType = host.AIBehavior().behaviorType;
            originalNPCType = host.npcType;
            host.ChangeBehavior(source.npcType, Entity.IsPlayer(source.gameObject)? AIBehaviorType.HalfPlayerControl : AIBehaviorType.NoControl);
            host.Movement().StopMoving();
        }
    }

    public void OnRecast(AbilityInfo ability)
    {

    }

    void Start()
    {
        GetDependencies();
    }

    // Update is called once per frame
    public new void OnBuffCleanse(BuffInfo buffInfo)
    {
        if (!applied) return;
        HandleRemoveState(buffInfo);
        buffInfo.hostInfo.ChangeBehavior(originalNPCType, originalBehaviorType);
    }

    public new void OnBuffCompletion(BuffInfo buffInfo)
    {
        if (!applied) return;
        HandleRemoveState(buffInfo);
        buffInfo.hostInfo.ChangeBehavior(originalNPCType, originalBehaviorType);
    }
}
