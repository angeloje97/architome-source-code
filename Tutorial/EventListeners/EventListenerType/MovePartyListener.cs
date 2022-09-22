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

            var selectMultipleIndex = keyBindData.SpriteIndex("SelectMultiple");
            var actionIndex = keyBindData.SpriteIndex("Action");

            result.Add($"Move all the party members at once by holding <sprite={selectMultipleIndex}> and pressing <sprite={actionIndex}>");

            return ArchString.StringList(result);
        }

        public override string Tips()
        {
            var result = base.Tips();
            return result;
        }
    }
}
