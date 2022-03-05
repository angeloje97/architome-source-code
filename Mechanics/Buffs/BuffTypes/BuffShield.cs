using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffShield : MonoBehaviour
{
    // Start is called before the first frame update
    public BuffInfo buffInfo;

    public float shieldAmount;
    public bool applied;
    public bool expired;
    void GetDependencies()
    {
        if (buffInfo == null)
        {
            if (GetComponent<BuffInfo>())
            {
                buffInfo = GetComponent<BuffInfo>();
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
            
            
        }
    }
    void Start()
    {
        GetDependencies();
        Invoke("ApplyBuff", .125f);
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
        HandleBuffExpire();
    }
    void HandleBuffExpire()
    {
        if (buffInfo.buffTimeComplete || buffInfo.cleansed)
        {
            if (!expired)
            {
                shieldAmount = 0;
                expired = true;

            }
            buffInfo.hostInfo.UpdateShield();
        }
    }

    public void DamageShield(float value)
    {
        shieldAmount -= value;

        if(buffInfo.hostInfo != buffInfo.sourceInfo)
        {
            buffInfo.sourceInfo.GainExp(value * .25f);
        }

        if(shieldAmount == 0)
        {
            buffInfo.depleted = true;
        }

        buffInfo.hostInfo.UpdateShield();
    }
}
