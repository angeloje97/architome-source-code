using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;

public class BuffHealthOverTime : MonoBehaviour
{
    // Start is called before the first frame update
    public BuffInfo buffInfo;

    public GameObject entityObject;
    public EntityInfo entityInfo;

    public bool sourceOfExpiration;
    public DamageType damageType;
    
    public void GetDependencies()
    {
        if(gameObject.GetComponent<BuffInfo>())
        {
            buffInfo = gameObject.GetComponent<BuffInfo>();

            if(buffInfo.hostInfo)
            {
                entityInfo = buffInfo.hostInfo;
            }
            if(buffInfo.hostObject)
            {
                entityObject = buffInfo.hostObject;
            }

            buffInfo.OnBuffInterval += OnBuffInterval;
        }
    }
    void Start()
    {
        GetDependencies();
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnBuffInterval(BuffInfo buff)
    {
        HandleHealthOverTime();
    }


    public void HandleHealthOverTime()
    {
        var value = buffInfo.properties.value;
        switch(buffInfo.buffTargetType)
        { 
            case BuffTargetType.Assist:
                buffInfo.HandleTargetHealth(buffInfo.hostInfo, value, BuffTargetType.Assist);
                break;
            case BuffTargetType.Harm:
                buffInfo.HandleTargetHealth(buffInfo.hostInfo, value, BuffTargetType.Harm);
                break;
        }
    }
}
