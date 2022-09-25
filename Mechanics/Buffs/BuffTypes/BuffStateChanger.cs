using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;
namespace Architome
{
    public class BuffStateChanger : BuffType
    {
        public EntityState stateToChange;
        public bool applied;

        public Action<BuffStateChanger, EntityState> OnSuccessfulStateChange;
        public Action<BuffStateChanger, EntityState> OnStateChangerEnd;
        // Start is called before the first frame update
        new void GetDependencies()
        {
            base.GetDependencies();

            if (buffInfo)
            {

                ApplyBuff();

                if (applied == true)
                {
                    OnSuccessfulStateChange?.Invoke(this, stateToChange);
                    buffInfo.OnBuffEnd += delegate (BuffInfo buff)
                    {
                        HandleRemoveState(buff);
                    };
                    return;
                }
                buffInfo.failed = true;
                buffInfo.Cleanse();
            }
        }

        public override string Description()
        {
            return $"{ArchString.CamelToTitle(stateToChange.ToString())}\n";
        }

        public override string GeneralDescription()
        {
            return $"Apply status: {ArchString.CamelToTitle(stateToChange.ToString())} to the target.";
        }

        void Start()
        {
            GetDependencies();
        }

        public void ApplyBuff()
        {
            applied = buffInfo.hostInfo.AddState(stateToChange);

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void HandleRemoveState(BuffInfo buff)
        {
            if (!applied) return;

            buffInfo.hostInfo.RemoveState(stateToChange);

            OnStateChangerEnd?.Invoke(this, stateToChange);
        }
    }

}
