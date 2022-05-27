using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BuffType : MonoBehaviour
    {
        public BuffInfo buffInfo;

        public float valueContributionToBuffType = 1;
        public float value = 1;

        public void GetDependencies()
        {
            buffInfo = GetComponent<BuffInfo>();


            if (buffInfo == null)
            {
                Destroy(gameObject);
                return;
            }

            value = valueContributionToBuffType * buffInfo.properties.value;
            
        }
    }

}
