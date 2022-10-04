using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.Tutorial
{
    public class AttackListener : EventListener
    {
        [Header("Kill Listener Properties")]
        public EntityInfo sourceInfo;
        public EntityInfo targetInfo;
        public List<EntityInfo> targets;

        void Start()
        {
            GetDependencies();
            HandleStart();
            HandleTargetsBeforeStart();
        }

        void HandleTargetsBeforeStart()
        {
            
            if (targets != null)
            {
                foreach (var target in targets)
                {
                    PreventEntityDeathBeforeStart(target);
                }
            }

            if (targetInfo)
            {
                PreventEntityDeathBeforeStart(targetInfo);
            }
        }

        public override void StartEventListener()
        {
            base.StartEventListener();

            if (targetInfo)
            {
                targetInfo.OnDeath += (CombatEventData eventData) => {
                    CompleteEventListener();
                };

            }

            if (targets == null) return;

            int targetCounts = 0;
            var deadTargets = new List<EntityInfo>();
            


            foreach (var target in targets)
            {
                if (target.isAlive)
                {
                    targetCounts++;

                    target.OnDeath += OnDeath;

                }
            }

            void OnDeath(CombatEventData eventData)
            {
                if (deadTargets.Contains(eventData.target)) return;
                deadTargets.Add(eventData.target);
                eventData.target.OnDeath -= OnDeath;

                if (deadTargets.Count != targetCounts) return;

                CompleteEventListener();
            }
        }

        public override string Directions()
        {
            var actionIndex = keyBindData.SpriteIndex("Action");


            var stringList = new List<string>();


            if (targetInfo)
            {
                stringList.Add($"Defeat {targetInfo}.");
            }

            if (targets != null && targets.Count > 0)
            {
                var newString = "Defeat ";
                var targetList = new List<string>();

                foreach (var target in targets)
                {

                    bool contains = false;
                    
                    for (int i = 0; i < targetList.Count; i++)
                    {
                        if ($"{target}".Equals(targetList[i]))
                        {
                            targetList[i] += "s";
                            contains = true;
                        }
                    }

                    if (!contains)
                    {
                        targetList.Add($"{target}");

                    }

                }

                newString += $"{ArchString.StringList(targetList)}.";

                stringList.Add(newString);
            }



            if (!simple)
            {
                stringList.Add($"You can attack targets with units by using (Action <sprite={actionIndex}>) over the target with a selected member.");
            }

            return ArchString.NextLineList(stringList);
        }
        public override string Tips()
        {
            var result = new List<string>() { base.Tips() };
            var memberIndex = 0;

            var members = sourceInfo.GetComponentInParent<PartyInfo>().GetComponentsInChildren<EntityInfo>();

            for (int i = 0; i < members.Length; i++)
            {
                if (members[i] != sourceInfo) continue;
                memberIndex = 0;

                break;
            }

            var memberActionIndex = keyBindData.SpriteIndex($"AlternateAction{memberIndex}");

            if (!simple)
            {
                result.Add($"You can also use the (Member Action {memberIndex + 1} <sprite={memberActionIndex}> ) to attack a target without needing to select the member");

            }

            return ArchString.NextLineList(result);
        }
    }
}
