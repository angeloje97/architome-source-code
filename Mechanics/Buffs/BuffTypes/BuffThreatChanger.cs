using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffThreatChanger : MonoBehaviour
{
    // Start is called before the first frame update
    BuffInfo buffInfo;
    public bool increasesThreat;


    
    void Start()
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

        if(!ApplyBuff())
        {
            Destroy(gameObject);
        }
    }

    public bool ApplyBuff()
    {
        if (buffInfo == null) { return false; }
        if (buffInfo.hostInfo == null) { return false; }
        if (buffInfo.sourceInfo == null) { return false; }
        if (!buffInfo.sourceInfo.CanAttack(buffInfo.hostInfo.gameObject)) { return false; }

        var val = buffInfo.properties.value;

        val = increasesThreat ? val : -val;

        buffInfo.hostInfo.ThreatManager().IncreaseThreat(buffInfo.sourceObject, val, true);

        return true;

    }

    // Update is called once per frame

    public void OnBuffCompletion(BuffInfo info)
    {
    }

    public void OnBuffCleanse(BuffInfo info)
    {
    }
}
