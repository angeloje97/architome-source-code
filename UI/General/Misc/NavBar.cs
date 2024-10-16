using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Architome
{
    public class NavBar : MonoBehaviour
    {

        #region ToggleController
        public class ToggleController
        {
            public Toggle toggle;
            public ArchButton button;
            public GameObject target;

            bool interactable => toggle.interactable;

            public ToggleController(Toggle toggle, GameObject target)
            {
                this.toggle = toggle;
                this.target = target;

                button = toggle.GetComponent<ArchButton>();

            }

            public void SetInteractable(bool interactable)
            {
                if(toggle.isOn && !interactable)
                {
                    toggle.isOn = false;
                }

                toggle.interactable = interactable;
                target.SetActive(interactable);
            }
        }
        #endregion

        [Serializable]
        public struct Prefabs
        {
            public Toggle toggleButton;
        }

        [SerializeField] Prefabs prefabs;

        public List<GameObject> items = new();
        public List<CanvasGroup> canvases = new();
        public List<Toggle> toggles = new();

        public float transitionTime;

        public int index;
        public int previousIndex { get; set; }
        int currentIndexCheck;
        public bool update;

        public Action<NavBar> OnChangeNavigation { get; set; }
        public UnityEvent<int> OnChangeNavigationU;
        public Action<NavBar, List<bool>> OnCanNavigateChange { get; set; }

        public bool navigationBlocked;
        public bool changing;

        public bool stallNavigation;

        private void Start()
        {
            toggles = GetComponentsInChildren<Toggle>().ToList();
            OnValueChange();
            previousIndex = -1;
        }

        private void OnValidate()
        {
            if (!update) return;
            update = false;
            toggles = GetComponentsInChildren<Toggle>().ToList();
            UpdateFromIndex();
            ForceUpdate(index);
            UpdateCanvases();
        }

        void UpdateCanvases()
        {
            canvases = new();

            foreach(var item in items)
            {
                var canvas = item.GetComponent<CanvasGroup>();
                canvases.Add(canvas);
            }
        }

        public ToggleController AddToggle(string toggleName, GameObject toggleTarget)
        {
            if (prefabs.toggleButton == null) return null;

            var newToggle = Instantiate(prefabs.toggleButton, transform).GetComponent<Toggle>();
            toggles.Add(newToggle);

            var archButton = newToggle.GetComponent<ArchButton>();
            archButton.SetName(toggleName);

            if (toggleTarget == null) return null;

            for (int i = 0; i < toggles.Count; i++)
            {
                if (i >= items.Count)
                {
                    items.Add(null);
                }
                

                if (toggles[i] == newToggle)
                {
                    items[i] = toggleTarget;
                }
            }

            return new(newToggle, toggleTarget);
        }

        public void ClearToggles()
        {
            foreach (var toggle in toggles)
            {
                Destroy(toggle.gameObject);
            }


            toggles = new();
            items = new();
        }

        public void UpdateFromIndex()
        {
            if (toggles == null) return;
            if (toggles.Count == 0) return;

            index = Mathf.Clamp(index, 0, toggles.Count - 1);

            toggles[index].isOn = true;

        }

        public void ForceUpdate(int index)
        {
            this.index = index;
            for(int i =0; i < items.Count; i++)
            {
                if(toggles.Count == items.Count)
                {
                    toggles[i].isOn = i == index;
                }
                var item = items[i];
                if (item == null) continue;
                var canvasGroup = item.GetComponent<CanvasGroup>();
                if (canvasGroup == null) continue;
                
                ArchUI.SetCanvas(canvasGroup, i == index);
            }
        }

        public async void UpdateFromIndex(int newIndex)
        {
            if (index == newIndex) return;

            if(toggles == null || toggles.Count == 0)
            {
                var canNavigate = await CanNavigate();
                if (!canNavigate) return;
                index = newIndex;
                UpdateNoToggles();
            }
            else
            {
                index = newIndex;
                UpdateFromIndex();
            }

        }

        void UpdateNoToggles()
        {
            for(int i = 0; i < items.Count; i++)
            {
                if (items[i] == null) continue;
                var canvas = items[i].GetComponent<CanvasGroup>();
                ArchUI.SetCanvas(canvas, i == index);
            }
        }

        async Task<bool> CanNavigate()
        {
            while (stallNavigation) await Task.Yield();
            stallNavigation = false;


            var checks = new List<bool>();
            OnCanNavigateChange?.Invoke(this, checks);

            while (stallNavigation) await Task.Yield();

            foreach(var check in checks)
            {
                if (!check) return false;
            }

            return true;
        }
        public async void OnValueChange()
        {
            if (items == null) return;
            if (toggles == null) return;
            if (navigationBlocked) return;
            if (previousIndex == index) return;

            while (changing) await Task.Yield();

            var activeCanvases = new HashSet<CanvasGroup>();
            var targetCanvases = new HashSet<CanvasGroup>();

            for (int i = 0; i < toggles.Count; i++)
            {
                if(items.Count <= i || items[i] == null)
                {
                    if (toggles[i].isOn)
                    {
                        index = i;
                        previousIndex = -1;
                    }

                    continue;
                }
                var toggle = toggles[i];
                var canvasGroup = items[i].GetComponent<CanvasGroup>();

                if (canvasGroup.alpha == 1) 
                {
                    activeCanvases.Add(canvasGroup);
                }

                if (toggle.isOn)
                {
                    if (activeCanvases.Contains(canvasGroup)) return;
                    targetCanvases.Add(canvasGroup);
                    if (i == index) return;
                    previousIndex = index;
                    index = i;
                }

                if (canvasGroup == null)
                {
                    canvasGroup = items[i].AddComponent<CanvasGroup>();
                }
            }
            var canNavigate = await CanNavigate();
            if (!canNavigate)
            {
                var temp = previousIndex;
                previousIndex = index;
                index = temp;
                toggles[index].isOn = true;
                return;
            }

            changing = true;
            OnChangeNavigationU?.Invoke(index);
            foreach(var group in activeCanvases)
            {
                await group.SetCanvasAsync(false, transitionTime);
            }

            foreach(var group in targetCanvases)
            {
                await group.SetCanvasAsync(true, transitionTime);
            }
            changing = false;


        }

        public int ActiveIndex()
        {
            for(int i = 0; i < toggles.Count; i++)
            {
                if (toggles[i].isOn)
                {
                    return i;
                }
            }
            return 0;
        }
    }
}
