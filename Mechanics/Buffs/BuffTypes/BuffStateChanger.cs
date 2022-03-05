using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
public class BuffStateChanger : MonoBehaviour
{
    public BuffInfo buffInfo;
    public EntityState stateToChange;
    public EntityState originalEntityState;
    public bool applied;
    // Start is called before the first frame update
    void GetDependencies()
    {
        if (GetComponent<BuffInfo>())
        {
            buffInfo = GetComponent<BuffInfo>();
            buffInfo.OnBuffCleanse += OnBuffCleanse;
            buffInfo.OnBuffCompletion += OnBuffCompletion;


            originalEntityState = buffInfo.hostInfo.currentState;

            if (!ApplyBuff())
            {
                buffInfo.Cleanse();
            }
        }
    }

    void Start()
    {
        GetDependencies();
    }

    public bool ApplyBuff()
    {
        bool applied = buffInfo.hostInfo.ChangeState(stateToChange);

        return applied;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnBuffCompletion(BuffInfo buff)
    {
        HandleRemoveState(buff);
    }

    public void OnBuffCleanse(BuffInfo buff)
    {
        HandleRemoveState(buff);
    }

    public void HandleRemoveState(BuffInfo buff)
    {
        var buffs = buff.hostInfo.Buffs().Buffs();
        var newState = EntityState.Active;
        foreach (var i in buffs)
        {
            if (i != buff && i.GetComponent<BuffStateChanger>())
            {
                newState = i.GetComponent<BuffStateChanger>().stateToChange;
            }
        }

        buff.hostInfo.ChangeState(newState);
    }
}
