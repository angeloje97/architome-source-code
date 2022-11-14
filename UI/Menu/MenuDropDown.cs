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
                var choice = await PromptHandler.active.GeneralPrompt(new() 
                {
                    title = "Quit Game",
                    question = "Are you sure you want to quit?",
                    options = new() { "Quit", "Cancel" },
                    blocksScreen = true,
                });

                if (choice.optionPicked == 0)
                {
                    Application.Quit();
                }
            }

        }

        public void OpenSettings(Transform parent)
        {
            if (prefabs.settings = null) return;

        }
    }
}
