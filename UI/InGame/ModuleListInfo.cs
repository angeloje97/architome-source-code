using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Architome
{
    public class ModuleListInfo : ModuleInfo
    {
        // Start is called before the first frame update
        public List<VerticalLayoutGroup> mainLayoutGroups;

        public CanvasGroup listItemsCanvasGroup;
        public Image activeImage;
        public Image deactiveImage;
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

         void UpdateGroups()
        {
            foreach (var item in mainLayoutGroups)
            {
                item.enabled = false;
                item.enabled = true;
            }

            foreach (var item in GetComponentsInChildren<ContentSizeFitter>())
            {
                item.enabled = false;
                item.enabled = true;
            }
        }

        public void UpdateModule()
        {
            ArchAction.IntervalFor(() => {
                UpdateGroups();
            }, .125f, .250f, true);

        }

        public void ToggleListItems()
        {
            var isActive = listItemsCanvasGroup.interactable;
            var toggledState = !isActive;
            listItemsCanvasGroup.interactable = toggledState;
            listItemsCanvasGroup.blocksRaycasts = toggledState;

            listItemsCanvasGroup.alpha = toggledState ? 1 : 0;

            if(activeImage && deactiveImage)
            {
                activeImage.enabled = toggledState;
                deactiveImage.enabled = !toggledState;
            }

        }
    }

}