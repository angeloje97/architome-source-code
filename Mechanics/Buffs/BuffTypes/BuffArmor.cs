using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffArmor : MonoBehaviour
{
    // Start is called before the first frame update
    BuffInfo buffInfo;
    public bool expired;
    void GetDependencies()
    {
        if (GetComponent<BuffInfo>())
        {
            buffInfo = GetComponent<BuffInfo>();
        }
    }

    void ApplyBuff()
    {
        if(buffInfo.hostInfo)
        {
            buffInfo.hostInfo.stats.armor += buffInfo.properties.value;
        }
    }
    void Start()
    {
        GetDependencies();
        ApplyBuff();
    }

    // Update is called once per frame
    void Update()
    {
        HandleExpiration();
    }

    void HandleExpiration()
    {
        if(!buffInfo.cleansed && !buffInfo.buffTimeComplete) { return; }
        if(!expired)
        {
            buffInfo.hostInfo.stats.armor -= buffInfo.properties.value;
            expired = true;
        }
    }
}
