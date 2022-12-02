using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UI;
using Architome.Enums;

namespace Architome
{
    [RequireComponent(typeof(CanvasGroup))]
    public class AlertIndicatorBehavior : MonoBehaviour
    {
        // Start is called before the first frame update
        public EntityInfo entityInfo;
        CombatInfo combatInfo;
        public Animator animator;
        public PortraitBehavior portraitBehavior;


        public Image alertImage;

        public CanvasGroup cGroup;


        bool active = false;

        void GetDependencies()
        {
            portraitBehavior = GetComponentInParent<PortraitBehavior>();
            entityInfo = GetComponentInParent<EntityInfo>();
            animator = GetComponent<Animator>();

            if (animator == null) return;


            if (portraitBehavior != null)
            {
                portraitBehavior.OnEntityChange += OnEntityChange;

                if (portraitBehavior.entity)
                {
                    entityInfo = portraitBehavior.entity;
                }


            }
            if (entityInfo != null)
            {
                combatInfo = entityInfo.CombatInfo();
            }

            if (combatInfo)
            {
                combatInfo.OnIsBeingAttackedChange += HandleBeingAttackedChange; 

            }
        }

        void Start()
        {
            GetDependencies();
        }

        private void OnValidate()
        {
            cGroup = GetComponent<CanvasGroup>();
            alertImage.enabled = true;
        }

        async void HandleBeingAttackedChange(CombatInfo info, bool isBeingAttacked)
        {
            if (!Entity.IsPlayer(entityInfo)) return;
            if (!isBeingAttacked) return;
            if (entityInfo.role == Role.Tank) return;
            if (alertImage == null) return;
            if (active) return;
            active = true;

            animator.SetBool("ShowAlert", true);

            while (info.isBeingAttacked)
            {
                await Task.Yield();
            }

            animator.SetBool("ShowAlert", false);
            active = false;
        }

        void OnEntityChange(EntityInfo previous, EntityInfo after)
        {
            if (entityInfo)
            {
                combatInfo = entityInfo.CombatInfo();
                combatInfo.OnIsBeingAttackedChange -= HandleBeingAttackedChange;
            }

            entityInfo = after;

            if (entityInfo)
            {
                combatInfo = entityInfo.CombatInfo();
                combatInfo.OnIsBeingAttackedChange += HandleBeingAttackedChange; 
            }
        }
    }

}
