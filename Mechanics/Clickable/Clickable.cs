using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Architome
{
    public class Clickable : MonoBehaviour
    {
        // Start is called before the first frame update
        [Serializable]
        public struct Option
        {
            public string text;
            public UnityEvent OnSelect;
        }

        public List<Option> options;

        //public List<string> options;
        
        public List<EntityInfo> clickedEntities;
        public bool partyCanClick;

        public Action<Clickable> OnSelectOption;
        public Action OnCancelOption;

        public string selectedString;
        public int selectedIndex;

        public bool isOver;
        void Start()
        {
        }

        // Update is called once per frame
        private void OnMouseEnter()
        {
            isOver = true;
            MouseEnterRoutine();
        
            async void MouseEnterRoutine()
            {
                while (isOver)
                {
                    await Task.Yield();

                    if (!Mouse.IsMouseOverUI() && isOver)
                    {
                        if (ClickableManager.active.currentClickableHover != gameObject)
                        {
                            ClickableManager.active.currentClickableHover = gameObject;
                        }
                    }
                    else
                    {
                        if (ClickableManager.active.currentClickableHover != null)
                        {
                            ClickableManager.active.currentClickableHover = null;
                        }
                    }
                }

            }
        }

        private void OnMouseExit()
        {
            isOver = false;
            ClickableManager.active.currentClickableHover = null;
        }

        public void CancelOption()
        {
            clickedEntities.Clear();

            OnCancelOption?.Invoke();
        }

        public void SelectOption(string option)
        {
            
            selectedIndex = options.IndexOf(options.Find(item => item.text == option));
            selectedString = option;
            options[selectedIndex].OnSelect?.Invoke();
            OnSelectOption?.Invoke(this);
            

            selectedIndex = -1;
            selectedString = null;
            clickedEntities.Clear();
        }

        public void SetOptions(List<string> options)
        {
            this.options = options.Select(option => new Option() { text = option }).ToList();
        }

        public void AddOption(string option)
        {
            this.options.Add(new() { text = option });
        }

        public void ClearOptions()
        {
            this.options = new();
        }

        public void Click(EntityInfo entity)
        {
            if (options.Count == 0) return;
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
