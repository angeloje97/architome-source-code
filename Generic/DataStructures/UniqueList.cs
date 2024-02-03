using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    #region UniqueList
    [Serializable]
    public class UniqueList<T>
    {
        HashSet<T> items;
        [SerializeField] List<T> debugValues;
        [SerializeField] bool enableDebug;
        public UniqueList(Component listener)
        {
            items = new();
            debugValues = new();

            HandleDebug(listener);
        }

        async void HandleDebug(Component listener)
        {
            while(listener != null)
            {
                if (enableDebug)
                {

                    debugValues = new();

                    foreach(var item in items)
                    {
                        debugValues.Add(item);
                    }
                }

                await Task.Delay(1000);
            }
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
            items.Remove(item);
            return true;
        }

        public bool Contains(T item) => items.Contains(item);
    }
    #endregion
}
