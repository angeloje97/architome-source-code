using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BuffThreatChanger : BuffType
    {
        public bool increasesThreat;

        new void GetDependencies()
        {
            base.GetDependencies();
            if (GetComponent<BuffInfo>())
            {
                buffInfo = GetComponent<BuffInfo>();
                buffInfo.OnBuffCompletion += OnBuffCompletion;
                buffInfo.OnBuffCleanse += OnBuffCleanse;
            }
            else
            {
                Destroy(gameObject);
            }

            if (!ApplyBuff())
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            GetDependencies();
        }

        public override string Description()
        {
            var result = "";

            result += increasesThreat ? "Increased " : "Decreased ";

            result += $" threat that the target has on the caster by {value} threat.\n";

            return result;
        }


        public override string GeneralDescription()
        {
            var result = increasesThreat ? "Increases " : "Decreases ";

            result += $" threat that the target has on the caster.\n";

            return result;
        }

        public bool ApplyBuff()
        {
            if (buffInfo == null) { return false; }
            if (buffInfo.hostInfo == null) { return false; }
            if (buffInfo.sourceInfo == null) { return false; }
            if (!buffInfo.sourceInfo.CanAttack(buffInfo.hostInfo.gameObject)) { return false; }


            var val = increasesThreat ? value : -value;


            var threatManager = buffInfo.hostInfo.ThreatManager();

            threatManager.IncreaseThreat(buffInfo.sourceObject, val);
            threatManager.AlertAllies(buffInfo.sourceObject);

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

}