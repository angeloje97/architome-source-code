using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{

    public class LayerMasksData : MonoBehaviour
    {
    
        public static LayerMasksData active { get; private set; }

        public LayerMask structureLayerMask;
        public LayerMask wallLayer;
        public LayerMask entityLayerMask;
        public LayerMask walkableLayer;

        private void Awake()
        {
            active = this;
        }
    }

    public static class LayerMasksExtensions 
    {
        public static bool ContainsLayer(this LayerMask mask, int layer)
        {
            if(((1 << layer) & mask) != 0)
            {
                return true;
            }
            return false;
        }
    }


}

