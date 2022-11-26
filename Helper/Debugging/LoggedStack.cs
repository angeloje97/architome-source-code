using Architome.Debugging;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Architome
{
    public class LoggedStack : MonoBehaviour
    {
        public TextMeshProUGUI stack;
        CanvasController canvasController;
        IGDebugger debugger;
        void Start()
        {
            GetDependencies();
        }

        void GetDependencies()
        {
            canvasController = GetComponent<CanvasController>();
            debugger = GetComponentInParent<IGDebugger>();

            if (debugger)
            {
                debugger.OnSelectLogData += HandleSelectLogData;
            }
        }

        void HandleSelectLogData(IGDebugger.LogData logData)
        {
            canvasController.SetCanvas(true);
            stack.text = logData.stack;
        }


        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
