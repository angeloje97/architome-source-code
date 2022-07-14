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
        public PortraitBehavior portraitBehavior;

        public bool isFlashing;

        public Image alertImage;
        public bool show;

        public bool isPortrait;
        public CanvasGroup cGroup;

        void GetDependencies()
        {
            portraitBehavior = GetComponentInParent<PortraitBehavior>();
            entityInfo = GetComponentInParent<EntityInfo>();


            if (portraitBehavior != null)
            {
                portraitBehavior.OnEntityChange += OnEntityChange;

                if (portraitBehavior.entity)
                {
                    isPortrait = true;
                    combatInfo = portraitBehavior.entity.GetComponentInChildren<CombatInfo>();
                }

                return;
            }
            if (entityInfo != null)
            {
                combatInfo = entityInfo.GetComponentInChildren<CombatInfo>();
            }

            if (combatInfo)
            {
                combatInfo.OnTargetedByEvent += OnTargetedByEvent;
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
            UpdateImage();
        }

        async void FlashingRoutine()
        {
            while (isFlashing)
            {
                await Task.Delay(250);

                show = !show;
                UpdateImage();
            }

            show = false;
            UpdateImage();
        }

        void UpdateImage()
        {
            cGroup.alpha = show ? 1 : 0;
        }

        async void OnTargetedByEvent(CombatInfo combatInfo)
        {
            if(!Entity.IsPlayer(entityInfo.gameObject)) { return; }
            if (!this.combatInfo.IsBeingAttacked()) return;
            if (entityInfo.role == Role.Tank) return;
            if (alertImage == null) { return; }
            if (isFlashing) return;

            isFlashing = true;

            await Task.Delay(250);


            FlashingRoutine();

            while (this.combatInfo.IsBeingAttacked())
            {
                await Task.Delay(1000);
            }

            isFlashing = false;
            show = false;
            UpdateImage();

        }

        void OnEntityChange(EntityInfo previous, EntityInfo after)
        {
            if (entityInfo)
            {
                combatInfo = entityInfo.GetComponentInChildren<CombatInfo>();
                combatInfo.OnTargetedByEvent -= OnTargetedByEvent;
            }

            entityInfo = after;

            if (entityInfo)
            {
                combatInfo = entityInfo.GetComponentInChildren<CombatInfo>();
                combatInfo.OnTargetedByEvent += OnTargetedByEvent;
            }
        }
    }

}
