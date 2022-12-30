using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class IgnoreProp : MonoBehaviour
    {
        public bool enableOn;
        public bool enableOff;

        public bool CanSet(bool val)
        {
            if (enableOn && val) return true;
            if (enableOff && !val) return true;
            
            return false;
        }

        public void SetOn(string fieldName)
        {
            var fields = GetType().GetFields();

            foreach(var field in fields)
            {
                if (field.Name != fieldName) continue;
                field.SetValue(this, true);
            }
        }

        public void SetOff(string fieldName)
        {
            foreach (var field in GetType().GetFields())
            {
                if (field.Name != fieldName) continue;
                field.SetValue(this, false);
            }
        }

        public void SetAll(bool val)
        {
            foreach(var field in GetType().GetFields())
            {
                field.SetValue(this, val);
            }
        }

        public void Remove()
        {
            Destroy(this);
        }
    }
}
