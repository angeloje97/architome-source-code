using Architome.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
            public TMP_InputField stringInput;
            public Toggle toggle;
        }

        struct Components
        {
            public Transform inputParent;
            public TMP_Dropdown setSelector;
        }

        Prefabs prefabs;
        Components components;

        List<GameObject> fieldObjects;

        private void Start()
        {
            GetDependencies();
            UpdateSetSelector();
            UpdateFields();
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

        void ResetFields()
        {
            fieldObjects ??= new();
            
            foreach(var fieldObj in fieldObjects)
            {
                Destroy(fieldObj);
            }

            fieldObjects = new();
        }

        void UpdateFields()
        {
            if (modifications == null) return;
            if (components.inputParent == null) return;
            ResetFields();

            var tempType = temp.GetType();
            var fields = tempType.GetFields();


            foreach(var field in fields)
            {
                var value = field.GetValue(temp);
                HandleEnum(value, field);
                HandleBool(value, field);
                HandleFloat(value, field);
            }


            void HandleEnum(object value, FieldInfo field)
            {
                if (value is not Enum) return;
                var enumVal = (Enum)value;

                var dropDown = Instantiate(prefabs.enumDropDown, components.inputParent);

                fieldObjects.Add(dropDown.gameObject);

                ArchUI.SetDropDown((Enum newValue) => {
                    field.SetValue(temp, newValue);
                }, dropDown, enumVal);
            }

            void HandleBool(object value, FieldInfo field)
            {
                if (value is not bool) return;
                var boolVal = (bool)value;

                var toggle = Instantiate(prefabs.toggle, components.inputParent);
                fieldObjects.Add(toggle.gameObject);

                ArchUI.SetToggle((bool newValue) => {
                    field.SetValue(temp, newValue);
                }, toggle, (bool)value);
            }

            void HandleFloat(object value, FieldInfo field)
            {
                if (value is not float) return;
                var floatVal = (float)value;

                var floatInput = Instantiate(prefabs.stringInput, components.inputParent);

                ArchUI.SetInputField((string newValue) => {
                    field.SetValue(temp, (float) Convert.ToDouble(newValue));
                }, floatInput, value.ToString(), "%[0-9]%");
            }
        }
    }
}
