using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    [Serializable]
    public class UniqueList<T>
    {
        HashSet<T> items;
        [SerializeField] List<T> debugValues;

        public UniqueList()
        {
            items = new();
            debugValues = new();
        }

        public bool Add(T item)
        {
            if(items.Contains(item)) return false;
            items.Add(item);

            return true;
        }

        public bool Remove(T item)
        {
            if (!items.Contains(item)) return false;
            items.Add(item);
            return true;
        }

        public bool Contains(T item) => items.Contains(item);
    }
}
