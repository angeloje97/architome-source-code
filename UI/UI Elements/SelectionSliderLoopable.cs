using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.Events;

namespace Architome
{
    public class SelectionSliderLoopable : MonoBehaviour
    {
        // Start is called before the first frame update
        
        [Serializable]
        public struct Info
        {
            public TextMeshProUGUI displayText;
        }

        public Info info;

        public int index;
        public List<string> options = new();

        public UnityEvent OnChangeValue;

        bool active = false;

        private void Start()
        {
            active = true;
        }

        public void Next()
        {
            index++;
            UpdateText();
        }

        public void Previous()
        {
            index--;
            UpdateText();
        }

        public void SetIndex(int index)
        {
            this.index = index;
            UpdateText();
        }

        void UpdateText()
        {
            if(options.Count == 0) return;
            if (index < 0)
            {
                index += options.Count;
            }

            index %= options.Count;

            info.displayText.text = options[index];

            if (!active) return;

            OnChangeValue?.Invoke();
        }

        

        private void OnValidate()
        {
            UpdateText();
        }
    }
}
