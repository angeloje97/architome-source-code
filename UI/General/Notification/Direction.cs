using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Architome
{
    public class Direction : Notification
    {

        [Header("Direction Components")]
        public Image completedIndicator;
        protected override void Start()
        {
            base.Start();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        public void CompleteDirection()
        {
            
            completedIndicator.enabled = true;
            completedIndicator.gameObject.SetActive(true);

        }
    }
}
