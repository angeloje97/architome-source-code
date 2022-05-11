using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;

namespace Architome
{
    public class Augment : MonoBehaviour
    {
        public int augmentId;

        public AbilityInfo ability;



        [Serializable]
        public struct Cataling
        {
            public bool enable;
            public GameObject catalyst;
            public AbilityType catalingType;
            public CatalystEvent releaseCondition;
            public int releasePerInterval;
            public float interval, targetFinderRadius, valueContribution, rotationPerInterval, startDelay;
        }

        void GetDependencies()
        {
            ability = GetComponentInParent<AbilityInfo>();
        }
        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
