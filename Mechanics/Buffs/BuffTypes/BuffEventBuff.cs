using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class BuffEventBuff : BuffType
    {
        [Serializable]
        public struct EventApplication
        {
            public BuffEvents eventTrigger;
            public List<BuffInfo> buffs;
        }

        public List<EventApplication> eventApplications;

        void Start()
        {
            GetDependencies();
        }

        public override string GeneralDescription()
        {
            if (eventApplications == null) return "";

            var applicationDescriptions = new List<string>();

            foreach (var application in eventApplications)
            {
                if (application.buffs == null || application.buffs.Count == 0) continue;
                var description = $"{ArchString.CamelToTitle(application.eventTrigger.ToString())} apply: ";

                var buffNames = new List<string>();

                foreach (var buff in application.buffs)
                {
                    buffNames.Add(buff.name);
                }

                description += ArchString.StringList(buffNames);

                applicationDescriptions.Add(description);
          
            }
            return ArchString.NextLineList(applicationDescriptions);
        }

        new void GetDependencies()
        {
            base.GetDependencies();

            if (eventApplications == null) return;

            var buffManager = buffInfo.hostInfo.Buffs();

            foreach (var application in eventApplications)
            {
                if (application.buffs == null || application.buffs.Count == 0) continue;

                buffInfo.AddEventAction(application.eventTrigger, () => {
                    
                    foreach (var buff in application.buffs)
                    {
                        buffManager.ApplyBuff(new(buff.gameObject, buffInfo));
                    }
                });
            }
        }
    }
}
