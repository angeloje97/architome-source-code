using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace Architome
{
    public class TargetAgent : EntityProp
    {
        public override void GetDependencies()
        {
            Destroy(gameObject);
        }

        // Update is called once per frame

    }

}
