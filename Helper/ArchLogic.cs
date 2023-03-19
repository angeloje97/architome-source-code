using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public enum LogicType
    {
        And,
        Or,
        Not,
    }
    public class ArchLogic
    {
        public List<bool> values;

        public ArchLogic(List<bool> values)
        {
            this.values = values;
        }

        public bool And()
        {
            foreach(var value in values)
            {
                if (!value) return false;
            }

            return true;
        }

        public bool Or()
        {
            foreach(var value in values)
            {
                if (value) return true;
            }
            return false;
        }

        public bool Valid(LogicType type)
        {
            return type switch
            {
                LogicType.And => And(),
                LogicType.Or => Or(),
                _ => false,
            };
        }
    }
}
