using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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


    }

}