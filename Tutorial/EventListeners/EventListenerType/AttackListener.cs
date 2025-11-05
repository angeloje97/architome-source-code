using Architome.Settings.Keybindings;
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
        public List<EntityInfo> targets;

        void Start()
        {
            GetDependencies();
            HandleStart();
        }

        public override void StartEventListener()
        {
            base.StartEventListener();

            if (targets == null) return;

            int targetCounts = 0;
            var deadTargets = new List<EntityInfo>();

            ArchAction.Delay(() => {
                if (completed) return;
                foreach(var target in targets)
                {
                    if (target.isAlive) return;
                }

                CompleteEventListener();
            }, 2f);
            


            foreach (var target in targets)
            {
                if (target.isAlive)
                {
                    targetCounts++;

                    target.combatEvents.AddListenerHealthLimit(eHealthEvent.OnDeath, OnDeath, this);
                }
            }

            void OnDeath(CombatEvent eventData)
            {
                if (deadTargets.Contains(eventData.target)) return;
                deadTargets.Add(eventData.target);

                if (deadTargets.Count != targetCounts) return;

                CompleteEventListener();
            }
        }

        public override string Directions()
        {
            var actionIndex = keyBindData.SpriteIndex(KeybindSetType.Party, "Action");


            var stringList = new List<string>()
            {
                base.Directions(),
            };



            if (targets != null && targets.Count > 0)
            {
                var newString = "Defeat ";
                var targetList = new List<string>();
                var targets = new List<string>();

                foreach (var target in this.targets)
                {

                    bool contains = false;
                    
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if ($"{target}".Equals(targets[i]))
                        {
                            if (!targetList.Contains($"{target}s"))
                            {
                                targetList[i] += "s";
                            }
                            contains = true;
                            break;
                        }
                    }

                    targets.Add(target.entityName);

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


            

            var memberActionIndex = keyBindData.SpriteIndex(KeybindSetType.Party, $"AlternateAction{memberIndex+1}");

            if (!simple)
            {
                var members = sourceInfo.GetComponentInParent<PartyInfo>().GetComponentsInChildren<EntityInfo>();

                for (int i = 0; i < members.Length; i++)
                {
                    if (members[i] != sourceInfo) continue;
                    memberIndex = 0;

                    break;
                }

                result.Add($"You can also use quick attack (for member {memberIndex + 1} <sprite={memberActionIndex}> ) to attack a target without needing to select the member");

            }

            return ArchString.NextLineList(result);
        }
    }
}
