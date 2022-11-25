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

            if (PromptHandler.active)
            {
                var choice = await PromptHandler.active.GeneralPrompt(new() 
                {
                    title = "Quit Game",
                    question = "Are you sure you want to quit?",
                    options = new()
                    {
                        new("Quit", (option) => HandleQuit()),
                        new("Cancel") {isEscape = true}
                    },
                    blocksScreen = true,
                });

            }

            void HandleQuit()
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
