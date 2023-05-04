using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class DistanceChecker : MonoBehaviour
    {
        public Transform obj1, obj2;
        public float currentDistance1, currentDistance2;
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if (obj1 == null) return;
            if (obj2 == null) return;

            currentDistance1 = V3Helper.Distance(obj1.position, obj2.position);
            currentDistance2 = Vector3.Distance(obj1.position, obj2.position);
        }
    }
}
