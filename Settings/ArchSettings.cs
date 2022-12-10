using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

namespace Architome.Settings
{
    public class ArchSettings : MonoBehaviour
    {
        [Header("States")]
        public bool dirty;
        public List<NavBar> reactiveNavBars;

        public Action OnLeaveDirty;


        bool hasCanvasController;
        bool chosenApply;

        public void HandleDirtyConflicts()
        {
            if (reactiveNavBars != null)
            {
                foreach(var navBar in reactiveNavBars)
                {
                    navBar.OnCanNavigateChange += (NavBar navBar, List<bool> checks) => {
                        HandleDirty(navBar, checks);
                    };
                }

            }

            var mainMenuUI = MainMenuUI.active;
            var pauseMenu = PauseMenu.active;
            var canvasController = GetComponentInParent<CanvasController>();


            if (mainMenuUI)
            {
                mainMenuUI.OnCanChangeModuleCheck += HandleCanChangeModuleCheck;
            }

            if (pauseMenu)
            {
                pauseMenu.OnTryOpenPause += HandleTryOpenPause;
            }

            if (canvasController)
            {
                canvasController.OnCanCloseCheck += HandleCloseCanvasController;
                hasCanvasController = true;
            }

        }

        async void HandleCloseCanvasController(CanvasController controller)
        {
            if (!dirty) return;
            controller.checks.Add(false);
            controller.haltChange = true;
            var confirmLeave = await ConfirmLeave("Canvas Controller");
            controller.haltChange = false;
            Debugger.UI(7691, $"Confirm Leave {confirmLeave}");
            if (!confirmLeave) return;

            if (dirty) HandleLeaveDirty();
            
            dirty = false;
            controller.SetCanvas(false);


        }

        void HandleTryOpenPause(PauseMenu pauseMenu)
        {

        }

        public virtual async void HandleCanChangeModuleCheck(MainMenuUI mainMenu, List<bool> checks)
        {
            if (!dirty) return;
            checks.Add(false);
            var desiredModule = mainMenu.desiredModule;

            var confirmLeave = await ConfirmLeave("Module");

            if (confirmLeave)
            {
                if(dirty) HandleLeaveDirty();
                dirty = false;
                mainMenu.OpenModule(desiredModule.gameObject);
            }

        }

        async Task<bool> ConfirmLeave(string sender)
        {
            Debugger.UI(2338, $"Sender for confirm leave prompt is {sender}");
            var promptHandler = PromptHandler.active;
            if (promptHandler == null) return true;

            var userChoice = await promptHandler.GeneralPrompt(PromptData());

            Debugger.UI(2335, $"The user picked {userChoice.optionPicked.text}");

            if(userChoice.optionPicked.text == "Cancel")
            {
                return false;
            }

            return true;
        }

        public virtual async void HandleDirty(NavBar navBar, List<bool> checks)
        {
            if (!dirty) return;
            var promptHandler = PromptHandler.active;

            if (promptHandler == null)
            {
                if(dirty)HandleLeaveDirty();
                dirty = false;
                return;
            }

            navBar.stallNavigation = true;
            //checks.Add(false);

            //var desiredIndex = navBar.ActiveIndex();
            //navBar.UpdateFromIndex(navBar.previousIndex);

            var confirmLeave = await ConfirmLeave("Nav Bar");

            

            if (confirmLeave)
            {
                if(dirty) HandleLeaveDirty();
                dirty = false;

            }
            else
            {
                checks.Add(false);
            }

            navBar.stallNavigation = false;

        }

        public virtual void HandleLeaveDirty()
        {

        }


        public virtual PromptInfoData PromptData()
        {
            chosenApply = false;
            return new()
            {
                title = "Settings",
                question = "Are you sure you want to continue without applying?",
                options = new() {
                    new("Apply Changes", (OptionData data) => {
                        chosenApply = true;
                        HandleChooseApply(); 
                    }),
                    new("Discard Changes"),
                    new("Cancel") {isEscape = true}
                },
                blocksScreen = true,
            };
        }

        public virtual void HandleChooseApply()
        {

        }

    }
}
