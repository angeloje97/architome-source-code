using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    public class GameLoadManager : MonoBehaviour
    {
        public List<SaveGame> savedGames;
        public List<SaveGameSlot> slots;
        public SaveGame selectedSave;
        public SaveGame hoverSave;
        public MenuModule module;
        public bool selectedSaveExists;

        [Serializable]
        public struct Prefabs
        {
            public GameObject saveGameSlotTemplate;
        }

        [Serializable]
        public class Info
        {
            public Transform slotsParent;
            public List<ArchButton> saveInteractionButtons;
        }

        [SerializeField] Info info;
        [SerializeField] Prefabs prefabs;

        public Action<SaveGame> OnSelectSave;
        private void Start()
        {
            selectedSave = null;
            savedGames = Core.AllSaves();
            module = GetComponent<MenuModule>();
            CreateSlots();
            UpdateButtons();
        }
        private void Update()
        {
            if (!module.isActive) return;
            //HandleNullMouseOvers();
        }

        void HandleNullMouseOvers()
        {
            if (!Input.GetKeyUp(KeyCode.Mouse0)) return;
            if (hoverSave.saveId != 0) return;

            selectedSaveExists = false;
            selectedSave = new();
            UpdateButtons();
        }
        void CreateSlots()
        {
            if (info.slotsParent == null) return;
            if (prefabs.saveGameSlotTemplate == null) return;

            slots = new();

            foreach (var save in savedGames)
            {
                var newSlot = Instantiate(prefabs.saveGameSlotTemplate, info.slotsParent).GetComponent<SaveGameSlot>();

                newSlot.SetSlot(save);

                newSlot.OnClick += OnClickSave;
                newSlot.OnDoubleClick += OnDoubleClickSave;

                slots.Add(newSlot);
                
            }
        }
        public void HandleSelect(SaveGame save)
        {
            selectedSave = save;
            OnSelectSave?.Invoke(save);
            selectedSaveExists = true;
            UpdateSlots();
            UpdateButtons();
        }

        public void OnClickSave(SaveGameSlot slot)
        {
            HandleSelect(slot.saveGame);
        }

        public void OnDoubleClickSave(SaveGameSlot slot)
        {
            if (selectedSave.saveId <= 0) return;
            LoadGame();
        }
        public void UpdateButtons()
        {
            foreach (var save in slots)
            {
                var isSelected = selectedSaveExists && save.saveGame == selectedSave;

                if (save.info.border.enabled != isSelected)
                {
                    save.info.border.enabled = isSelected;
                }
            }

            foreach (var button in info.saveInteractionButtons)
            {
                button.SetButton(selectedSaveExists);
            }

        }
        public void UpdateSlots()
        {
            foreach (var slot in slots)
            {
                slot.SetStatus(slot.saveGame == selectedSave);
            }
        }
        public void LoadGame()
        {
            Core.SetSave(selectedSave);

            ArchSceneManager.active.LoadScene(selectedSave.currentSceneName);
            //SceneManager.LoadScene(selectedSave.currentSceneName);
        }
        

    }
}
