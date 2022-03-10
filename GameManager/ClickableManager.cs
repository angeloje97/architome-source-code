using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    public class ClickableManager : MonoBehaviour
    {
        public static ClickableManager active;

        public Action<Clickable> OnClickableObject;

        void Start()
        {

        }

        private void Awake()
        {
            active = this;
        }
        // Update is called once per frame
        void Update()
        {

        }

        public void HandleClickable(Clickable clicked)
        {
            OnClickableObject?.Invoke(clicked);
        }
    }

}
