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
    public class AlertIndicatorBehavior : MonoBehaviour
    {
        // Start is called before the first frame update
        public EntityInfo entityInfo;
        public PortraitBehavior portraitBehavior;

        public bool isFlashing;

        public Image alertImage;
        void GetDependencies()
        {
            entityInfo = GetComponentInParent<EntityInfo>();
            portraitBehavior = GetComponentInParent<PortraitBehavior>();
            if (entityInfo != null)
            {
                var combatInfo = entityInfo.GetComponentInChildren<CombatInfo>();
                combatInfo.OnTargetedByEvent += OnTargetedByEvent;
                return;
            }


            if (portraitBehavior != null)
            {
                portraitBehavior.OnEntityChange += OnEntityChange;

                if (portraitBehavior.entity)
                {

                    var combatInfo = portraitBehavior.entity.GetComponentInChildren<CombatInfo>();

                    combatInfo.OnTargetedByEvent += OnTargetedByEvent;
                }


                return;
            }
        }

        void Start()
        {
            GetDependencies();
        }

        async void FlashingRoutine()
        {
            while (isFlashing)
            {
                await Task.Delay(250);
                
                alertImage.enabled = !alertImage.enabled;
            }

            alertImage.enabled = false;
        }

        async void OnTargetedByEvent(CombatInfo combatInfo)
        {
            if(!Entity.IsPlayer(entityInfo.gameObject)) { return; }
            if (entityInfo.role == Role.Tank) return;
            if (alertImage == null) { return; }
            if (isFlashing) return;

            isFlashing = true;

            var enemiesTargeting = combatInfo.EnemiesTargetedBy();
            var enemiesCasting = combatInfo.EnemiesCastedBy();


            while (enemiesCasting.Count > 0 || enemiesTargeting.Count > 0)
            {
                
                await Task.Delay(250);
                enemiesTargeting = combatInfo.EnemiesTargetedBy();
                enemiesCasting = combatInfo.EnemiesCastedBy();
                alertImage.enabled = !alertImage.enabled;
            }

            isFlashing = false;
            alertImage.enabled = false;

            //if (!combatInfo.IsBeingAttacked())
            //{
            //    return;   
            //}

            //if (isFlashing)
            //{
            //    return;
            //}

            //isFlashing = true;

            //FlashingRoutine();
            //await combatInfo.BeingAttacked();
            //isFlashing = false;
        }

        void OnEntityChange(EntityInfo previous, EntityInfo after)
        {
            if (entityInfo)
            {
                var combatInfo = entityInfo.GetComponentInChildren<CombatInfo>();

                combatInfo.OnTargetedByEvent -= OnTargetedByEvent;

            }

            entityInfo = after;

            if (entityInfo)
            {
                var combatInfo = entityInfo.GetComponentInChildren<CombatInfo>();
                combatInfo.OnTargetedByEvent += OnTargetedByEvent;
            }
        }
    }

}
