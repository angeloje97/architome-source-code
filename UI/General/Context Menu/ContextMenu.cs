using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace Architome
{
    public class ContextMenu : MonoBehaviour
    {
        public static ContextMenu current { get; set; }

        [Serializable]
        public struct Info
        {
            public TextMeshProUGUI title;
            public GameObject optionPrefab;
            public Transform options;
        }

        public ContextOption currentHover;

        public ModuleInfo module;
        public HorizontalOrVerticalLayoutGroup layoutGroup;

        public Info info;
        
        public int pickedOption;
        public bool isChoosing;
        public float startingWidth;

        RectTransform rectTransform;
        KeyBindings keyBindData;
        bool exitKeysActive;


        public Action<ContextMenu, bool> OnContextActiveChange;

        void GetDependencies()
        {
            module = GetComponent<ModuleInfo>();
            layoutGroup = GetComponentInChildren<HorizontalOrVerticalLayoutGroup>();
            rectTransform = GetComponent<RectTransform>();
            module.OnActiveChange += OnActiveChange;
            keyBindData = KeyBindings.active;

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

        public async Task HandleInput()
        {

            await ExitKeys();

            if (isChoosing)
            {
                CancelOptions();
            }

            //var cancelKeys = new List<KeyCode>()
            //{
            //    KeyCode.Mouse0,
            //    KeyCode.Mouse1,
            //    KeyCode.Escape
            //};

            //foreach (var key in cancelKeys)
            //{
            //    if (!Input.GetKeyUp(key)) continue;
            //    CancelOptions();
            //}

            //if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Escape)))
            //{
            //    if (currentHover == null)
            //    {
            //        CancelOptions();
            //    }
            //}
        }

        public async Task ExitKeys()
        {
            exitKeysActive = true;

            var alternateActionsName = new List<string>();

            for (int i = 0; i < 5; i++)
            {
                alternateActionsName.Add($"AlternateAction{i}");
            }

            var exitKeys = new List<KeyCode>() { 
                KeyCode.Mouse0,
                KeyCode.Mouse1,
                KeyCode.Escape
            };

            foreach (var action in alternateActionsName)
            {
                exitKeys.Add(keyBindData.keyBinds[action]);
            }


            while (exitKeysActive)
            {
                if (!isChoosing) break;
                foreach (var key in exitKeys)
                {
                    if (Input.GetKeyUp(key))
                    {
                        exitKeysActive = false;
                    }
                }
                await Task.Yield();
            }

            exitKeysActive = false;
        }

        void OnActiveChange(bool isActive)
        {
            if (isActive) return;
            if (!isChoosing) return;

            CancelOptions();
        }

        void CancelOptions()
        {
            ArchAction.Yield(() => {
                isChoosing = false;
                pickedOption = -1;
            });
        }

        public void PickOption(int option)
        {
            isChoosing = false;
            pickedOption = option;
        }

        async public Task<ContextMenuResponse> UserChoice(ContextMenuData contextData)
        {
            transform.SetAsLastSibling();
            if (isChoosing)
            {
                CancelOptions();
                await Task.Yield();
            }


            ClearOptions();

            isChoosing = true;
            OnContextActiveChange?.Invoke(this, true);
            pickedOption = -1;

            info.title.text = contextData.title;
            CreateOptions(contextData.options);

            await ExitKeys();

            await HandleInput();

            module.SetActive(false);


            var response = new ContextMenuResponse();

            if (pickedOption == -1)
            {
                response.stringValue = "";
                response.index = -1;
            }
            else
            {
                response.stringValue = contextData.options[pickedOption];
                response.index = pickedOption;
            }


            OnContextActiveChange?.Invoke(this, false);
            return response;
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

    public struct ContextMenuResponse
    {
        public string stringValue;
        public int index;
    }

    public struct ContextMenuData
    {
        public string title;
        public List<string> options;

        
    }
}
