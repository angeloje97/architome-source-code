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

        void Start()
        {
            GetDependencies();
            HandleStart();
        }

        public override void StartEventListener()
        {
            base.StartEventListener();

            targetInfo.OnDeath += (CombatEventData eventData) => {
                CompleteEventListener();
            };
        }

        public override string Directions()
        {
            var actionIndex = keyBindData.SpriteIndex("Action");
            var stringList = new List<string>()
            {
                $"Defeat {targetInfo.entityName}.",
                $"You can attack targets with units by using (Action <sprite={actionIndex}> ) over the target with a selected member."
            };

            return ArchString.NextLineList(stringList);
        }
        public override string Tips()
        {
            var memberIndex = 0;

            var members = sourceInfo.GetComponentInParent<PartyInfo>().GetComponentsInChildren<EntityInfo>();

            for (int i = 0; i < members.Length; i++)
            {
                if (members[i] != sourceInfo) continue;
                memberIndex = 0;

                break;
            }

            var memberActionIndex = keyBindData.SpriteIndex($"AlternateAction{memberIndex}");


            return ArchString.NextLineList(new() {
                base.Tips(),
                $"You can also use the (Member Action {memberIndex + 1} <sprite={memberActionIndex}> ) to attack a target without needing to select the member"
            });
        }
    }
}
