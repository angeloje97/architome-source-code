using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Architome
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleController : MonoBehaviour
    {
        Toggle toggle;
        public bool isOn;

        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if (!toggle) return;

            if(isOn != toggle.isOn)
            {
                isOn = toggle.isOn;
            }
        }

        void GetDependencies()
        {
            toggle = GetComponent<Toggle>();
        }
    }
}
