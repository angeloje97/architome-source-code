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
        public bool test;

        public Vector3 startPosition;
        public Vector3 endPosition;
        public float time;

        bool testing;

        float timeElapsed;

        private void Start()
        {
            transform.position = startPosition;
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
            if (test)
            {
                test = false;
                Test();
            }
        }

        async void Test()
        {
            if (testing) return;
            testing = true;

            timeElapsed = 0f;
            await ArchCurve.Smooth((float lerpValue) => {
                transform.position = Vector3.Lerp(startPosition, endPosition, lerpValue);
                timeElapsed += Time.deltaTime;
            }, CurveType.EaseInOut, time);

            testing = false;
        }
    }
}
