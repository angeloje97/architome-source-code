using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.Testing
{
    public class FieldMarkUpTest : MonoBehaviour
    {
        public FieldMarkUp<FieldMarkUpTest> fieldMarkUp;

        public float field1, field2, field3;
        public bool bool1, bool2, bool3;

        private void OnValidate()
        {
            fieldMarkUp.Validate(this);
        }
    }
}
