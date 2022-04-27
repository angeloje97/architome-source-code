using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

namespace Architome
{
    public class ChestModule : MonoBehaviour
    {
        ArchChest chest;
        ModuleInfo module;

        [Serializable]
        struct Info
        {
            public TextMeshProUGUI moduleTitle;
            public Image[] stars;
        }

        [SerializeField] Info info;

        
        public void SetChest(ArchChest chest)
        {
            this.chest = chest;

            this.chest.events.OnClose += OnClose;

            WorldModuleCore.active.OnChestOpen += OnAnotherChestOpen;

            module = GetComponent<ModuleInfo>();

            module.OnActiveChange += OnActiveChange;

            module.SetActive(true, true);
        }

        public void OnClose(ArchChest chest)
        {
            module.SetActive(false, true);

            this.chest.events.OnClose -= OnClose;
            WorldModuleCore.active.OnChestOpen -= OnAnotherChestOpen;

            ArchAction.Delay(() => {
                Destroy(gameObject);
            }, 1f);
        }
        
        void OnActiveChange(bool isActive)
        {
            if (isActive) return;

            chest.GetComponent<WorkInfo>().RemoveAllLingers();
        }

        void OnAnotherChestOpen(ArchChest chest)
        {
            OnClose(this.chest);
            
        }
    }

}