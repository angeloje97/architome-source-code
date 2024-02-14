using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class AwakeTestItem : MonoBehaviour
    {
        public string log;

        private void Awake()
        {
            log += "Awake\n";
        }

        public void PrintInstantiate()
        {
            log += "Instantaite\n";
        }
    }
}