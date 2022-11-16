using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Threading.Tasks;

namespace Architome
{
    [RequireComponent(typeof(SizeFitter))]
    public class PromptInfo : ModuleInfo
    {
        // Start is called before the first frame update
        [Header("PromptInfo Properties")]
        public int choicePicked;
        public string userInput;
        public bool optionEnded;
        public int amount;
        public List<ArchButton> buttonOptions;
        public List<string> choices;
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


        public void EndOptions()
        {

            optionEnded = true;
            PlaySound(false);
            SetActive(false);
            OnEndOptions?.Invoke(this);
            ArchAction.Delay(() => { Destroy(gameObject); }, 1f);
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




        private void Update()
        {
            HandleEscape();

            void HandleEscape()
            {
                if (!Input.GetKeyUp(KeyCode.Escape)) return;
                if (promptData.forcePick) return;
                if(promptData.escapeOption != "")
                {
                    for (int i = 0; i < buttonOptions.Count; i++)
                    {
                        if (choices[i] != promptData.escapeOption) continue;
                        PickOption(i);
                    }
                }


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

        public void PickOption(int option)
        {
            choicePicked = option;
            choiceData.optionPicked = option;
            if (promptData.options != null)
            {
                choiceData.optionString = promptData.options[option];
            }

            OnPickChoice?.Invoke(this);

            ArchAction.Yield(() => EndOptions());
        }

        public void SetPrompt(PromptInfoData promptData)
        {
            this.promptData = promptData;

            info.title.text = promptData.title;
            info.question.text = promptData.question;
            //info.options[0].text = promptData.option1;
            //info.options[1].text = promptData.option2;

            info.screenBlocker.SetActive(promptData.blocksScreen);
            forceActive = promptData.forcePick;

            HandleSlider();
            HandleButtons();
            HandleButtonTimers();
            HandleInputField();

            UpdateSize();

            if (promptData.icon)
            {
                info.promptIcon.sprite = promptData.icon;
            }
        }

        void HandleButtonTimers()
        {
            if (promptData.options == null) return;
            if (promptData.optionsAffectedByTimer == null) return;

            foreach(var optionIndex in promptData.optionsAffectedByTimer)
            {
                HandleTimer(buttonOptions[optionIndex]);
            }

            async static void HandleTimer(ArchButton button)
            {
                button.SetButton(false);

                var originalText = button.buttonName.text;

                var seconds = 3;
                button.SetName($"{originalText} in {seconds}s");
                while(seconds > 0)
                {
                    await Task.Delay(1000);
                    seconds -= 1;
                    button.SetName($"{originalText} in {seconds}s");
                }
                button.SetName(originalText);
                button.SetButton(true);
            }

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

        void HandleButtons()
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

            buttonOptions = new List<ArchButton>();
            choices = new();

            for (int i = 0; i < promptData.options.Count; i++)
            {
                var option = promptData.options[i];
                var newButton = Instantiate(button, info.choicesParents).GetComponent<ArchButton>();

                buttonOptions.Add(newButton);
                choices.Add(option);

                newButton.SetName(option);

                newButton.OnClick += delegate (ArchButton button)
                {
                    PickOption(buttonOptions.IndexOf(button));
                };
            }
        }

        public void UpdateInputField(TMP_InputField input)
        {
            promptData.IsValidInput ??= DefaultPredicate;

            bool validInput = promptData.IsValidInput(input.text);
            choiceData.userInput = input.text;

            Debugger.Environment(5491, $"Input text is valid: {validInput}");

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
                if (buttonOptions == null) return;
                var affectedByInvalid = promptData.optionsAffectedByInvalidInput;
                if (affectedByInvalid == null) return;

                foreach(var index in affectedByInvalid)
                {
                    buttonOptions[index].SetButton(validInput);
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

        public void UpdateSlider()
        {
            if (sliderInfo.enable == false) return;

            sliderInfo.sliderTMP.text = sliderInfo.slider.value.ToString();

            choiceData.amount = (int) sliderInfo.slider.value;
            choiceData.amountF = sliderInfo.slider.value;
            
        }


    }
    public struct PromptInfoData
    {
        public string title;
        public string question;
        public string escapeOption;
        public List<int> optionsAffectedByInvalidInput;
        public List<int> optionsAffectedByTimer;
        public Predicate<string> IsValidInput;
        public List<string> options { get; set; }
        public bool blocksScreen;
        public bool forcePick;

        //Slider
        public int amountMin;
        public int amountMax;

        //Input Field
        public int maxInputLength;

        public Sprite icon;

        [Serializable]
        public struct OptionsData
        {
            public string reference;
            public bool affectedByInvalidInput;
        }

    }
    [Serializable]
    public struct PromptChoiceData
    {
        public int optionPicked;
        public string optionString;

        //Slider
        public int amount;
        public float amountF;


        //Input field
        public string userInput;

        public static PromptChoiceData defaultPrompt
        {
            get
            {
                return new()
                {
                    optionPicked = -1,
                    optionString = "",
                };
            }
        }
    }
}