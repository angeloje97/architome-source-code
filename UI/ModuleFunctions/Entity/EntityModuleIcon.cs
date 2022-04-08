using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Architome
{
    public class EntityModuleIcon : MonoBehaviour
    {
        // Start is called before the first frame update
        [Serializable]
        public struct Info
        {
            public Image entityIcon;
        }

        public Info info;

        public EntityInfo entity;
        public ModuleInfo module;

        void Start()
        {
            ArchAction.Delay(() => {

                module = GetComponentInParent<ModuleInfo>();

            }, .125f);
        }

        public void SetEntity(EntityInfo entity)
        {
            this.entity = entity;
            if (entity.entityPortrait)
            {
                info.entityIcon.sprite = entity.entityPortrait;
            }
        }

        public void SelectEntity()
        {
            if (entity == null) return;
            if (module == null) return;

            module.OnSelectEntity?.Invoke(entity);

        }
    }

}