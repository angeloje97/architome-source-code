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
            public UnityEvent<ChoiceData> OnSelect;
        }

        public class ChoiceData
        {
            public Clickable clickable;
            public List<EntityInfo> entitiesClickedWith;

            public ChoiceData(Clickable clickable)
            {
                this.clickable = clickable;
                entitiesClickedWith = clickable.clickedEntities;
            }
        }

        public List<Option> options;

        bool inactive;

        public List<EntityInfo> clickedEntities;
        public bool partyCanClick;

        public Action<Clickable> OnSelectOption;
        public Action OnCancelOption;

        public string selectedString;
        public int selectedIndex;

        public bool isOver;
        ClickableManager manager;
        void Start()
        {
            manager = ClickableManager.active;

            ArchAction.Delay(() => {
                if (options == null || options.Count == 0)
                {
                    enabled = false;
                }
            }, .35f);
        }

        // Update is called once per frame
        private void OnMouseEnter()
        {
            isOver = true;
            MouseEnterRoutine();
        
            async void MouseEnterRoutine()
            {
                while (this != null && isOver)
                {
                    await Task.Yield();
                    if (this == null) return;
                    if (!Mouse.IsMouseOverUI() && isOver)
                    {
                        if (ClickableManager.active.currentClickableHover != gameObject)
                        {
                            ClickableManager.active.currentClickableHover = gameObject;
                        }
                    }
                    else
                    {
                        if (ClickableManager.active.currentClickableHover == gameObject)
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

            if (manager.currentClickableHover == this)
            {
                ClickableManager.active.currentClickableHover = null;
            }
        }

        public void CancelOption()
        {
            clickedEntities.Clear();

            OnCancelOption?.Invoke();
        }

        public void SetActive(bool active)
        {
            inactive = !active;
        }

        public void SelectOption(string option)
        {
            
            selectedIndex = options.IndexOf(options.Find(item => item.text == option));
            selectedString = option;
            options[selectedIndex].OnSelect?.Invoke(new(this));
            OnSelectOption?.Invoke(this);
            

            selectedIndex = -1;
            selectedString = null;
            clickedEntities.Clear();
        }
        async void HandleOptions()
        {
            //List<string> options = this.options.Select(option => option.text).ToList();

            var options = new List<ContextMenu.OptionData>();

            foreach(var option in this.options)
            {
                options.Add(new(option.text));
            }

            var response = await ContextMenu.current.UserChoice(new()
            {
                title = clickedEntities.Count > 1 ? $"{clickedEntities[0].entityName} + {clickedEntities.Count - 1}" : $"{clickedEntities[0].entityName}",
                options = options
            });

            var choice = response.index;

            if (choice == -1) return;

            selectedIndex = choice;
            selectedString = this.options[choice].text;
            this.options[selectedIndex].OnSelect?.Invoke(new(this));
            OnSelectOption?.Invoke(this);

            selectedIndex = -1;
            selectedString = null;
            clickedEntities.Clear();
        }

        public void AddOption(string option)
        {
            this.options.Add(new() { text = option });
        }

        public void ClearOptions()
        {
            this.options = new();
        }

        public void RemoveOptions(Predicate<Option> predicate)
        {
            options ??= new();
            for (int i = 0; i < options.Count; i++)
            {
                if (predicate(options[i]))
                {
                    options.RemoveAt(i);
                    i--;
                }
            }
        }

        public void Click(EntityInfo entity)
        {
            if (options.Count == 0) return;
            clickedEntities.Clear();
            clickedEntities.Add(entity);
            HandleOptions();
        }

        public void ClickMultiple(List<EntityInfo> entities)
        {
            if (options.Count == 0) return;
            clickedEntities = entities;
            HandleOptions();
        }
        public bool Interactable { get { return options.Count > 0; } }
    }

}
