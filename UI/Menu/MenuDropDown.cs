using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class MenuDropDown : MonoBehaviour
    {
        [System.Serializable]
        public struct Prefabs
        {
            public GameObject settings;
        }

        public Prefabs prefabs;

        async public void QuitApplication()
        {
            bool quit = true;

            if (PromptHandler.active)
            {
                int choice = await PromptHandler.active.GeneralPrompt(new() 
                {
                    option1 = "Quit",
                    option2 = "Cancel",
                    question = "Are you sure you want to quit?",
                    blocksScreen = true,
                });

                if (choice == 1)
                {
                    quit = false;
                }
            }

            if (quit)
            {
                Application.Quit();
            }

        }

        public void OpenSettings(Transform parent)
        {
            if (prefabs.settings = null) return;

        }
    }
}
