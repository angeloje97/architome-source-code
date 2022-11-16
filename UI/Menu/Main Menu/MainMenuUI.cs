using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

        public Action<MainMenuUI, List<bool>> OnCanChangeModuleCheck;

        public MenuModule desiredModule;

        bool CanOpenModule()
        {
            var checks = new List<bool>();

            OnCanChangeModuleCheck?.Invoke(this, checks);

            foreach(var check in checks)
            {
                if (!check) return false;
            }

            return true;
        }

        public void OpenModule(GameObject menuModule)
        {
            var module = menuModule.GetComponent<MenuModule>();
            desiredModule = module;
            if (!CanOpenModule()) return;
            if (menuItems == null) return;
            if (module == null) return;

            CloseAllMenuModules();

            module.Show(true);
            module.transform.SetAsLastSibling();
        }

        public void Awake()
        {
            active = this;
            audioManager = GetComponent<AudioManager>();
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
    }
}
