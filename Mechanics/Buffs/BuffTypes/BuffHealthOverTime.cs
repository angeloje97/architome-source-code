using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;

public class BuffHealthOverTime : BuffType
{

    public GameObject entityObject;
    public EntityInfo entityInfo;

    public bool sourceOfExpiration;
    public DamageType damageType;
    
    new public void GetDependencies()
    {
        base.GetDependencies();

        if (buffInfo)
        {

            if (buffInfo.hostInfo)
            {
                entityInfo = buffInfo.hostInfo;
            }
            if (buffInfo.hostObject)
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


    public override string Description()
    {
        var result = "";

        if (buffInfo.buffTargetType == BuffTargetType.Assist)
        {
            result += "Heals ";
        }

        if (buffInfo.buffTargetType == BuffTargetType.Harm)
        {
            result += "Damages ";
        }

        result += $" the target for {ArchString.FloatToSimple(value)} health every {buffInfo.properties.intervals} seconds.";

        return result;
    }

    public override string GeneralDescription()
    {
        var buffInfo = GetComponent<BuffInfo>();
        var result = buffInfo.buffTargetType == BuffTargetType.Assist ? "Heals " : "Damages ";

        result += $"target every {buffInfo.properties.intervals} seconds.";

        return result;
    }
    public void OnBuffInterval(BuffInfo buff)
    {
        HandleHealthOverTime();
    }


    public void HandleHealthOverTime()
    {
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
