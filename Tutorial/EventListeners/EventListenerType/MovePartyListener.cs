using Architome.Settings.Keybindings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.Tutorial
{
    public class MovePartyListener : EventListener
    {
        [Header("Move Party Properties")]

        public PartyInfo party;

        void Start()
        {
            GetDependencies();
            HandleStart();
        }

        public override void StartEventListener()
        {
            base.StartEventListener();


            Action<PartyFormation> action = (PartyFormation formation) => {
                CompleteEventListener();
            };

            party.events.OnMoveFormation += action;

            OnSuccessfulEvent += (EventListener listener) => {
                party.events.OnMoveFormation -= action;
            };

        }

        public override string Directions()
        {
            var result = new List<string>()
            {
                base.Directions()
            };

            var selectMultipleIndex = keyBindData.SpriteIndex(KeybindSetType.Party, "SelectMultiple");
            var actionIndex = keyBindData.SpriteIndex(KeybindSetType.Party, "Action");

            if (!simple)
            {
                result.Add($"Move all the party members at once by holding <sprite={selectMultipleIndex}> and pressing <sprite={actionIndex}>");
            }

            return ArchString.StringList(result);
        }

        public override string Tips()
        {
            var result = new List<string>() { base.Tips() };

            if (!simple)
            {
                var actionIndex = keyBindData.SpriteIndex(KeybindSetType.Party, "Action");
                result.Add($"You can also hold <sprite={actionIndex}> and move you mouse to change the direction of the formation");
            }
            return ArchString.NextLineList(result);
        }
    }
}
