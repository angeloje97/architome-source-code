using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using TMPro;
using Architome.Settings.Keybindings;

namespace Architome.Settings
{
    public class KeyBindMap : MonoBehaviour
    {
        // Start is called before the first frame update
        public KeyBindMapping mapper;
        public KeybindSet sourceSet;
        

        [Serializable]
        public struct Info
        {
            public TextMeshProUGUI keyNameDisplay;
            public TextMeshProUGUI keyButtonDisplay;
            public Color valid;
            public Color conflicted;
        }

        public Info info;

        public string keyName;
        public string originalName;
        public KeyCode keyCode;
        public int index;
        public bool conflicted;


        public Action<KeyBindMap> OnSetBinding;

        public void SetMap(string name, KeyCode keyCode, int index, KeyBindMapping mapper, KeybindSet set)
        {
            info.keyNameDisplay.text = $"{ArchString.CamelToTitle(name)}";
            info.keyButtonDisplay.text = keyCode.ToString();
            keyName = name;
            this.keyCode = keyCode;
            this.index = index;
            this.mapper = mapper;
            sourceSet = set;
        }

        public void SetKeyString(KeyCode keyCode)
        {
            this.keyCode = keyCode;
            info.keyButtonDisplay.text = keyCode.ToString();
        }

        public void SetConflict(bool conflicted)
        {
            if (this.conflicted == conflicted) return;
            this.conflicted = conflicted;
            info.keyButtonDisplay.color = this.conflicted ? info.conflicted : info.valid;
        }


        async public void ReadMap()
        {
            info.keyButtonDisplay.text = "_";

            mapper.pickingKey = true;
            var newKey = await ArchAction.NewKey();

            while (Input.GetKey(newKey))
            {
                await Task.Yield();
            }

            mapper.pickingKey = false;


            if (mapper.blackList.Contains(newKey))
            {
                keyCode = KeyCode.None;
                
            }
            else
            {
                keyCode = newKey;
            }

            var keyString = keyCode.ToString();
            if (info.keyButtonDisplay.text == keyString) return;

            info.keyButtonDisplay.text = keyString;
            

            mapper.UpdateMap(sourceSet, this);
            OnSetBinding?.Invoke(this);
        }


    }
}
