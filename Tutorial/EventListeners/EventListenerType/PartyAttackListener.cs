using Architome.Settings.Keybindings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

namespace Architome.Tutorial
{
    public class PartyAttackListener : EventListener
    {
        [Header("Party Attack Properties")]

        public EntityInfo target;
        public PartyInfo party;
        bool focused;
        private void Start()
        {
            GetDependencies();
            HandleStart();
        }

        public override void StartEventListener()
        {
            base.StartEventListener();


            party.events.OnPartyFocus += OnPartyFocus;

        }

        void OnPartyFocus(EntityInfo target)
        {

            if (target != this.target && this.target != null) return;
            CompleteEventListener();
        }

        public override string Directions()
        {
            var result = new List<string>() { base.Directions() };

            var targetName = target != null ? target.ToString() : "them";
            if (!simple)
            {
                var multipleIndex = keyBindData.SpriteIndex(KeybindSetType.Party, "SelectMultiple");
                var action = keyBindData.SpriteIndex(KeybindSetType.Party, "Action");
                result.Add($"To have the entire party focus a target: Hold <sprite={multipleIndex}>, hover over {targetName},  and click on the target with <sprite={action}>");
            }
            else
            {
                result.Add($"Hover over {targetName} and use the party action on them to attack them");
            }

            return ArchString.NextLineList(result);
        }
    }
}
