using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Architome
{
    public class PromptInfo : ModuleInfo
    {
        // Start is called before the first frame update

        public int choicePicked;
        public string userInput;
        public bool optionEnded;
        public int amount;



        public Action<PromptInfo> OnPickChoice;
        public UnityEvent OnInvalidInput;
        public UnityEvent OnValidInput;

        public PromptChoiceData choiceData;
        public PromptInfoData promptData;


        [Serializable] public struct Info
        {
            public TextMeshProUGUI title;
            public TextMeshProUGUI question;
            public TextMeshProUGUI[] options;
            public Image promptIcon;
            public GameObject screenBlocker;

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
        }

        public Info info;
        
        public SliderInfo sliderInfo;

        public InputFieldInfo inputFieldInfo;

        bool validInput;
        public void EndOptions()
        {

            optionEnded = true;
            PlaySound(false);
            SetActive(false);
            ArchAction.Delay(() => { Destroy(gameObject); }, 1f);
        }
        void Start()
        {
            PlaySound(true);
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
            choiceData.optionString = info.options[option].text;

            OnPickChoice?.Invoke(this);
        }

        public void SetPrompt(PromptInfoData promptData)
        {
            this.promptData = promptData;

            info.title.text = promptData.title;
            info.question.text = promptData.question;
            info.options[0].text = promptData.option1;
            info.options[1].text = promptData.option2;

            info.screenBlocker.SetActive(promptData.blocksScreen);
            forceActive = promptData.forcePick;

            HandleSlider();
            HandleInputField();

            if (promptData.icon)
            {
                info.promptIcon.sprite = promptData.icon;
            }
        }

        public void UpdateInputField(TMP_InputField input)
        {
            var maxInputLength = promptData.maxInputLength != 0 ? promptData.maxInputLength : 99;
            bool validInput = input.text.Length > 0 && input.text.Length < maxInputLength;
            choiceData.userInput = input.text;


            if (this.validInput != validInput)
            {
                this.validInput = validInput;

                if (validInput)
                {
                    OnValidInput?.Invoke();
                }
                else
                {
                    OnInvalidInput?.Invoke();
                }
            }

        }

        void HandleInputField()
        {
            if (!inputFieldInfo.enable) return;

            UpdateInputField(inputFieldInfo.inputField);
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

        // Update is called once per frame
        void Update()
        {

        }


    }
    public struct PromptInfoData
    {
        public string title;
        public string question;
        public string option1;
        public string option2;
        public bool blocksScreen;
        public bool forcePick;

        //Slider
        public int amountMin;
        public int amountMax;

        //Input Field
        public int maxInputLength;

        public Sprite icon;

    }
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