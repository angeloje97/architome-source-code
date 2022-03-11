using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UltimateClean;
using System.Threading.Tasks;

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
            UpdateModule(false);
            ClearOptions();
            currentClickable = data;
            SetTitle();
            CreateOptions(data.options);
            ArchAction.Delay(() => AdjustPosition(), .0625f);
            ArchAction.Delay(() => UpdateModule(true), .0626f);
        }

        public void AdjustPosition()
        {
            var rectTransform = GetComponent<RectTransform>();
            var localScale = rectTransform.localScale;

            var mousePosX = Input.mousePosition.x;
            var mousePosY = Input.mousePosition.y;

            var xMid = Screen.width/2;
            var yMid = Screen.height/2;

            int xValue = mousePosX > xMid ? 1 : -1;
            int yValue = mousePosY > yMid ? 1 : -1;

            var height = Height();

            Debugger.InConsole(1295, $"{(yValue * (localScale.y) * height / 2)}");

            rectTransform.position = Input.mousePosition - new Vector3((xValue) * (localScale.x) * rectTransform.sizeDelta.x / 2, (yValue) *  height / 2 , 0);
            
        }

        public float Height()
        {
            var totalHeight = 0f;

            foreach(Transform child in options)
            {
                var childHeight = child.GetComponent<RectTransform>().rect.height * child.GetComponent<RectTransform>().localScale.y;

                totalHeight += childHeight;
            }

            Debugger.InConsole(1295, $"{totalHeight}");

            return totalHeight/2;
        }

        public void SetTitle()
        {

            if(currentClickable.clickedEntities.Count == 1)
            {
                title.text = currentClickable.clickedEntities[0].entityName;
                return;
            }

            if(currentClickable.clickedEntities.Count > 1)
            {
                title.text = $"{currentClickable.clickedEntities[0].entityName} + {currentClickable.clickedEntities.Count - 1} more";
                return;
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
            currentClickable.SelectOption(option);

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
