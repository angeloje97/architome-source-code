using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Architome.Debugging
{
    public class LoggedItem : MonoBehaviour
    {
        public IGDebugger.LogData data;
        IGDebugger debugger;

        [Header("Components")]
        public TextMeshProUGUI type;
        public TextMeshProUGUI log;


        [SerializeField] int position;

        public Action<LoggedItem> OnDestroy;

        void Start()
        {
        
        }

        private void OnValidate()
        {
        }

        public void SetLogData(IGDebugger.LogData data, Color color)
        {
            this.data = data;
            data.SetLogItem(this);
            data.OnIncrement += HandleIncrement;
            UpdateText();
            log.text = data.log;

            type.color = color;

            data.taken = true;
        }

        void UpdateText()
        {
            var text = $"{data.type}";

            if(data.amount > 1)
            {
                text += $"({data.amount})";
            }

            text += ":";

            type.text = text;
        }

        public void SetDebugger(IGDebugger debugger)
        {
            this.debugger = debugger;
            position = 1;
            
            debugger.OnNewLog += HandleNewLog;
            debugger.OnStackedLog += HandleStackedLog;

            OnDestroy += (LoggedItem loggedItem) => {
                debugger.OnNewLog -= HandleNewLog;
                debugger.OnStackedLog -= HandleStackedLog;
            };

            void HandleNewLog(LoggedItem other)
            {
                position++;

                if(position > debugger.maxLogs)
                {
                    debugger.OnNewLog -= HandleNewLog;
                    debugger.OnStackedLog -= HandleStackedLog;
                    RemoveSelf();
                }
            }

            void HandleStackedLog(LoggedItem other)
            {
                if (other == this) return;

                if(other.position > this.position)
                {
                    position++;
                }
            }
        }

        void HandleIncrement(IGDebugger.LogData logData)
        {
            position = 1;
            transform.SetAsLastSibling();
            UpdateText();
        }

        public void RemoveSelf()
        {
            this.data.OnIncrement -= HandleIncrement;
            if(data.itemHost == this)
            {
                data.itemHost = null;
            }

            OnDestroy?.Invoke(this);

            Destroy(gameObject);
        }

        public void SelectLog()
        {
            debugger.SelectLogData(data);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
