using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UltimateClean;

namespace Architome
{
    public class TaskModuleInfo : MonoBehaviour
    {
        // Start is called before the first frame update
        public ModuleInfo module;
        public GameObject optionPrefab;
        public TextMeshProUGUI title;

        public ClickableManager clickableManager;
        public Clickable currentClickable;

        public Transform options;
        public VerticalLayoutGroup bottomLayoutGroup;
        
        bool isActive;

        public void GetDependencies()
        {
            if(GetComponentInParent<ModuleInfo>())
            {
                module = GetComponentInParent<ModuleInfo>();
            }


            if(ClickableManager.active)
            {
                clickableManager = ClickableManager.active;
                clickableManager.OnClickableObject += OnClickableObject;
            }
        }
        void Start()
        {
            GetDependencies();
            module.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (!isActive) { return; }
            HandleUserInput();
        }

        public void HandleUserInput()
        {
            if(Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
            {
                if(!module.isHovering)
                {
                    CancelOptions();

                }
            }
        }

        public void OnClickableObject(Clickable data)
        {
            AdjustPosition();
            ClearOptions();
            currentClickable = data;
            SetTitle();
            CreateOptions(data.options);
            UpdateModule(true);
        }

        public void AdjustPosition()
        {
            var rectTransform = GetComponent<RectTransform>();
            var localScale = rectTransform.localScale;

            rectTransform.position = Input.mousePosition - new Vector3(-(localScale.x) * rectTransform.sizeDelta.x / 2, (localScale.y) * rectTransform.sizeDelta.y / 2, 0);
            
        }

        public float Height()
        {
            var totalHeight = 0f;

            foreach(Transform child in options)
            {
                var childHeight = child.GetComponent<RectTransform>().sizeDelta.y;

                totalHeight += childHeight;
            }

            return totalHeight;
        }

        public void SetTitle()
        {
            if(currentClickable.clickedEntity)
            {
                title.text = currentClickable.clickedEntity.entityName;
                return;
            }

            if(currentClickable.clickedEntities.Count == 1)
            {
                title.text = currentClickable.clickedEntities[0].entityName;
                return;
            }

            if(currentClickable.clickedEntities.Count > 1)
            {
                title.text = $"{currentClickable.clickedEntities[0].entityName} + {currentClickable.clickedEntities.Count - 1} more";
            }
        }

        public void ClearOptions()
        {
            foreach (Transform child in options)
            {
                Destroy(child.gameObject);
            }
        }

        public void CancelOptions()
        {
            ClearOptions();
            UpdateModule(false);

            currentClickable.CancelOption();
            currentClickable = null;
        }

        public void SelectOption(string option)
        {
            currentClickable.OnSelectOption?.Invoke(option);

            currentClickable = null;

            ClearOptions();
            UpdateModule(false);
        }

        public void UpdateModule(bool val)
        {
            module.SetActive(val);
            isActive = val;
        }

        public void CreateOptions(List<string> options)
        {
            foreach(var option in options)
            {
                Instantiate(optionPrefab, this.options).GetComponent<ContextOption>().SetOption(option);
            }
        }

    }

}
