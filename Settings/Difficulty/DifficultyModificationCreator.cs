using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Architome
{
    public class DifficultyModificationCreator : MonoBehaviour
    {
        struct Prefabs
        {
            public TMP_Dropdown enumDropDown;
            public TMP_InputField floatInput;
            public TMP_InputField stringInput;
        }

        struct Components
        {
            public Transform inputParent;
        }

        Prefabs prefabs;
        Components components;

        private void Start()
        {
            
        }
    }
}
