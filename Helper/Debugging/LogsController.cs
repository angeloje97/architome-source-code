using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.Debugging
{
    public class LogsController : MonoBehaviour
    {
        [SerializeField] ToggleDropDown toggleDropDown;
        void Start()
        {
            if (toggleDropDown == null) return;
            UpdateOptions();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void UpdateOptions()
        {
            var options = new List<ToggleDropDown.ToggleData>();

            foreach(ALogType logType in Enum.GetValues(typeof(ALogType)))
            {
                var isOn = Debugger.Status(logType);

                var suffix = isOn ? "On" : "Off";
                options.Add(new() {
                    option = $"{logType}: {suffix}",
                });
            }

            toggleDropDown.SetToggleOptions(options);
        }

        public void UpdateLogType(int index)
        {
            var logType = (ALogType) Enum.GetValues(typeof(ALogType)).GetValue(index);

            Debugger.Toggle(logType);

            UpdateOptions();

        }
    }
}
