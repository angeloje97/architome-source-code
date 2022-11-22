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

            if (mainMenuUI)
            {
                mainMenuUI.OnCanChangeModuleCheck += HandleCanChangeModuleCheck;
            }



        }

        public virtual async void HandleCanChangeModuleCheck(MainMenuUI mainMenu, List<bool> checks)
        {
            if (!dirty) return;
            checks.Add(false);
            var desiredModule = mainMenu.desiredModule;

            var confirmLeave = await ConfirmLeave();

            if (confirmLeave)
            {
                HandleLeaveDirty();
                dirty = false;
                mainMenu.OpenModule(desiredModule.gameObject);
            }

        }

        async Task<bool> ConfirmLeave()
        {
            var promptHandler = PromptHandler.active;
            if (promptHandler == null) return true;

            var userChoice = await promptHandler.GeneralPrompt(PromptData());

            if(userChoice.optionString == "Cancel")
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
                dirty = false;
                HandleLeaveDirty();
                return;
            }
            checks.Add(false);

            var desiredIndex = navBar.ActiveIndex();
            navBar.UpdateFromIndex(navBar.previousIndex);

            var confirmLeave = await ConfirmLeave();

            if(!confirmLeave)
            {
                return;
            }

            dirty = false;
            navBar.UpdateFromIndex(desiredIndex);
            HandleLeaveDirty();
            return;

        }

        public virtual void HandleLeaveDirty()
        {

        }


        public virtual PromptInfoData PromptData()
        {
            return new()
            {
                title = "Settings",
                question = "Are you sure you want to continue without applying?",
                options = new List<string>() { 
                    "Leave Settings",
                    "Cancel"
                },
                escapeOption = "Cancel",
                blocksScreen = true,
            };
        }

    }
}