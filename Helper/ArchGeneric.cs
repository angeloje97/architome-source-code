using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;
using System.Threading.Tasks;
using System.Linq;

namespace Architome
{
    public class ArchGeneric
    {

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

        public static async void DontDestroyOnLoad(GameObject gameObject, bool canvasItem = false)
        {
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

        public static bool RollSuccess(float roll = 50f)
        {
            var rollChance = UnityEngine.Random.Range(0f, 100f);

            return rollChance < roll;
        }
    }

}