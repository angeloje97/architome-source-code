using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
public class BuffAOEHealthOverTime : BuffType
{
    public AOEType aoeType;
    public bool expired;

    public bool isHealing;
    public bool isDamage;


    new void GetDependencies()
    {
        base.GetDependencies();

        if (buffInfo)
        {
            buffInfo.OnBuffInterval += OnBuffInterval;
        }
    }

    void Start()
    {
        GetDependencies();
    }

    public override string Description()
    {
        if (!isHealing && !isDamage) return "";

        var result = $"Every {ArchString.FloatToSimple(buffInfo.properties.intervals)} seconds in a {ArchString.FloatToSimple(buffInfo.properties.radius)} meter radius. ";

        if (isHealing)
        {
            result += $"Heal all allies ";

            if (isDamage)
            {
                result += "and ";
            }
        }

        if (isDamage)
        {
            result += $"Damage all enemies ";
        }

        result += $"for a base value of {ArchString.FloatToSimple(value)} that {ArchString.CamelToTitle(aoeType.ToString())} through all targets.\n";
        return result;
    }

    public override string GeneralDescription()
    {
        var buffInfo = GetComponent<BuffInfo>();
        var result = buffInfo.buffTargetType == BuffTargetType.Assist ? "Heals allies " : "Damages enemies ";

        result += $"in a {buffInfo.properties.radius} meter radius every {buffInfo.properties.intervals} seconds.\n";

        return result;
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
        var value = ProcessedValue(allies.Count);
        foreach (var i in allies)
        {
            buffInfo.HandleTargetHealth(i, value, BuffTargetType.Assist);
        }
    }

    public float ProcessedValue(int count)
    {
        switch (aoeType)
        {
            case AOEType.Distribute:
                return value / count;
            case AOEType.Multiply:
                return value * count;
            default:
                return value;
        }
    }

    // Update is called once per frame
}
