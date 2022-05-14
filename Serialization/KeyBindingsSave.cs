using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    [Serializable]
    public class KeyBindingsSave
    {
        static KeyBindingsSave current;
        public static KeyBindingsSave _current
        {
            get
            {
                if (current == null)
                {
                    current = new();
                }

                return current;
            }
        }

        public List<String2> combatBindings;

        public void Save()
        {
            SerializationManager.SaveConfig("KeyBindings", this);
        }

        public KeyBindingsSave LoadBindings()
        {
            var obj = SerializationManager.LoadConfig("KeyBindings");
            if (obj == null) return null;

            var bindings = (KeyBindingsSave)obj;

            CopyValues(bindings);

            return bindings;

        }

        void CopyValues(KeyBindingsSave bindings)
        {
            foreach (var field in bindings.GetType().GetFields())
            {
                field.SetValue(this, field.GetValue(bindings));
            }
        }
    }
}
