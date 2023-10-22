using Architome.Enums;
using Architome.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Architome
{
    public class DifficultyModificationCreator : ArchSettings
    {
        [Header("Difficulty Modification Properties")]
        DifficultyModifications modifications;
        DifficultySet current;
        DifficultySet temp;

        bool createdFields;

        public Action onUpdateFields;

        struct Prefabs
        {
            public TMP_Dropdown enumDropDown;
            public TMP_InputField stringInput;
            public Toggle toggle;
        }

        struct Components
        {
            public Transform inputParent;
            public TMP_Dropdown setSelector;
            public Button saveButton;
        }

        Prefabs prefabs;
        Components components;


        private void Start()
        {
            GetDependencies();
            UpdateSetSelector();
            UpdateFields();
            HandleDirtyConflicts();

            if (components.saveButton)
            {
                HandleDirtyChange((bool isDirty) => components.saveButton.interactable = isDirty );
            }

        }

        void GetDependencies()
        {
            modifications = DifficultyModifications.active;
            SetSet(modifications.settings);
        }

        void UpdateSetSelector()
        {
            var dropDown = components.setSelector;
            if (modifications = null) return;
            if (dropDown == null) return;

            var options = new List<TMP_Dropdown.OptionData>();
            dropDown.onValueChanged.RemoveAllListeners();

            foreach(var set in modifications.difficultySets)
            {
                options.Add(new() { text = set.name });
            }

            dropDown.options = options;

            dropDown.onValueChanged.AddListener((int index) => {
                SetSet(modifications.difficultySets[index]);
            });
        }

        void ResetFields()
        {
            SetSet(current);

        }

        public void SaveFields()
        {
            SetSet(temp);
            ArchGeneric.CopyClassValue(current, modifications.DifficultySet(current.name, true));
            modifications.SaveCurrentSet();
            UpdateSetSelector();
        }

        public void SetSet(DifficultySet set)
        {
            ArchGeneric.CopyClassValue(set, current);
            ArchGeneric.CopyClassValue(current, temp);
            onUpdateFields?.Invoke();
            SetDirty(false);
        }


        void UpdateFields()
        {
            if (modifications == null) return;
            if (components.inputParent == null) return;

            ResetFields();

            var tempType = temp.GetType();
            var fields = tempType.GetFields();


            if (!createdFields)
            {
                foreach(var field in fields)
                {
                    var value = field.GetValue(temp);
                    HandleEnum(value, field);
                    HandleBool(value, field);
                    HandleFloat(value, field);
                    HandleString(value, field);
                }
                createdFields = true;
            }


            void HandleEnum(object value, FieldInfo field)
            {
                if (value is not Enum) return;
                var enumVal = (Enum)value;

                var dropDown = Instantiate(prefabs.enumDropDown, components.inputParent);

                var enums = Enum.GetValues(enumVal.GetType());


                var setDropDown = ArchUI.SetDropDown((Enum newValue) => {
                    field.SetValue(temp, newValue);
                    SetDirty(true);
                }, dropDown, enumVal);

                onUpdateFields += () => {
                    var updatedValue = (Enum) field.GetValue(temp);
                    setDropDown(updatedValue);
                };

            }

            void HandleBool(object value, FieldInfo field)
            {
                if (value is not bool) return;
                var boolVal = (bool)value;

                var toggle = Instantiate(prefabs.toggle, components.inputParent);

                ArchUI.SetToggle((bool newValue) => {
                    field.SetValue(temp, newValue);
                    SetDirty(true);

                }, toggle, (bool)value);

                onUpdateFields += () => {
                    toggle.SetIsOnWithoutNotify((bool)field.GetValue(temp));
                };
            }

            void HandleFloat(object value, FieldInfo field)
            {
                if (value is not float) return;
                var floatVal = (float)value;

                var floatInput = Instantiate(prefabs.stringInput, components.inputParent);

                ArchUI.SetInputField((string newValue) => {
                    field.SetValue(temp, (float) Convert.ToDouble(newValue));
                    SetDirty(true);

                }, floatInput, value.ToString(), "%[0-9]%");

                onUpdateFields += () => {
                    floatInput.SetTextWithoutNotify(field.GetValue(temp).ToString());
                };
            }

            void HandleString(object value, FieldInfo field)
            {
                if (value is not string) return;
                var stringVal = (string)value;

                var stringInput = Instantiate(prefabs.stringInput, components.inputParent);

                ArchUI.SetInputField((string newString) => {
                    field.SetValue(temp, newString);
                    SetDirty(true);

                }, stringInput, stringVal);

                onUpdateFields += () => {
                    stringInput.SetTextWithoutNotify(field.GetValue(temp).ToString());
                };
            }
        }

        void SetDirty(bool newValue)
        {
            dirty = newValue;
        }

        #region ArchSettings Functions
        public override void HandleChooseApply() => SaveFields();
        public override void HandleLeaveDirty() => ResetFields();

        #endregion
    }
}
