using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class EntityProp : MonoBehaviour
    {
        // Start is called before the first frame update
        public EntityInfo entityInfo;
        public void GetDependencies()
        {
            entityInfo = GetComponentInParent<EntityInfo>();

        }
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
