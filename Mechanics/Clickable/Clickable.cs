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
        public bool partyCanClick;

        public Action<Clickable> OnSelectOption;
        public Action OnCancelOption;

        public string selectedString;
        public int selectedIndex;
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }


        public void CancelOption()
        {
            clickedEntities.Clear();

            OnCancelOption?.Invoke();
        }

        public void SelectOption(string option)
        {
            selectedIndex = options.IndexOf(option);
            selectedString = option;
            OnSelectOption?.Invoke(this);

            selectedIndex = -1;
            selectedString = null;
            clickedEntities.Clear();
        }

        public void SetOptions(List<string> options)
        {
            this.options = new List<string>(options);
        }

        public void AddOption(string option)
        {
            this.options.Add(option);
        }

        public void ClearOptions()
        {
            this.options = new List<string>();
        }

        public void Click(EntityInfo entity)
        {
            clickedEntities.Clear();
            clickedEntities.Add(entity);
            ClickableManager.active.HandleClickable(this);
        }

        public void ClickMultiple(List<EntityInfo> entities)
        {
            clickedEntities = entities;

            ClickableManager.active.HandleClickable(this);
        }
    }

}
