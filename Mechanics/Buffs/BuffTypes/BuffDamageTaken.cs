using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffDamageTaken : MonoBehaviour
{
    // Start is called before the first frame update
    public BuffInfo buffInfo;
    public float damageTakenAmount;
    public void GetDependencies()
    {
        if(GetComponent<BuffInfo>())
        {
            buffInfo = GetComponent<BuffInfo>();
            buffInfo.OnBuffCompletion += OnBuffCompletion;
            buffInfo.OnBuffCleanse += OnBuffCleanse;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        GetDependencies();
        ApplyBuff(true);

    }
    // Update is called once per frame

    public void OnBuffCompletion(BuffInfo buff)
    {
        ApplyBuff(false);
    }

    public void OnBuffCleanse(BuffInfo buff)
    {
        ApplyBuff(false);
    }
    void ApplyBuff(bool val)
    {
        if(val)
        {
            buffInfo.hostInfo.stats.damageTakenMultiplier += damageTakenAmount;
        }
        else
        {
            buffInfo.hostInfo.stats.damageTakenMultiplier -= damageTakenAmount;
        }
    }
}
