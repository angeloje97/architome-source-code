using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Architome.Enums;

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
        bool isChoosingCheck;
        ArchInput archInput;

        ArchInput inputManager => archInput ??= ArchInput.active;


        public Action<ContextMenu, bool> OnContextActiveChange;
        public Action<ContextMenu, bool> OnChoosingChange;

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
            HandlePauseMenu();
            HandleModuleVisiblity();
        }

        private void Update()
        {
            HandleEvents();
        }


        void HandleEvents()
        {
            if (isChoosingCheck != isChoosing)
            {
                isChoosingCheck = isChoosing;
                OnChoosingChange?.Invoke(this, isChoosing);
            }
        }

        void HandleModuleVisiblity()
        {
            OnChoosingChange += (ContextMenu menu, bool isChoosing) => {
                module.SetActive(isChoosing);
            };
        }

        void HandlePauseMenu()
        {
            var pauseMenu = PauseMenu.active;
            if (pauseMenu == null) return;

            pauseMenu.OnCanOpenCheck += (pause, checks) =>
            {
                if (isChoosing)
                {
                    checks.Add(false);
                }
            };

            pauseMenu.OnTryOpenPause += (pause) => {
                if (isChoosing)
                {
                    pauseMenu.pauseBlocked = true;

                }
            };
        }
        public async Task HandleInput()
        {

            await ExitKeys();

            if (isChoosing)
            {
                CancelOptions();
            }
        }

        void HandleArchInput()
        {
            if (inputManager == null) return;
            inputManager.SetTempInput(ArchInputMode.InGameUI, (object obj) => isChoosing);
        }

        public async Task ExitKeys()
        {
            exitKeysActive = true;

            var alternateActionsName = new List<string>();

            for (int i = 0; i < 5; i++)
            {
                alternateActionsName.Add($"AlternateAction{i}");
            }

            var exitKeyUps = new List<KeyCode>() { 
                KeyCode.Mouse0,
                //KeyCode.Mouse1,
                KeyCode.Escape
            };


            var exitKeyDowns = new List<KeyCode>() { KeyCode.Mouse1 };

            if (ArchInput.active)
            {

                var currentKeyBindSet = ArchInput.active.currentKeybindSet;
                foreach (var action in alternateActionsName)
                {
                    exitKeyUps.Add(currentKeyBindSet.KeyCodeFromName(action));
                
                }

                await KeyBindings.LetGoKeys(exitKeyDowns);
                await KeyBindings.LetGoKeys(exitKeyUps);
            }


            await Task.Yield();

            while (exitKeysActive)
            {
                if (!isChoosing) break;


                foreach (var key in exitKeyUps)
                {
                    if (Input.GetKeyUp(key))
                    {
                        exitKeysActive = false;
                    }

                    
                }

                foreach(var key in exitKeyDowns)
                {
                    if (Input.GetKeyDown(key))
                    {
                        exitKeysActive = false;
                    }
                }

                await Task.Yield();
            }

            exitKeysActive = false;


            async Task ExtraInputs()
            {

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
            }

            while (isChoosing)
            {
                await Task.Yield();
            }


            ClearOptions();

            OnContextActiveChange?.Invoke(this, true);
            pickedOption = -1;

            info.title.text = contextData.title;


            await CreateOptions(contextData.options);

            isChoosing = true;

            HandleArchInput();
            await HandleInput();

            isChoosing = false;


            var response = new ContextMenuResponse();

            if (pickedOption == -1)
            {
                response.stringValue = "";
                response.index = -1;
            }
            else
            {
                contextData.options[pickedOption].SelectOption();
                response.stringValue = contextData.options[pickedOption].text;
                response.index = pickedOption;
                
            }


            OnContextActiveChange?.Invoke(this, false);
            return response;
        }

        async Task CreateOptions(List<OptionData> options)
        {
            for (int i = 0; i < options.Count; i++)
            {
                var context = Instantiate(info.optionPrefab, info.options).GetComponent<ContextOption>();
                var text = options[i].text;
                context.SetOption(text, i);
            }

            layoutGroup.enabled = true;

            await Task.Delay(62);

            AdjustPosition();

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

        [Serializable]
        public class OptionData
        {
            public ContextMenu contextMenu;
            public string text;

            Action<OptionData> OnSelect;

            public OptionData(string text)
            {
                this.text = text;
            }

            public OptionData(string text, Action<OptionData> selectAction)
            {
                this.text = text;
                OnSelect += selectAction;
            }

            public OptionData(string text, Action selectAction)
            {
                this.text = text;
                OnSelect += (optionData) => {
                    selectAction();
                };
            }

            public void SelectOption()
            {
                OnSelect?.Invoke(this);
            }
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
        public List<ContextMenu.OptionData> options;

        
    }
}
