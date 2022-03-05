using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;
public class BuffRevive : MonoBehaviour
{
    // Start is called before the first frame update
    
    public BuffInfo buffInfo;
    public EntityInfo hostInfo;

    public float healthRevivePercent;


    void GetDependencies()
    {
        if(GetComponent<BuffInfo>())
        {
            buffInfo = GetComponent<BuffInfo>();
            hostInfo = buffInfo.hostInfo;
        }
        else
        {
            Destroy(gameObject);
        }
        
        if(buffInfo.sourceInfo.GetType() == typeof(EntityInfo))
        {
            Revive(buffInfo.sourceInfo.transform.position);
        }
        else if(buffInfo.sourceInfo.GetType() == typeof(SpawnerInfo))
        {

        }
        else
        {
            Revive(hostInfo.transform.position);
        }
    }

    public void Revive(Vector3 position)
    {
        hostInfo.transform.position = position;

        var combatData = new CombatEventData(buffInfo.sourceInfo)
        {
            buff = buffInfo,
            percentValue = healthRevivePercent,
        };

        hostInfo.Revive(combatData);
        hostInfo.currentRoom = hostInfo.CurrentRoom();
        hostInfo.Movement().MoveTo(hostInfo.transform.position);
        
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
