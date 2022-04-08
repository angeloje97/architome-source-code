using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class EntityModuleIconManager : MonoBehaviour
    {
        // Start is called before the first frame update
        public GameObject entityIcon;
        public List<EntityModuleIcon> moduleIcons;
        void Start()
        {
            GameManager.active.OnNewPlayableEntity += OnNewPlayableEntity;
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void OnNewPlayableEntity(EntityInfo entity, int index)
        {
            if (entityIcon == null) return;

            var newIcon = Instantiate(entityIcon, transform).GetComponent<EntityModuleIcon>();
            newIcon.SetEntity(entity);

            moduleIcons.Add(newIcon);

            UpdateList();
        }

        void UpdateList()
        {
            foreach (var icon in moduleIcons)
            {
                icon.transform.SetAsFirstSibling();
            }
        }

    }

}