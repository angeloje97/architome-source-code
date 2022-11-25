using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Architome.Debugging
{
    public class LoggedItem : MonoBehaviour, IPointerDownHandler
    {
        public IGDebugger.LogData data;

        bool isHovering, isSelected;

        [Header("Components")]
        public TextMeshProUGUI type;
        public TextMeshProUGUI log;






        void Start()
        {
        
        }

        private void OnValidate()
        {
        }

        public void SetLogData(IGDebugger.LogData data, Color color)
        {
            this.data = data;

            type.text = $"{data.type}:";
            log.text = data.log;

            type.color = color;

            data.taken = true;
        }

        public void RemoveSelf()
        {
            Destroy(gameObject);
        }



        public void OnPointerDown(PointerEventData eventData)
        {
            isSelected = true;
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
