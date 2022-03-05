using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffMovementSpeedChange : MonoBehaviour
{
    // Start is called before the first frame update
    public BuffInfo buffInfo;
    
    public float changeAmount;

    void Start()
    {
        if(GetComponent<BuffInfo>())
        {
            buffInfo = GetComponent<BuffInfo>();
            buffInfo.OnBuffCompletion += OnBuffCompletion;
            buffInfo.OnBuffCleanse += OnBuffCleanse;

            buffInfo.hostInfo.stats.movementSpeed += changeAmount;
        }
    }

    public void OnBuffCompletion(BuffInfo buff)
    {
        buffInfo.hostInfo.stats.movementSpeed -= changeAmount;
    }

    public void OnBuffCleanse(BuffInfo buff)
    {
        buffInfo.hostInfo.stats.movementSpeed -= changeAmount;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
