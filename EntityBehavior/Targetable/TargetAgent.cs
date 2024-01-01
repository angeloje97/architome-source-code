using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace Architome
{
    public class TargetAgent : EntityProp
    {
        public override async Task GetDependencies(Func<Task> extension)
        {
            await base.GetDependencies(async () => {
                Destroy(gameObject);

                await extension();
            });
        }

        // Update is called once per frame

    }

}
