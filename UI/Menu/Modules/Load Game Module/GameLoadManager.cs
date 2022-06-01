using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

namespace Architome
{
    public class GameLoadManager : MonoBehaviour
    {
        public List<SaveGame> savedGames;
        public List<SaveGameSlot> slots;
        public SaveGame selectedSave;
        public SaveGame hoverSave;



        [Serializable]
        public struct Prefabs
        {
            public GameObject saveGameSlotTemplate;
        }

        [Serializable]
        public struct Info
        {
            public Transform slotsParent;
            public ArchButton loadSave, renameSave, deleteSave;
        }

        [SerializeField] Info info;
        [SerializeField] Prefabs prefabs;

        public Action<SaveGame> OnSelectSave;

        private void Start()
        {
            savedGames = Core.AllSaves();
            CreateSlots();
            UpdateButtons();
        }

        private void Update()
        {
            HandleNullMouseOvers();
        }

        void HandleNullMouseOvers()
        {
            if (hoverSave != null) return;
            if (!Input.GetKeyDown(KeyCode.Mouse0)) return;

            selectedSave = null;
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

                slots.Add(newSlot);
            }
        }

        public void HandleSelect(SaveGame save)
        {
            selectedSave = save;
            OnSelectSave?.Invoke(save);

            UpdateSlots();
        }



        public void UpdateButtons()
        {
            bool selectedSaveExists = selectedSave != null;

            foreach (var save in slots)
            {
                if (!selectedSaveExists) break;
                var isSelected = save.saveGame == selectedSave;

                if (save.info.border.enabled != isSelected)
                {
                    save.info.border.enabled = isSelected;
                }
            }

            foreach (var field in info.GetType().GetFields())
            {
                if (field.FieldType != typeof(ArchButton)) continue;
                var archButton = (ArchButton) field.GetValue(info);

                archButton.SetButton(selectedSaveExists);
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
            SceneManager.LoadScene(selectedSave.currentSceneName);
        }
        

    }
}
