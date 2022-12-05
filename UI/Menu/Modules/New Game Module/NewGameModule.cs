using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Architome.Enums;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


namespace Architome
{
    public class NewGameModule : MonoBehaviour
    {
        // Start is called before the first frame update


        [Serializable]
        public struct Info
        {
            public TMP_InputField inputField;
            public SelectionSliderLoopable selection;
            public Toggle toggle;
            public string sceneToLoad;
            public ArchButton createNewGameButton;
        }

        public Info info;

        public string saveName;
        public Trilogy trilogy;
        public Difficulty currentDifficulty;
        public GameSettingsData settings;

        public SaveGame newSave;


        private void OnValidate()
        {
            CreateOptions();
        }

        public void Start()
        {
            OnSaveNameChange(info.inputField);
        }

        public void CreateOptions()
        {
            if (info.toggle)
            {
            }

            if (info.selection)
            {
                info.selection.options.Clear();

                foreach (Difficulty difficulty in Enum.GetValues(typeof(Difficulty)))
                {
                    info.selection.options.Add(difficulty.ToString());
                }

                currentDifficulty = (Difficulty)Enum.GetValues(typeof(Difficulty)).GetValue(info.selection.index);

                settings.difficulty = currentDifficulty;
            }

        }

        public void OnSaveNameChange(TMP_InputField inputField)
        {
            var valid = inputField.text != null && inputField.text.Length != 0;

            info.createNewGameButton.SetButton(valid);
        }

        public void SelectDifficulty(SelectionSliderLoopable slider)
        {
            var enums = Enum.GetValues(typeof(Difficulty));
            if (slider.index >= enums.Length) return;

            currentDifficulty = (Difficulty) Enum.GetValues(typeof(Difficulty)).GetValue(slider.index);

            settings.difficulty = currentDifficulty;

        }

        public void SetName(TMP_InputField inputField)
        {
            this.saveName = inputField.text;
        }

        public void ToggleHardCore(Toggle toggle)
        {
            settings.hardcore = toggle.isOn;
        }

        public void ClearForm()
        {
            if (info.toggle)
            {
                info.toggle.isOn = false;
            }

            if (info.selection)
            {
                info.selection.SetIndex(0);
            }

            if (info.inputField)
            {
                info.inputField.text = "";
            }
        }


        public void CreateNewSave()
        {
            SaveGame newSave = new()
            {
                saveName = saveName,
                gameSettings = settings,
                trilogy = trilogy,
                time = DateTime.Now,
                build = Application.version,
                newBorn = true
            };

            this.newSave = newSave;

            Core.SetSave(newSave);


            ArchSceneManager.active.LoadScene(info.sceneToLoad, true);
            //SceneManager.LoadScene(info.sceneToLoad);
            
        }


    }
}
