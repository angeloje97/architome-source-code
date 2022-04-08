using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    public class Preferences : MonoBehaviour
    {
        public static Preferences active;
        [Serializable]
        public struct PopUps
        {
            public bool showGeneral;
            public bool showDamage;
            public bool showCrowdControl;
        }

        public PopUps popUpPreferences;

        private void Awake()
        {

            active = this;
        }
    }

}
