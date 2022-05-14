using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using System;
using UnityEngine.UI;
namespace Architome
{
    public class ContextMenu : MonoBehaviour
    {
        public static ContextMenu current;

        [Serializable]
        public struct Info
        {
            public TextMeshProUGUI title;
            public GameObject optionPrefab;
            public Transform options;
        }

        public ModuleInfo module;
        public HorizontalOrVerticalLayoutGroup layoutGroup;

        public Info info;

        int pickedOption;
        bool isChoosing;
        float startingWidth;

        RectTransform rectTransform;

        void GetDependencies()
        {
            module = GetComponent<ModuleInfo>();
            layoutGroup = GetComponentInChildren<HorizontalOrVerticalLayoutGroup>();
            rectTransform = GetComponent<RectTransform>();
            module.OnActiveChange += OnActiveChange;

            startingWidth = rectTransform.rect.width;
        }

        private void Awake()
        {
            current = this;
        }

        public void Start()
        {
            GetDependencies();
        }

        public void Update()
        {
            if (!isChoosing) return;
            HandleInput();
        }
        public void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (!module.isHovering)
                {
                    CancelOptions();
                }
            }
        }

        void OnActiveChange(bool isActive)
        {
            if (isActive) return;
            if (!isChoosing) return;

            CancelOptions();
        }

        void CancelOptions()
        {
            isChoosing = false;
            pickedOption = -1;
        }

        public void PickOption(int option)
        {
            isChoosing = false;
            pickedOption = option;
        }

        async public Task<int> UserChoice(ContextMenuData contextData)
        {
            if (isChoosing)
            {
                CancelOptions();
                await Task.Yield();
            }

            ClearOptions();

            isChoosing = true;
            pickedOption = -1;

            info.title.text = contextData.title;
            CreateOptions(contextData.options);


            while (isChoosing)
            {
                await Task.Yield();
            }

            module.SetActive(false);

            return pickedOption;
        }

        void CreateOptions(List<string> options)
        {
            for (int i = 0; i < options.Count; i++)
            {
                var context = Instantiate(info.optionPrefab, info.options).GetComponent<ContextOption>();

                context.SetOption(options[i], i);
            }

            layoutGroup.enabled = true;

            ArchAction.Delay(() => { AdjustPosition(); }, .0625f);
            ArchAction.Delay(() => { module.SetActive(true); }, .0625f);
        }

        void ClearOptions()
        {
            layoutGroup.enabled = false;
            foreach (Transform child in info.options)
            {
                Destroy(child.gameObject);

            }
        }



        void AdjustPosition()
        {

            rectTransform.sizeDelta = info.options.GetComponent<RectTransform>().sizeDelta;


            var xMousePos = Input.mousePosition.x;
            var yMousePos = Input.mousePosition.y;

            var screenWidth = Screen.width / 2;
            var screenHeight = Screen.height / 2;

            var xValue = xMousePos > screenWidth ? 1 : 0;
            var yValue = yMousePos > screenHeight ? 1 : 0;


            rectTransform.pivot = new Vector2(xValue, yValue);

            transform.position = Input.mousePosition;

        }

        
    }

    public struct ContextMenuData
    {
        public string title;
        public List<string> options;

        
    }
}
