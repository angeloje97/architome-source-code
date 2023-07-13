using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public class SmoothFunctionTesting : MonoBehaviour
    {
        public bool saveStart;
        public bool saveEnd;

        public Vector3 startPosition;
        public Vector3 endPosition;
        public float time;
        public float endAccel;
        public float startDeccel;
        public float delay;

        async void Start()
        {
            await Task.Delay((int)(1000 * delay));
            await ArchGeneric.Smooth((float lerpValue) => {
                transform.position = Vector3.Lerp(startPosition, endPosition, lerpValue);
            }, time, endAccel, startDeccel);
        }

        private void OnValidate()
        {
            if (saveStart)
            {
                saveStart = false;
                startPosition = transform.position;
            }

            if (saveEnd)
            {
                saveEnd = false;
                endPosition = transform.position;
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
