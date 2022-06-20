using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

namespace Architome
{
    public class PromptInfo : ModuleInfo
    {
        // Start is called before the first frame update

        public int choicePicked;
        public string userInput;
        public bool optionEnded;


        [Serializable]
        public struct Info
        {
            public TextMeshProUGUI title;
            public TextMeshProUGUI question;
            public TextMeshProUGUI[] options;
            public Image promptIcon;
            public GameObject screenBlocker;
            public TMP_InputField inputField;
        }

        public Info info;

        public void EndOptions()
        {
            if (info.inputField)
            {
                userInput = info.inputField.text;
            }

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

            
        }

        public void OnInputChange(TMP_InputField input)
        {
            bool validInput = input.text.Length > 0;
        }

        public void SetPrompt(PromptInfoData promptData)
        {
            

            info.title.text = promptData.title;
            info.question.text = promptData.question;
            info.options[0].text = promptData.option1;
            info.options[1].text = promptData.option2;

            if (promptData.blocksScreen)
            {
                info.screenBlocker.SetActive(true);
            }

            if (promptData.forcePick)
            {
                forceActive = true;
            }

            if (promptData.icon)
            {
                info.promptIcon.sprite = promptData.icon;
            }
            

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
        public Sprite icon;

    }
}