using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;
public class BuffStateChanger : MonoBehaviour
{
    public BuffInfo buffInfo;
    public EntityState stateToChange;
    public EntityState originalEntityState;
    public bool applied;

    public Action<BuffStateChanger, EntityState> OnSuccessfulStateChange;
    public Action<BuffStateChanger, EntityState> OnStateChangerEnd;
    // Start is called before the first frame update
    void GetDependencies()
    {
        if (GetComponent<BuffInfo>())
        {
            buffInfo = GetComponent<BuffInfo>();
            buffInfo.OnBuffEnd += OnBuffEnd;


            originalEntityState = buffInfo.hostInfo.currentState;

            ApplyBuff();

            if (applied == true)
            {
                OnSuccessfulStateChange?.Invoke(this, stateToChange);
                return;
            }

            buffInfo.Cleanse();
        }
    }

    void Start()
    {
        GetDependencies();
    }

    public void ApplyBuff()
    {
        applied = buffInfo.hostInfo.ChangeState(stateToChange);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnBuffEnd(BuffInfo buff)
    {
        HandleRemoveState(buff);
    }

    public void HandleRemoveState(BuffInfo buff)
    {
        if (!applied) return;
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

        OnStateChangerEnd?.Invoke(this, newState);
    }
}
