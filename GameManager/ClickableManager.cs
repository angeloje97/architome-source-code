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

        public Action<Clickable> OnClickableObject { get; set; }

        public GameObject currentClickableHover;

        public Action<GameObject, GameObject> OnNewClickableHover { get; set; }

        GameObject clickableHoverCheck;

        void Start()
        {
            ArchInput.active.OnActionMultiple += OnActionMultiple;
            ArchInput.active.OnAction += OnAction;
        }

        private void Awake()
        {
            active = this;
        }
        // Update is called once per frame
        void Update()
        {
            HandleEvents();
        }

        void HandleEvents()
        {
            if (clickableHoverCheck != currentClickableHover)
            {
                OnNewClickableHover?.Invoke(clickableHoverCheck, currentClickableHover);
                clickableHoverCheck = currentClickableHover;
            }
        }


        public void OnActionMultiple()
        {
            if (currentClickableHover == null) return;
            if (GameManager.active.playableParties.Count != 1) return;
            var partyEntites = GameManager.active.playableParties[0].members;

            if (partyEntites.Count == 0) return;

            var entityInfos = new List<EntityInfo>();

            foreach (var member in partyEntites)
            {
                entityInfos.Add(member.GetComponent<EntityInfo>());
            }

            var clickable = currentClickableHover.GetComponent<Clickable>();

            clickable.ClickMultiple(entityInfos);
        }

        public void OnAction()
        {
            if (currentClickableHover == null) return;

            var entityInfos = new List<EntityInfo>();

            foreach (var selected in ContainerTargetables.active.selectedTargets)
            {
                if (!Entity.IsPlayer(selected)) continue;
                entityInfos.Add(selected.GetComponent<EntityInfo>());
            }

            if (entityInfos.Count == 0) return;

            var clickable = currentClickableHover.GetComponent<Clickable>();


            clickable.ClickMultiple(entityInfos);
        }



        public void HandleClickable(Clickable clicked)
        {
            OnClickableObject?.Invoke(clicked);
        }
    }

}
