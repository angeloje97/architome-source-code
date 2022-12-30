using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;

namespace Architome
{
    [RequireComponent(typeof(AudioManager))]
    public class MainMenuUI : MonoBehaviour
    {
        public static MainMenuUI active;
        public Transform menuItems;
        public Transform defaultCameraPosition;
        AudioManager audioManager;
        ArchSceneManager sceneManager;

        public Action<MenuModule> OnOpenMenu;

        public Action<MainMenuUI, List<bool>> OnCanChangeModuleCheck { get; set; }
        public Action<MainMenuUI, List<bool>> OnEscapeIsFree { get; set; }

        public GameObject desiredModule { get; set; }

        bool CanChangeModules()
        {
            var checks = new List<bool>();

            OnCanChangeModuleCheck?.Invoke(this, checks);

            foreach (var check in checks)
            {
                if (!check) return false;
            }

            return true;
        }


        public void OpenModule(GameObject menuModule)
        {
            desiredModule = menuModule;
            if (!CanChangeModules()) return;
            if (menuItems == null) return;

            CloseAllMenuModules();

            if (menuModule == null) return;
            var module = menuModule.GetComponent<MenuModule>();
            if (module == null) return;
            module.Show(true);
            module.transform.SetAsLastSibling();
        }

        public bool EscapeIsFree()
        {
            var checks = new List<bool>();

            OnEscapeIsFree?.Invoke(this, checks);

            foreach(var check in checks)
            {
                if (!check) return false;
            }

            return true;
        }

        public void Awake()
        {
            active = this;
            audioManager = GetComponent<AudioManager>();
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (!EscapeIsFree()) return;
                desiredModule = null;
                OpenModule(null);
            }
        }

        public void PlayAudioClip(AudioClip audio)
        {
            if (audioManager == null) return;

            audioManager.PlaySound(audio);
        }

        public void CloseAllMenuModules()
        {
            foreach (var menuModule in menuItems.GetComponentsInChildren<MenuModule>())
            {
                menuModule.Show(false);
            }
        }

        public void LoadTutorial(int tutorialNum)
        {
            ArchSceneManager.active.LoadScene(ArchScene.Tutorial, tutorialNum);
        }
    }
}
