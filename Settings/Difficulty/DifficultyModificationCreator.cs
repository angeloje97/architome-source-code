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
        DifficultySet tempo;


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
        }

        void GetDependencies()
        {
            modifications = DifficultyModifications.active;
            UpdateSetSelector();
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
    }
}
