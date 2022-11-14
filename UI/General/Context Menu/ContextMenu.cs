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
        bool isChoosingCheck;


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

            var keyDownTimers = .25f;

            foreach (var action in alternateActionsName)
            {
                exitKeyUps.Add(keyBindData.keyBinds[action]);
            }

            await KeyBindings.LetGoKeys(exitKeyDowns);
            await KeyBindings.LetGoKeys(exitKeyUps);


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

                foreach (var key in exitKeyDowns)
                {
                    if (keyDownTimers > 0)
                    {
                        keyDownTimers -= Time.deltaTime;
                    }
                    else if (Input.GetKeyDown(key))
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

            OnContextActiveChange?.Invoke(this, true);
            pickedOption = -1;

            info.title.text = contextData.title;


            await CreateOptions(contextData.options);

            isChoosing = true;

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
                response.stringValue = contextData.options[pickedOption];
                response.index = pickedOption;
            }


            OnContextActiveChange?.Invoke(this, false);
            return response;
        }

        async Task CreateOptions(List<string> options)
        {
            for (int i = 0; i < options.Count; i++)
            {
                var context = Instantiate(info.optionPrefab, info.options).GetComponent<ContextOption>();

                context.SetOption(options[i], i);
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
