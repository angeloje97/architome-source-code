using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class GeneralToolTipHandler : MonoBehaviour
    {
        ToolTip current;
        ToolTipManager manager;
        ContainerTargetables targetManager;

        void GetDependencies()
        {
            targetManager = ContainerTargetables.active;
            
            if (targetManager)
            {
                targetManager.OnNewHoverTarget += OnNewHoverTarget;
            }
        }

        private void OnValidate()
        {
            manager = GetComponent<ToolTipManager>();
        }
        private void Start()
        {
            GetDependencies();
        }

        void OnNewHoverTarget(GameObject before, GameObject after)
        {
            if (manager == null) return;
            if (current) current.DestroySelf();
            if (after == null) return;

            var info = after.GetComponent<EntityInfo>();

            if (info == null) return;

            current = manager.Side();

            if (current == null) return;

            var className = info.archClass ? info.archClass.className : "";

            current.SetToolTip(new() {
                name = info.entityName,
                type = className,
                subeHeadline = info.role.ToString(),
                description = info.entityDescription,
                attributes = $"{info.npcType} {info.rarity}",
                requirements = info.ObjectivesDescription(),
                value = $"Level {info.entityStats.Level}",
            });


        }
    }
}
