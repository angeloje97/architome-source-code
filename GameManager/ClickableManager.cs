using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Architome
{
    public class ClickableManager : MonoBehaviour
    {
        public static ClickableManager active;

        public Action<Clickable> OnClickableObject;

        void Start()
        {

        }

        private void Awake()
        {
            active = this;
        }
        // Update is called once per frame
        void Update()
        {
            HandleUserInput();
        }

        public void HandleUserInput()
        {
            if(Input.GetKeyDown(KeyBindings.active.keyBinds["Action"]))
            {
                var clickableObject = Mouse.CurrentHoverObject();


                if(clickableObject == null || clickableObject.GetComponent<Clickable>() == null) { return; }

                var clickable = clickableObject.GetComponent<Clickable>();

                if (HandlePartyClicked(clickable)) { return; }

                HandleSelectedClicked(clickable);
                

            }
        }

        public bool HandlePartyClicked(Clickable clickable)
        {
            if (!Input.GetKey(KeyBindings.active.keyBinds["SelectMultiple"])) { return false; }

            if (GMHelper.GameManager().playableParties.Count != 1) return false;

            var partyEntities = GMHelper.GameManager().playableParties[0].members;

            if(partyEntities.Count == 0) { return false; }

            var entityInfos = new List<EntityInfo>();

            foreach(var entity in partyEntities)
            {
                entityInfos.Add(entity.GetComponent<EntityInfo>());
            }

            clickable.ClickMultiple(entityInfos);

            return true;
        }

        public void HandleSelectedClicked(Clickable clickable)
        {
            var selectedEntities = ContainerTargetables.active.selectedTargets.Where(entity => Entity.IsPlayer(entity)).ToList();


            if (selectedEntities.Count == 0) { return; }

            var entityInfos = new List<EntityInfo>();

            foreach (var selected in selectedEntities)
            {
                if (selected.GetComponent<EntityInfo>())
                {
                    entityInfos.Add(selected.GetComponent<EntityInfo>());
                }
            }

            clickable.ClickMultiple(entityInfos);
        }


        public void HandleClickable(Clickable clicked)
        {
            OnClickableObject?.Invoke(clicked);
        }
    }

}
