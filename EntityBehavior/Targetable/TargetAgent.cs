using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Architome
{
    public class TargetAgent : EntityProp
    {
        new void GetDependencies()
        {
            base.GetDependencies();
            Destroy(gameObject);
        }
        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame

    }

}
