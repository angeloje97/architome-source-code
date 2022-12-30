using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using TMPro;

namespace Architome.Settings
{
    public class KeyBindMap : MonoBehaviour
    {
        // Start is called before the first frame update
        public KeyBindMapping mapper;

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
        public string keyCodeString;
        public int index;
        public bool conflicted;

        public KeyCode currentBinding;

        public Action<KeyBindMap> OnSetBinding;

        public void SetMap(string name, string keyCode, int index, KeyBindMapping mapper)
        {
            info.keyNameDisplay.text = $"{ArchString.CamelToTitle(name)}";
            info.keyButtonDisplay.text = keyCode;
            keyName = name;
            keyCodeString = keyCode;
            this.index = index;
            this.mapper = mapper;

            DetermineKeyBind();
        }


        public void DetermineKeyBind()
        {
            currentBinding = (KeyCode)Enum.Parse(typeof(KeyCode), keyCodeString);
        }

        public void SetKeyString(string keyString)
        {
            keyCodeString = keyString;
            info.keyButtonDisplay.text = keyString;
            DetermineKeyBind();
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
                currentBinding = KeyCode.None;
                
            }
            else
            {
                currentBinding = newKey;
            }

            if (info.keyButtonDisplay.text == currentBinding.ToString()) return;

            info.keyButtonDisplay.text = currentBinding.ToString();
            keyCodeString = currentBinding.ToString();

            mapper.UpdateMap(this);
            OnSetBinding?.Invoke(this);
        }


    }
}
