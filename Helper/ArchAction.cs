using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace Architome
{
    public class ArchAction
    {
        // Start is called before the first frame update
        public static async void Delay(Action action, int time)
        {
            await Task.Delay(time);

            action();
        }

        public static async void Update(Action action)
        {
            while(true)
            {
                action();
                await Task.Yield();
            }
        }

        public static async void LateUpdate(Action action)
        {
            while(true)
            {
                await Task.Yield();
                action();
            }
        }
    }

}
