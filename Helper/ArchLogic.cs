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
        Exists,
        NotExists,
        NoFalse,
        NoTrue,
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
        public bool Exists()
        {
            foreach(var value in values)
            {
                if (value) return true;
            }

            return false;
        }
        public bool NotExists()
        {
            foreach(var value in values)
            {
                if (value) return false;
            }

            return true;
        }
        public bool NoFalse()
        {
            foreach(var value in values)
            {
                if (!value) return false;
            }

            return true;
        }
        public bool NoTrue()
        {
            foreach(var value in values)
            {
                if (value) return false;
            }

            return true;
        }
        public bool Valid(LogicType type)
        {
            return type switch
            {
                LogicType.And => And(),
                LogicType.Or => Or(),
                LogicType.Exists => Exists(),
                LogicType.NotExists => NotExists(),
                LogicType.NoFalse => NoFalse(),
                LogicType.NoTrue => NoTrue(),
                _ => false,
            };
        }
    }
}
