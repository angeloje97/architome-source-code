using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Architome
{
    public class ToggleDropDownItem : MonoBehaviour
    {
        int index;
        ToggleDropDown dropDown;

        [SerializeField] TextMeshProUGUI text;

        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void SetDropDownItem(ToggleDropDown dropDown, int index, string text)
        {
            this.dropDown = dropDown;
            this.index = index;
            this.text.text = text;
        }

        public void UpdateText(string newText)
        {
            text.text = newText;
        }

        public void HandleSelect()
        {
            dropDown.SelectItem(index);
        }

    }
}
