using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public class InvokerQueueHandler
    {
        public int maxInvokesQueued = 1;
        public float delayAmount = 1f;
        public bool invokeAtStart = true;
        bool isDelayed = false;
        int invokesQueued;

        public async void InvokeAction(Action action)
        {
            if (invokesQueued >= maxInvokesQueued) return;

            invokesQueued += 1;
            while (isDelayed) await Task.Yield();
            invokesQueued -= 1;

            if (invokeAtStart)
            {
                action();
            }


            isDelayed = true;

            await Task.Delay((int) (1000 * delayAmount));

            if (!invokeAtStart)
            {
                action();
            }

            isDelayed = false;
        }
    }
}
