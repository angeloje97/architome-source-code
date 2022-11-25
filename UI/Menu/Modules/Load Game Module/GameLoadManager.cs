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
        public SaveGameSlot selectedSlot;
        public MenuModule module;
        public bool selectedSaveExists { get; set; }

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

        public Action<SaveGame> OnSelectSave { get; set; }
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

                newSlot.OnDestroySelf += delegate (SaveGameSlot slot)
                {
                    slot.OnClick -= OnClickSave;
                    slot.OnDoubleClick -= OnDoubleClickSave;
                };

                slots.Add(newSlot);
                
            }
        }
        public void HandleSelect(SaveGameSlot saveSlot)
        {
            var save = saveSlot.saveGame;
            selectedSlot = saveSlot;
            selectedSave = save;
            OnSelectSave?.Invoke(save);
            selectedSaveExists = true;
            UpdateSlots();
            UpdateButtons();
        }

        public async void DeleteSelectedSave()
        {
            if (selectedSave == null) return;

            var userChoice = await PromptHandler.active.GeneralPrompt(new()
            {
                title = "Delete Save Game",
                question = $"Are you sure you want to delete {selectedSave.saveName}?",
                
                options = new() {
                    new("Confirm", (option) => HandleRemove()),
                    new("Cancel") { isEscape = true }
                },
                blocksScreen = true
            });




            void HandleRemove()
            {
                slots.Remove(selectedSlot);
                selectedSlot.DestroySelf();

                SerializationManager.DeleteSave(selectedSave.SaveFileName());
                selectedSlot = null;
                selectedSave = null;
                UpdateSlots();
                UpdateButtons();

            }
            
        }

        public void OnClickSave(SaveGameSlot slot)
        {
            HandleSelect(slot);
        }

        public void OnDoubleClickSave(SaveGameSlot slot)
        {
            if (selectedSave.saveId <= 0) return;
            LoadGame();
        }
        public void UpdateButtons()
        {
            selectedSaveExists = selectedSlot != null;
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

        public async void RenameSave()
        {
            if (selectedSave == null) return;

            var apply = false;

            var userInput = await PromptHandler.active.InputPrompt(new() {
                title = selectedSave.saveName,
                question = $"New name for {selectedSave.saveName}",
                
                options = new()
                {
                    new("Apply", (option) => {apply = true; }) { affectedByInvalidInput = true },
                    new("Cancel") {isEscape = true},
                },
                IsValidInput = CheckValidInput,
                blocksScreen = true,
                maxInputLength = 30
            });


            if (apply)
            {
                HandleApply();
            }
            

            void HandleApply()
            {
                var newName = userInput.userInput;

                selectedSave.saveName = newName;
                selectedSlot.info.saveName.text = newName;

                selectedSave.Save();

                
            }

            bool CheckValidInput(string input)
            {
                var length = input.Length;

                if (length <= 0) return false;
                if (length >= 30) return false;
                if (input == selectedSave.saveName) return false;

                return true;
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
