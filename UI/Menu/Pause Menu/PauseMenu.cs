using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Architome.Enums;

namespace Architome
{
    public class PauseMenu : MonoBehaviour
    {
        public static PauseMenu active;

        public bool isActive;
        public bool selfEscape;


        bool activeCheck;

        public List<CanvasGroup> items;
        public CanvasGroup router;

        Dictionary<ArchScene, string> sceneTitles;

        public Action<PauseMenu> OnTryOpenPause { get; set; }
        public Action<PauseMenu, bool> OnActiveChange {get; set;}
        public Action<PauseMenu, List<bool>> OnCanOpenCheck {get; set;}
        public Action<PauseMenu, List<bool>> OnCanCloseCheck {get; set;}

        public bool pauseBlocked;

        private void OnValidate()
        {
            UpdateMenu();
        }

        private void Update()
        {
            if (!selfEscape) return;
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            PauseMenuBack();

        }

        private void Awake()
        {
            active = this;
            CreateTitles();

        }

        void CreateTitles()
        {
            sceneTitles = new()
            {
                { ArchScene.Tutorial, "Tutorial" },
                { ArchScene.PostDungeon, "Post Dungeon" },
                { ArchScene.Dungeon, "Dungeon" },
                { ArchScene.Menu, "Menu" },
                { ArchScene.DungeoneerMenu, "Guild Menu" }
            };
        }

        private void Start()
        {
        }

        bool ContextMenuActive()
        {
            var contextMenu = ContextMenu.current;
            if (contextMenu == null) return false;
            if (!contextMenu.isChoosing) return false;

            return true;
        }

        public void PauseMenuBack()
        {
            pauseBlocked = false;

            OnTryOpenPause?.Invoke(this);

            if (pauseBlocked) return;


            bool changed = false;
            foreach (var item in items)
            {
                if (item.alpha == 1)
                {
                    SetCanvas(item, false);
                    changed = true;
                }
            }

            if (changed)
            {
                SetCanvas(router, true);
                return;
            }

            ToggleMenu(); 
        }

        public void ToggleMenu()
        {
            isActive = !isActive;
            UpdateMenu();
        }

        public void SetMenu(bool active)
        {

            this.isActive = active;

            UpdateMenu();
        }
        

        bool CanOpen()
        {
            var checks = new List<bool>();

            OnCanOpenCheck?.Invoke(this, checks);

            foreach (var check in checks)
            {
                if (!check) return false;
            }
            return true;
        }
        

        void UpdateMenu()
        {
            if (router == null) return;

            if (activeCheck != isActive)
            {
                activeCheck = isActive;
                OnActiveChange?.Invoke(this, isActive);
            }

            SetCanvas(router, isActive);

        }

        void LoadScene(ArchScene scene)
        {
            ArchSceneManager.active.LoadScene(scene);
            //SceneManager.LoadScene(sceneName);
        }

        public void LoadGuild(bool safe)
        {
            if (safe)
            {
                LoadSceneSafe(ArchScene.DungeoneerMenu);
            }
            else
            {
                LoadScene(ArchScene.DungeoneerMenu);
            }
        }

        public void LoadMenu(bool safe)
        {
            if (safe)
            {
                LoadSceneSafe(ArchScene.Menu);
            }
            else
            {
                LoadScene(ArchScene.Menu);
            }
        }

        async void LoadSceneSafe(ArchScene scene)
        {
            var confirmed = await Confirmed();

            if (!confirmed) return;

            ArchSceneManager.active.LoadScene(scene);


            async Task<bool> Confirmed()
            {
                var promptHandler = PromptHandler.active;
                if (promptHandler == null) return true;

                var title = sceneTitles[scene];

                var question = Core.currentSave != null ? $"Are you sure you want to go to {title}? You may lose all your saved progress" : $"Are you sure you want to go to {title}?";

                bool confirm = false;

                var userChoice = await promptHandler.GeneralPrompt(new()
                {
                    title = title.ToString(),
                    question = question,
                    options = new() {
                        new("Confirm", (option) => { confirm = true; }),
                        new("Cancel") { isEscape = true }
                        //"Confirm",
                        //"Cancel"
                    },
                    blocksScreen = true
                });

                return confirm;
            }
        }

        public async void ExitApplication(bool safe = false)
        {
            if (safe)
            {
                var confirm = await Confirmed();
                if (!confirm) return;
            }

            Application.Quit();

            async Task<bool> Confirmed()
            {
                var promptHandler = PromptHandler.active;

                if (promptHandler == null) return true;

                var confirmed = false;

                var userChoice = await promptHandler.GeneralPrompt(new()
                {
                    title = "Quit Game",
                    question ="Are you sure you want to quit? The current progress might not be saved.",
                    options= new() {
                        new("Confirm", (option) => { confirmed = true; }),
                        new("Cancel"){ isEscape = true },
                    },
                    blocksScreen=true,
                });;

                return confirmed;

            }
        }


        public void SetCanvas(CanvasGroup canvas, bool val)
        {
            canvas.alpha = val ? 1 : 0;
            canvas.interactable = val;
            canvas.blocksRaycasts = val;
        }
    }
}
