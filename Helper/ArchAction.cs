using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace Architome
{
    public  struct ArchAction
    {
        static World world;
        static World World
        {
            get
            {
                if(world == null)
                {
                    world = World.active;
                }

                return world;
            }
        }

        public static async void Delay(Action action, float seconds)
        {
            int milliSeconds = (int)(seconds * 1000);
            await Task.Delay(milliSeconds);
            if (!Application.isPlaying) return;
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
            if (!Application.isPlaying) return;
            await Task.Yield();
            action();
        }
        public static async void YieldFor(Action action, int amount = 1)
        {
            for (int i = 0; i < amount; i++)
            {
                await Task.Yield();
            }

            action();
        }

        //Will keep running until predicate returns expected
        public static async Task WaitUntil(Predicate<float> predicate, bool expected)
        {
            await World.UpdateAction((float deltaTime) => {
                return predicate(deltaTime) != expected;
            });
        }

        public static async Task WaitUntil(Func<bool> predicate, bool expected, float delay = 0f)
        {
            if(delay == 0)
            {
                await World.UpdateAction((float deltaTime) => {
                    return predicate() != expected;
                });

            }
            else
            {
                await World.ActionInterval((float deltaTime) =>
                {
                    return predicate() != expected;
                }, delay);
            }
        }

        public static async void UpdateWhile(Action action, ArchCondition condition)
        {
            while (condition.isMet)
            {
                if (!Application.isPlaying) return;
                action();
                await Task.Yield();
            }
        }
        public static async void UpDateUntil(Action action, ArchCondition condition)
        {
            while (!condition.isMet)
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
            var keyCodes = Enum.GetValues(typeof(KeyCode));
            while (true)
            {
                await Task.Yield();

                foreach (KeyCode keyCode in keyCodes)
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
        public List<bool> conditions;
        public bool isMet;
        public bool condition;

        public float value;
        public float maxValue;

        public float range;
        public float minRange;
        public float maxRange;

        public bool OrIsMet()
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

        public bool OrIsMetConditions()
        {
            if (conditions == null) return true;

            foreach (var condition in conditions)
            {
                if (condition)
                {
                    return true;
                }
            }

            return false;
        }

        public bool OnlyOne()
        {
            if (conditions == null) return false;

            var active = false;

            foreach (var condition in conditions)
            {
                if (!condition) continue;

                if (active)
                {
                    active = false;
                    break;
                }

                active = true;
            }

            return active;
        }

        public bool EveryCon()
        {
            if (conditions == null) return false;

            foreach (var condition in conditions)
            {
                if (!condition)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
