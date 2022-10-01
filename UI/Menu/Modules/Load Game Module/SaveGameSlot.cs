using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Architome
{
    public class SaveGameSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // Start is called before the first frame update

        GameLoadManager manager;
        public bool selected { get; private set; }

        bool clicked;
        public SaveGame saveGame { get; private set; }

        [Serializable]
        public struct Info
        {
            public TextMeshProUGUI saveName, lastSave, version;
            public Image border;
        }

        public Info info;


        public Action<SaveGameSlot> OnClick { get; set; }
        public Action<SaveGameSlot> OnDoubleClick { get; set; }

        public Action<SaveGameSlot> OnDestroySelf { get; set; }

        void Start()
        {
            manager = GetComponentInParent<GameLoadManager>();

        }

        public void DestroySelf()
        {
            OnDestroySelf?.Invoke(this);
            Destroy(gameObject);
        }

        public void SetStatus(bool selected)
        {
            info.border.enabled = selected;
        }

        public void SetSlot(SaveGame save)
        {
            saveGame = save;
            info.border.enabled = false;

            info.saveName.text = save.saveName;
            info.lastSave.text = save.timeString;
            info.version.text = $"Version {save.build}";
        }

        async public void SelectSave()
        {
            if (clicked)
            {
                OnDoubleClick?.Invoke(this);
                clicked = false;
                return;
            }

            clicked = true;

            OnClick?.Invoke(this);

            float doubleClickTimer = .5f;

            while (doubleClickTimer > 0)
            {
                doubleClickTimer -= Time.deltaTime;
                
                if (!clicked)
                {
                    return;
                }

                await Task.Yield();
            }


            clicked = false;

            return;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetHover(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHover(false);
        }


        public void SetHover(bool isHovering)
        {
            if (!isHovering)
            {
                if (manager.hoverSave == saveGame)
                {
                    manager.hoverSave = new();
                }
            }
            else
            {
                manager.hoverSave = saveGame;
            }
        }
    }
}
