using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem active { get; private set; }

        public Action<SaveGame> OnSave { get; set; }
        public Action<SaveGame> BeforeSave { get; set; }

        private void Awake()
        {
            active = this;
        }

        public void Save()
        {
            if (Core.currentSave == null) return;

            BeforeSave?.Invoke(Core.currentSave);

            Core.SaveCurrent();

            OnSave?.Invoke(Core.currentSave);

            return;
        }
    }
}
