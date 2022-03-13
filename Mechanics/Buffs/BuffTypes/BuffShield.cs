using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffShield : MonoBehaviour
{
    // Start is called before the first frame update
    public BuffInfo buffInfo;

    public float shieldAmount;
    public bool applied;
    void GetDependencies()
    {
        if (buffInfo == null)
        {
            if (GetComponent<BuffInfo>())
            {
                buffInfo = GetComponent<BuffInfo>();
                buffInfo.OnBuffEnd += OnBuffEnd;
            }

            if(buffInfo && buffInfo.hostInfo && buffInfo.sourceInfo)
            {
                if (buffInfo.hostInfo == buffInfo.sourceInfo)
                {
                    if (buffInfo && buffInfo.sourceAbility)
                    {
                        shieldAmount = buffInfo.properties.value * buffInfo.sourceAbility.selfCastMultiplier;
                    }
                    
                }
                else
                {
                    shieldAmount = buffInfo.properties.value;
                }
            }

            ApplyBuff();
        }
    }
    void Start()
    {
        GetDependencies();
    }
    void ApplyBuff()
    {
        if(buffInfo && buffInfo.hostInfo)
        {
            applied = true;
            buffInfo.hostInfo.UpdateShield();
        }
    }
    // Update is called once per frame
    void Update()
    {
    }

    public void OnBuffEnd(BuffInfo buff)
    {
        shieldAmount = 0;
        buffInfo.hostInfo.UpdateShield();
    }
    //void HandleBuffExpire()
    //{
    //    if (buffInfo.buffTimeComplete || buffInfo.cleansed)
    //    {
    //        if (!expired)
    //        {
    //            shieldAmount = 0;
    //            expired = true;

    //        }
    //        buffInfo.hostInfo.UpdateShield();
    //    }
    //}

    public float DamageShield(float value)
    {
        var nextValue = shieldAmount > value ? 0 : value - shieldAmount;

        if (value > shieldAmount)
        {
            value = shieldAmount;
        }

        shieldAmount -= value;

        if(buffInfo.hostInfo != buffInfo.sourceInfo)
        {
            buffInfo.sourceInfo.OnDamagePreventedFromShields?.Invoke(new Architome.CombatEventData(buffInfo, buffInfo.sourceInfo, value) {target = buffInfo.sourceInfo});
        }

        if(shieldAmount == 0)
        {
            buffInfo.Deplete();
        }

        buffInfo.hostInfo.UpdateShield();

        return nextValue;
    }
}
