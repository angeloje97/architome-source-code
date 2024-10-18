using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public static class SingletonManger
    {
        #region Initialization

        static Dictionary<Type, GameObject> singleTons;
        static Dictionary<Type, MonoBehaviour> monoSingleTons;

        public static void HandleSingleton(Type type, GameObject instance, bool persistantGameObject = false, bool canvasItem = false, Action onSuccess = null)
        {
            singleTons ??= new();

            if (singleTons.ContainsKey(type))
            {
                if (instance != singleTons[type])
                {
                    UnityEngine.Object.Destroy(instance);
                }

                return;
            }

            singleTons.Add(type, instance);
            Debugger.System(4412, $"Creating singleton for {type}");

            if (persistantGameObject)
            {
                ArchGeneric.DontDestroyOnLoad(instance, canvasItem);
            }
            onSuccess?.Invoke();
        }

        public static void HandleSingleton(Type type, MonoBehaviour behavior, bool persistantGameObject = false, bool canvasItem = false, Action onSuccess = null)
        {
            monoSingleTons ??= new();

            if (monoSingleTons.ContainsKey(type))
            {
                if (behavior != monoSingleTons[type])
                {
                    UnityEngine.Object.Destroy(behavior);
                }

                return;
            }

            monoSingleTons.Add(type, behavior);
            Debugger.System(4412, $"Creating singleton for {type}");

            if (persistantGameObject)
            {
                ArchGeneric.DontDestroyOnLoad(behavior.gameObject, canvasItem);
            }
            onSuccess?.Invoke();
        }



        public static GameObject GetSingleTon(Type type)
        {
            if (singleTons.ContainsKey(type))
            {
                return singleTons[type];
            }

            return null;
        }

        public static T GetSingleTon<T>(Type type) where T: MonoBehaviour
        {
            if (monoSingleTons.ContainsKey(type))
            {
                return (T) monoSingleTons[type];
            }

            return null;
        }

        #endregion
    }
}
