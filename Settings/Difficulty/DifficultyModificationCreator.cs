using Architome.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Architome
{
    public class DifficultyModificationCreator : MonoBehaviour
    {
        DifficultyModifications modifications;
        DifficultySet current;
        DifficultySet temp;


        struct Prefabs
        {
            public TMP_Dropdown enumDropDown;
            public TMP_InputField floatInput;
            public TMP_InputField stringInput;
        }

        struct Components
        {
            public Transform inputParent;
            public TMP_Dropdown setSelector;
        }

        Prefabs prefabs;
        Components components;

        private void Start()
        {
            GetDependencies();
            UpdateSetSelector();
            UpdateFields();
        }

        private void OnValidate()
        {
            Debug.Log($"{AbilityType.Spawn} is Enum: {AbilityType.Spawn is Enum}");
        }

        void GetDependencies()
        {
            modifications = DifficultyModifications.active;
            current = modifications.settings;
            temp = current;
        }

        void UpdateSetSelector()
        {
            var dropDown = components.setSelector;
            if (modifications = null) return;
            if (dropDown == null) return;

            var options = new List<TMP_Dropdown.OptionData>();

            foreach(var set in modifications.difficultySets)
            {
                options.Add(new() { text = set.name });
            }

            dropDown.options = options;

        }

        void UpdateFields()
        {
            if (modifications == null) return;
            if (components.inputParent == null) return;

            var tempType = temp.GetType();
            var fields = tempType.GetFields();


            foreach(var field in fields)
            {
                var value = field.GetValue(temp);
                HandleEnum(value, field.Name);
                HandleBool(value, field.Name);
                HandleFloat(value, field.Name);
            }


            void HandleEnum(object value, string fieldName)
            {
                if (value is not Enum) return;
                var enumVal = (Enum)value;

                var dropDown = Instantiate(prefabs.enumDropDown, components.inputParent);

                ArchUI.SetDropDown((Enum newValue) => {
                    var field = tempType.GetField(fieldName);
                    field.SetValue(temp, newValue);
                }, dropDown, enumVal);
            }

            void HandleBool(object value, string fieldName)
            {
                if (value is not bool) return;
                var boolVal = (bool)value;
            }

            void HandleFloat(object value, string fieldName)
            {
                if (value is not float) return;
                var floatVal = (float)value;
            }
        }
    }
}
