using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace Architome
{
    public struct ArchAction
    {
        // Start is called before the first frame update
        public static async void Delay(Action action, float seconds)
        {
            int milliSeconds = (int)(seconds * 1000);
            await Task.Delay(milliSeconds);

            action();
        }
        public static async Task DelayT(Action action, float seconds)
        {
            int milliSeconds = (int)(seconds * 1000);

            await Task.Delay(milliSeconds);

            action();
        }
        public static async void Yield(Action action)
        {
            await Task.Yield();
            action();
        }
        public static async void UpdateWhile(Action action, ArchCondition condition)
        {
            while (condition.IsMet())
            {
                action();
                await Task.Yield();
            }
        }
        public static async void UpDateUntil(Action action, ArchCondition condition)
        {
            while (!condition.IsMet())
            {
                action();
                await Task.Yield();
            }
        }
        public static async void Update(Action action)
        {
            while(true)
            {
                action();
                await Task.Yield();
            }
        }
        public static async void UpdateFor(Action action, float seconds)
        {
            float currentTime = 0f;
            while(currentTime < seconds)
            {
                action();

                currentTime += Time.deltaTime;
                await Task.Yield();

            }
        }
        public static async Task UpdateForT(Action action, float seconds)
        {
            float currentTime = 0f;

            while (currentTime < seconds)
            {
                action();
                currentTime += Time.deltaTime;
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
        public static async void Interval(Action action, float interval, bool invokeOnStart = false)
        {
            int milliSeconds = (int)(interval * 1000);

            if (invokeOnStart)
            {
                action();
            }

            while(true)
            {
                await Task.Delay(milliSeconds);
                action();
            }
        }
        public static async void IntervalT(Action action, float interval, bool invokeOnStart = false)
        {
            int milliSeconds = (int)(interval * 1000);

            if (invokeOnStart)
            {
                action();
            }

            while (true)
            {
                await Task.Delay(milliSeconds);
                action();
            }
        }
        public static async void IntervalFor(Action action, float interval, float seconds, bool invokeOnStart = false)
        {
            float currentTime = 0;
            int milliSeconds = (int)(interval * 1000);

            if (invokeOnStart)
            {
                action();
            }

            while (currentTime < seconds)
            {
                await Task.Delay(milliSeconds);
                currentTime += interval;
                action();
            }
        }
        public static async Task IntervalForT(Action action, float interval, float seconds, bool invokeOnStart = false)
        {
            float currentTime = 0;
            int milliSeconds = (int)(interval * 1000);

            if (invokeOnStart)
            {
                action();
            }

            while (currentTime < seconds)
            {
                await Task.Delay(milliSeconds);
                currentTime += interval;
                action();
            }
        }
        public static async void RepeateFor(Action action, float interval, int amount, bool invokeOnStart = false)
        {
            int count = 0;
            int milliSeconds = (int)interval * 1000;

            if (invokeOnStart)
            {
                action();
            }

            while (count < amount)
            {
                await Task.Delay(milliSeconds);

                action();

                count++;
            }
        }
        public static async Task RepeateForT(Action action, float interval, int amount, bool invokeOnStart = false)
        {
            int count = 0;
            int milliSeconds = (int)interval * 1000;

            if (invokeOnStart)
            {
                action();
            }

            while (count < amount)
            {
                await Task.Delay(milliSeconds);

                action();

                count++;
            }
        }

        public static async Task<KeyCode> NewKey()
        {
            while (true)
            {
                await Task.Yield();

                foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(keyCode))
                    {
                        return keyCode;
                    }
                }
            }
        }

    }


    public class ArchCondition
    {
        public bool condition;

        public float value;
        public float maxValue;

        public float range;
        public float minRange;
        public float maxRange;

        public bool IsMet()
        {
            if (condition == true)
            {
                return true;
            }

            if (value > maxValue)
            {
                return true;
            }

            if (range < minRange && range < maxRange)
            {
                return true;
            }

            return false;
        }
    }
}
