using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public static class SingletonManger
    {
        static Dictionary<Type, GameObject> singleTons;

        public static void HandleSingleton(Type type, GameObject instance, bool persistantGameObject = false, bool canvasItem = false)
        {
            singleTons ??= new();

            if (singleTons.ContainsKey(type))
            {
                UnityEngine.Object.Destroy(instance);
                return;
            }

            singleTons.Add(type, instance);
            Debugger.System(4412, $"Creating singleton for {type}");

            if (persistantGameObject)
            {
                ArchGeneric.DontDestroyOnLoad(instance, canvasItem);
            }
        }
    }
}
