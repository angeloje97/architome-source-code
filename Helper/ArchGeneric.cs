using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Win32.SafeHandles;
using Language.Lua;
using System.Reflection;

namespace Architome
{
    #region Generic
    public static class ArchGeneric
    {
        public static bool disallowPersistant = false;

        public static T CopyComponent<T>(T original, GameObject target) where T : Component
        {
            var component = target.AddComponent<T>();

            foreach (var field in original.GetType().GetFields())
            {
                if (!field.IsDefined(typeof(ObsoleteAttribute), true)) continue;
                if (field.IsStatic) continue;
                
                field.SetValue(component, field.GetValue(original));
            }

            foreach (var prop in original.GetType().GetProperties())
            {
                if (!prop.IsDefined(typeof(ObsoleteAttribute), true)) continue;
                if (!prop.IsDefined(typeof(NotSupportedException), true)) continue;
                if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
                prop.SetValue(component, prop.GetValue(original));
            }
            return component;
        }


        public static T RandomItem<T>(List<T> items)
        {
            var randomIndex = UnityEngine.Random.Range(0, items.Count);
            return items[randomIndex];
        }

        public static T RandomItem<T>(T[] items)
        {
            var randomIndex = UnityEngine.Random.Range(0, items.Length);
            
            return items[randomIndex];
        }

        public static T RandomItem<T>(IEnumerable<T> items)
        {
            var randomIndex =  UnityEngine.Random.Range(0, items.Count());
            var index = 0;
            foreach(var item in items)
            {
                if(index == randomIndex)
                {
                    return item;
                }

                index++;
            }

            return default(T);
        }


        public static T GetTopMostComponent<T>(this Component behavior) where T: Component
        {
            var current = behavior;
            var component = current.GetComponent<T>();

            while (current.transform.parent)
            {
                var parentComponent = current.transform.parent.GetComponent<T>();

                if (parentComponent)
                {
                    component = parentComponent;
                }

                current = current.transform.parent;
            }
            return component;
        }

        public static  void CopyClassValue<T>(T copiedClass, T resultClass) where T: class
        {
            var fields = copiedClass.GetType().GetFields();

            foreach(var field in fields)
            {
                field.SetValue(resultClass, field.GetValue(copiedClass));
            }
        }

        public static List<T> Shuffle<T>(List<T> items)
        {
            var newList = items.ToList();

            var random = new System.Random();

            int count = items.Count;

            while(count > 1)
            {
                count--;

                int randomIndex = random.Next(count + 1);
                (newList[count], newList[randomIndex]) = (newList[randomIndex], newList[count]);
            }

            return newList;
        }
        public static void DestroyOnLoad(GameObject gameObject)
        {
            var tempObject = new GameObject("Destroy on Load");
            gameObject.transform.SetParent(tempObject.transform);
            gameObject.transform.SetParent(null);
            UnityEngine.Object.Destroy(tempObject);
        }

        public static void IterateSafe<T>(List<T> items, Action<T, int> action)
        {
            for(int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                if(item == null)
                {
                    items.RemoveAt(i);
                    i--;
                    continue;
                }

                action(item, i);
            }
        }

        public static List<T> ClearNulls<T>(this List<T> items)
        {
            var newList = new List<T>();

            for(int i = 0; i < items.Count; i++)
            {
                if (items[i] == null)
                {
                    items.RemoveAt(i);
                    i--;
                    continue;
                }

                newList.Add(items[i]);
            }

            return newList;
        }

        public static async void DontDestroyOnLoad(GameObject gameObject, bool canvasItem = false)
        {
            if (disallowPersistant)
            {
                Debug.LogWarning($"Trying to stop {gameObject} from becoming a persistant game object.");
                return;
            }
            gameObject.transform.SetParent(null);

            if (!canvasItem)
            {
                UnityEngine.Object.DontDestroyOnLoad(gameObject);
                return;
            }


            var persistantCanvas = PersistantCanvas.active;

            while(persistantCanvas == null)
            {
                await Task.Yield();
                persistantCanvas = PersistantCanvas.active;
            }

            gameObject.transform.SetParent(persistantCanvas.transform);
        }
        public static string StateDescription(EntityState state)
        {
            var stateDescription = new Dictionary<EntityState, string>() {
                { EntityState.Stunned, "Renders a unit from being able to cast, move, or attack." },
                { EntityState.Immobalized, "Renders a unit from being able to move." },
                { EntityState.Silenced, "Renders a unit from being able to cast." },
                { EntityState.Taunted, "Forces a unit to attack the unit that taunted them." },
                { EntityState.MindControlled, "The unit controlled by another." },
            };



            return stateDescription[state];
        }

        #region Method Names

        public static List<MethodInfo> GetPublicMethods(Type type)
        {
            return GetPublicMethod(type, (method) => true);
        }

        public static List<MethodInfo> GetPublicVoidMethods(Type type)
        {
            return GetPublicMethod(type, (method) => method.ReturnType == typeof(void) && method.GetParameters().Length == 0);
        }


        public static List<MethodInfo> GetPublicMethod(Type type, Predicate<MethodInfo> predicate)
        {
            var list = new List<MethodInfo>();

            var methods = type.GetMethods(BindingFlags.Public);

            foreach (var method in methods)
            {
                if (!predicate(method)) continue;
                list.Add(method);
            }

            return list;
        }
        #endregion

        public static bool RollSuccess(float roll = 50f)
        {
            var rollChance = UnityEngine.Random.Range(0f, 100f);

            return rollChance < roll;
        }

    }
    #endregion

    #region Generic Lists

    public static class LogicList
    {
        public static bool IsFalsey(this List<bool> values)
        {
            if (values.Count == 0) return false;
            foreach(var value in values)
            {
                if (!value) return true;
            }

            return false;
        }

        public static bool IsTruthy(this List<bool> values)
        {
            foreach(var value in values)
            {
                if (!value) return false;
            }

            return true;
        }

        public static bool ValidateLogic(this List<bool> values, bool targetValue, LogicType type)
        {
            return type switch
            {
                LogicType.And => HandleAnd(),
                LogicType.Or => HandleOr(),
                LogicType.Exists => HandleExists(), 
                LogicType.NotExists => HandleNotExists(),
                _ => true,
            };

            bool HandleExists()
            {
                foreach(var value in values)
                {
                    if (value == targetValue) return true;
                }

                return false;
            }

            bool HandleNotExists()
            {
                foreach(var value in values)
                {
                    if (value == targetValue) return false;
                }

                return true;
            }


            bool HandleAnd()
            {

                foreach(var value in values)
                {
                    if (value != targetValue) return false;
                }

                return true;
            }

             bool HandleOr()
            {
                foreach(var value in values)
                {
                    if(value ==  targetValue) return true;
                }

                return false;
            }
        }
    }
    #endregion
}