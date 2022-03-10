using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    public class Clickable : MonoBehaviour
    {
        // Start is called before the first frame update
        public List<string> options;
        
        public List<EntityInfo> clickedEntities;
        public EntityInfo clickedEntity;
        public bool partyCanClick;
        public Action<string> OnSelectOption;
        public Action OnCancelOption;
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }


        public void CancelOption()
        {
            clickedEntity = null;
            clickedEntities.Clear();

            OnCancelOption?.Invoke();
        }
        public void Click(EntityInfo entity)
        {
            clickedEntity = entity;


            ClickableManager.active.HandleClickable(this);
        }

        public void ClickMultiple(List<EntityInfo> entities)
        {
            clickedEntities = entities;

            ClickableManager.active.HandleClickable(this);
        }
    }

}
