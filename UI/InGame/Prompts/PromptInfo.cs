using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Architome
{
    [RequireComponent(typeof(SizeFitter))]
    public class PromptInfo : ModuleInfo
    {
        // Start is called before the first frame update
        [Header("PromptInfo Properties")]
        public string userInput;
        public bool optionEnded { get; set; }
        public int amount;
        public bool destroyed { get; private set; }

        //public List<ArchButton> buttonOptions;
        //public List<string> choices;
        public Action<PromptInfo> OnPickChoice { get; set; }
        public Action<PromptInfo> OnEndOptions { get; set; }

        
        public PromptChoiceData choiceData;
        public PromptInfoData promptData;

        [Serializable] public struct Info
        {
            public TextMeshProUGUI title;
            public TextMeshProUGUI question;
            public Image promptIcon;
            public Transform iconFrame;
            public GameObject screenBlocker;
            public Transform choicesParents;
            public List<Transform> buttonAccents;
        }

        [Serializable]
        public struct PromptPrefabs
        {
            public ArchButton button;
        }
        [Serializable] public struct SliderInfo
        {
            public bool enable;
            public Slider slider;
            public TextMeshProUGUI sliderTMP;
        }
        [Serializable] public struct InputFieldInfo
        {
            public bool enable;
            public TMP_InputField inputField;
            public bool validInput;
            public bool start;
            public UnityEvent OnInvalidInput;
            public UnityEvent OnValidInput;
        }

        public Info info;
        
        public SliderInfo sliderInfo;

        public InputFieldInfo input;

        public PromptPrefabs promptPrefabs;


        public void ClosePrompt()
        {
            if (destroyed) return;
            PlaySound(false);
            SetActive(false);
            OnEndOptions?.Invoke(this);
            ArchAction.Delay(() => {
                if (this == null) return;
                Destroy(gameObject); 
            }, 1f);
            destroyed = true;
        }
        void Start()
        {
            PlaySound(true);
            HandlePauseMenu();
            HandleIGGUI();
        }

        void HandlePauseMenu()
        {
            var pauseMenu = PauseMenu.active;
            if (!pauseMenu) return;

            pauseMenu.OnTryOpenPause += HandleTryPause;

            OnEndOptions += (PromptInfo prompt) => {
                pauseMenu.OnTryOpenPause -= HandleTryPause;
            };

            void HandleTryPause(PauseMenu menu)
            {
                menu.pauseBlocked = true;
            }
        }

        void HandleIGGUI()
        {
            var gui = IGGUIInfo.active;
            if (gui == null) return;

            gui.OnClosingModulesCheck += OnCloseCheck;

            OnEndOptions += (info) => {
                gui.OnClosingModulesCheck -= OnCloseCheck;
            };

            void OnCloseCheck(IGGUIInfo info, List<bool> checks)
            {
                checks.Add(false);
            }
        }

        public void PlaySound(bool open)
        {
            try
            {
                var inGameUI = GetComponentInParent<IGGUIInfo>();
                var audioManager = inGameUI.GetComponent<AudioManager>();

                var audioClip = open ? openSound : closeSound;
                if (audioClip != null)
                {
                    audioManager.PlaySound(audioClip);
                }

            }
            catch { }
        }

        public void PickOption(OptionData option)
        {
            choiceData.optionPicked = option;
            optionEnded = true;
            OnPickChoice?.Invoke(this);

        }

        public void SetPrompt(PromptInfoData promptData, bool adjustSize = true)
        {
            this.promptData = promptData;
            choiceData = new();
            optionEnded = false;


            info.title.text = promptData.title;
            info.question.text = promptData.question;

            info.screenBlocker.SetActive(promptData.blocksScreen);
            forceActive = promptData.forcePick;


            if (this.promptData.icon)
            {
                info.promptIcon.sprite = this.promptData.icon;
                info.iconFrame.gameObject.SetActive(true);
            }

            this.promptData.OnStart?.Invoke(this);

            HandleSlider();
            HandleOptions();
            HandleInputField();

            if(adjustSize) UpdateSize();

            
        }

        async void UpdateSize()
        {
            var sizeFitters = GetComponentsInChildren<ContentSizeFitter>();
            var canvasGroup = GetComponent<CanvasGroup>();

            ArchUI.SetCanvas(canvasGroup, false);
            var sizeFitter = GetComponent<SizeFitter>();
            sizeFitter.AdjustToSize();
            UpdateSizeFitters();

            bool halfWay = false;

            var adjustSizeIterations = 10;

            for (int i = 0; i < adjustSizeIterations; i++)
            {

                await Task.Yield();

                if (halfWay) continue;

                if (i >= adjustSizeIterations / 2)
                {
                    halfWay = true;
                    sizeFitter.AdjustToSize();
                    UpdateSizeFitters();
                }
            }

            sizeFitter.AdjustToSize();
            UpdateSizeFitters();

            ArchUI.SetCanvas(canvasGroup, true);
            
            void UpdateSizeFitters()
            {
                foreach(var fitter in sizeFitters)
                {
                    sizeFitter.gameObject.SetActive(false);
                    sizeFitter.gameObject.SetActive(true);
                }
            }
        }

        void HandleOptions()
        {
            var valid = true;
            if (promptData.options == null) valid = false;
            if (info.choicesParents == null) valid = false;
            var button = promptPrefabs.button;
            if (button == null) valid = false;

            if (!valid)
            {
                var accents = info.buttonAccents;

                if (accents != null)
                {
                    foreach(var accent in accents)
                    {
                        accent.gameObject.SetActive(false);
                    }
                }

                return;
            }

            var hasEscape = false;
            var hasAutoPick = false;
            
            for(int i = 0; i < promptData.options.Count; i++)
            {
                var option = promptData.options[i];
                if (hasEscape) option.isEscape = false;
                if (hasAutoPick) option.autoPick = false;

                if (option.isEscape) hasEscape = true;
                if (option.autoPick) hasAutoPick = true;
                Debugger.UI(2337, $"Option {i +1} is {option.text}");

                option.HandlePrompt(this);
                if (promptData.autoClose)
                {
                    option.OnClose += (optionData) => {

                        ArchAction.Yield(ClosePrompt);
                    };
                }

            }

        }

        public void RemoveOptions()
        {
            if (info.choicesParents == null) return;

            var buttons = info.choicesParents.GetComponentsInChildren<ArchButton>();
            foreach(var button in buttons)
            {
                Destroy(button.gameObject);
            }
        }
        public void UpdateInputField(TMP_InputField input)
        {
            promptData.IsValidInput ??= DefaultPredicate;

            bool validInput = promptData.IsValidInput(input.text);
            choiceData.userInput = input.text;

            Debugger.Environment(5491, $"Input text is valid: {validInput}");
            promptData.OnInputFieldChange?.Invoke(this, input);

            if (!this.input.start)
            {
                this.input.start = true;
                this.input.validInput = !validInput;
            }


            if (this.input.validInput != validInput)
            {
                this.input.validInput = validInput;

                if (validInput)
                {
                    this.input.OnValidInput?.Invoke();
                }
                else
                {
                    this.input.OnInvalidInput?.Invoke();
                }
            }

            UpdateButtons();

            void UpdateButtons()
            {
                foreach(var option in promptData.options)
                {
                    option.HandleInput(validInput);
                }
            }

            bool DefaultPredicate(string inputText)
            {
                var maxInputLength = promptData.maxInputLength != 0 ? promptData.maxInputLength : 99;
                return inputText.Length > 0 && inputText.Length < maxInputLength;
            }
        }
        void HandleInputField()
        {
            if (!input.enable) return;
            

            UpdateInputField(input.inputField);
            
        }
        public void HandleSlider()
        {
            if (!sliderInfo.enable) return;

            sliderInfo.slider.minValue = promptData.amountMin;
            sliderInfo.slider.maxValue = promptData.amountMax;
            sliderInfo.slider.wholeNumbers = true;
            sliderInfo.slider.value = promptData.amountMin;


        }
        public async Task<PromptChoiceData> UserChoice()
        {
            await World.UpdateAction((float time) => {
                if (optionEnded) return false;
                if (destroyed) return false;
                if (this == null) return false;
                if (!isActive) return false;
                return true;
            });

            return choiceData;
        }

        public void UpdateSlider()
        {
            if (sliderInfo.enable == false) return;

            sliderInfo.sliderTMP.text = sliderInfo.slider.value.ToString();

            choiceData.amount = (int) sliderInfo.slider.value;
            choiceData.amountF = sliderInfo.slider.value;
            
        }


    }
    [Serializable]
    public class PromptInfoData
    {
        public string title;
        public string question;
        public Predicate<string> IsValidInput;
        public List<OptionData> options { get; set; }
        public bool blocksScreen { get; set; }
        public bool forcePick;
        public bool autoClose = true;
        public Action<PromptInfo> OnStart;
        public bool handleClose { get; set; }
        //Slider
        public int amountMin;
        public int amountMax;

        //Input Field
        public int maxInputLength;
        public Action<PromptInfo, TMP_InputField> OnInputFieldChange;

        public Sprite icon;

        
    }
    [Serializable]
    public class PromptChoiceData
    {
        public OptionData optionPicked;

        //Slider
        public int amount { get; set; }
        public float amountF { get; set; }


        //Input field
        public string userInput { get; set; }

    }

    [Serializable]
    public class OptionData
    {
        PromptInfo promptInfo;
        public string text;
        public bool affectedByInvalidInput;
        public bool preventClosePrompt;
        public bool isEscape { get; set; }
        public bool autoPick { get; set; }
        public int autoPickTimer { get; set; }
        public int timer;

        public ArchButton button { get; private set;  }

        public Action<OptionData> OnChoose { get; set; }
        public Action<OptionData> OnClose { get; set; }


        public OptionData(string text, Action<OptionData> action)
        {
            this.text = text;
            OnChoose += action;
        }

        public OptionData(string text)
        {
            this.text = text;
        }

        bool active
        {
            get
            {
                return button.interactable;
            }
        }


        public void HandlePrompt(PromptInfo prompt)
        {
            promptInfo = prompt;
            var parent = prompt.info.choicesParents;
            var button = prompt.promptPrefabs.button;

            if (parent == null || button == null) return;

            this.button = UnityEngine.Object.Instantiate(button.gameObject, parent).GetComponent<ArchButton>();

            this.button.SetButton(text, ChooseOption);

            HandleButtonTimer();
            HandleEscape();
            HandleAutoPick();

        }

        async void HandleAutoPick()
        {
            if (!autoPick) return;
            var autoTimer = autoPickTimer;

            var finalText = $"{text} ({autoTimer}s)";
            button.SetName(finalText);

            while(autoTimer > 0)
            {
                await Task.Delay(1000);
                autoTimer--;
                if (!promptInfo) return;
                button.SetName($"{text} ({autoTimer}s)");
            }

            if (!promptInfo) return;
            ChooseOption();
        }

        async void HandleEscape()
        {
            if (!isEscape) return;
            while (!promptInfo.optionEnded)
            {
                if (active)
                {
                    if (Input.GetKeyUp(KeyCode.Escape))
                    {
                        ChooseOption();
                        break;
                    }

                }
                await Task.Yield();
            }


        }

        public void SetAvailable(bool val)
        {
            button.SetButton(val);
        }

        async void HandleButtonTimer()
        {
            if (this.timer == 0) return;
            var timer = this.timer;

            SetAvailable(false);

            for (int i = timer; i >= 0; i--)
            {
                button.SetName($"{text}({i})");

                await Task.Delay(1000);
            }

            button.SetName(text);

            SetAvailable(true);
        }

        public void ChooseOption()
        {
            OnChoose?.Invoke(this);
            promptInfo.PickOption(this);
            if (!preventClosePrompt)
            {
                ClosePrompt();
            }
        }

        public void ClosePrompt()
        {
            OnClose?.Invoke(this);
        }

        public void HandleInput(bool val)
        {
            if (!affectedByInvalidInput) return;
            
            this.button.SetButton(val);
        }
    }


}