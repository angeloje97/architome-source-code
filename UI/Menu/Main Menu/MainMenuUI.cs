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

        public Action<MenuModule> OnOpenMenu;

        public void OpenModule(GameObject menuModule)
        {
            if (menuItems == null) return;
            var module = menuModule.GetComponent<MenuModule>();
            if (module == null) return;

            module.Show(true);
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
            foreach (var menuModule in GetComponentsInChildren<MenuModule>())
            {
                menuModule.Show(false);
            }
        }
    }
}
