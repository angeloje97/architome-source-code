using System.Collections;
using UnityEngine;

namespace Architome
{
    public class AwakeTester : MonoBehaviour
    {
        public bool testAwake;
        public AwakeTestItem testItem;
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (!testAwake) return;
            testAwake = false;
            var item = Instantiate(testItem, transform);
            item.PrintInstantiate();
        }
    }
}