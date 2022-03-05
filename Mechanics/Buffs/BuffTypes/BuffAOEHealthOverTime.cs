using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
public class BuffAOEHealthOverTime : MonoBehaviour
{
    // Start is called before the first frame update
    public BuffInfo buffInfo;
    public AOEType aoeType;
    public bool expired;

    public bool isHealing;
    public bool isDamage;

    public List<EntityInfo> alliesWithinRange;
    public List<EntityInfo> enemiesWithinRange;

    void Start()
    {
        if(GetComponent<BuffInfo>())
        {
            buffInfo = GetComponent<BuffInfo>();
            buffInfo.OnBuffInterval += OnBuffInterval;
        }
    }

    public void OnBuffInterval(BuffInfo buff)
    {
        HandleDamage();
        HandleHealing();
    }

    void HandleDamage()
    {
        if(!isDamage) { return; }

        var enemies = buffInfo.EnemiesWithinRange();
        var value = ProcessedValue(enemies.Count);


        foreach(var i in enemies)
        {
            buffInfo.HandleTargetHealth(i, value, BuffTargetType.Harm);
        }
    }
            
    void HandleHealing()
    {
        if(!isHealing) { return; }

        var allies = buffInfo.AlliesWithinRange();
        alliesWithinRange = allies;
        var value = ProcessedValue(allies.Count);
        foreach (var i in allies)
        {
            buffInfo.HandleTargetHealth(i, value, BuffTargetType.Assist);
        }
    }

    public float ProcessedValue(int count)
    {
        var aoeValue = buffInfo.properties.aoeValue;
        switch (aoeType)
        {
            case AOEType.Distribute:
                return aoeValue / count;
            case AOEType.Multiply:
                return aoeValue* count;
            default:
                return aoeValue;
        }
    }

    // Update is called once per frame
}
