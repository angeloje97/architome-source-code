using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BuffType : MonoBehaviour
    {
        public BuffInfo buffInfo;

        public void GetDependencies()
        {
            buffInfo = GetComponent<BuffInfo>();
        }
    }

}
