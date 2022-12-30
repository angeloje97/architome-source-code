using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.Serialization
{
    [Serializable]
    public class SaveUI
    {
        [SerializeField] SaveUIModule modules;
        public SaveUIModule Modules { get { modules ??= new(); return modules; } }

        [SerializeField] SaveUIGroupFormation groupFormation;
        public SaveUIGroupFormation GroupFormation { get { groupFormation ??= new(); return groupFormation; } }

        
        public SaveUI()
        {
            modules = new();
        }

    }
}
