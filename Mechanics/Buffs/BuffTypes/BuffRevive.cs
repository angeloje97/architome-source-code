using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;
public class BuffRevive : BuffType
{
    // Start is called before the first frame update
    
    public EntityInfo hostInfo;

    public float healthRevivePercent;


    new void GetDependencies()
    {
        base.GetDependencies();

        if (buffInfo)
        {
            hostInfo = buffInfo.hostInfo;
        }
        
        if(buffInfo.sourceInfo.GetType() == typeof(EntityInfo))
        {
            Revive(buffInfo.sourceInfo.transform.position);
        }
        else if(buffInfo.sourceInfo.GetType() == typeof(SpawnerInfo))
        {
            var spawner = (SpawnerInfo) buffInfo.sourceInfo;

            Revive(spawner.RandomPosition());
        }
        else
        {
            Revive(hostInfo.transform.position);
        }
    }

    public override string Description()
    {
        var result = "";

        result += $"Revives a friendly target for {ArchString.FloatToSimple(healthRevivePercent*100)}% health.";
       
        return result;
    }

    public override string GeneralDescription()
    {
        return $"Revive a dead target and restore their health to {ArchString.FloatToSimple(healthRevivePercent*100)}% health";
    }

    public void Revive(Vector3 position)
    {
        hostInfo.transform.position = position;

        var combatData = new HealthEvent(buffInfo, value);
        //{
        //    buff = buffInfo,
        //    percentValue = healthRevivePercent,
        //};

        combatData.SetPercentValue(healthRevivePercent);


        hostInfo.Revive(combatData);
        hostInfo.currentRoom = hostInfo.CurrentRoom();
        hostInfo.Movement().StopMoving(true);
        
    }
    void Start()
    {
        GetDependencies();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
