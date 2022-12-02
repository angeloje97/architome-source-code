using DungeonArchitect.Samples.SnapGridFlow;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{

    [Serializable]
    public class ActionHashList<T> : IEnumerable<T>
    {

        public HashSet<T> hash;
        public List<T> list;

        public Action<T> OnAddItem { get; set; }
        public Action<T> OnRemoveItem { get; set; }
        public Action<T> OnUpdateItem { get; set; }

        public int Length
        {
            get
            {
                return list.Count;
            }
        }

        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        public ActionHashList()
        {
            hash = new();
            list = new();
        }

        public bool Add(T item)
        {
            if (hash.Contains(item))
            {
                return false;
            }


            hash.Add(item);
            list.Add(item);
            OnAddItem?.Invoke(item);
            return true;
        }


        public bool Remove(T item)
        {
            if (!hash.Contains(item)) return false;

            hash.Remove(item);
            list.Remove(item);
            OnRemoveItem?.Invoke(item);

            return true;
        }

        public void RemoveAt(int index)
        {
            var item = list[index];
            hash.Remove(item);
            list.RemoveAt(index);

            OnRemoveItem?.Invoke(item);
        }

        public void Update()
        {
            for(int i = 0; i < list.Count; i++)
            {
                if (!hash.Contains(list[i]))
                {
                    list.RemoveAt(i);
                    OnRemoveItem?.Invoke(list[i]);
                    i--;
                }

            }
        }

        public void Update(Predicate<T> predicate)
        {
            for(int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (!hash.Contains(item))
                {
                    list.RemoveAt(i);
                    i--;
                    OnRemoveItem?.Invoke(item);
                    continue;
                }

                if (!predicate(item))
                {
                    hash.Remove(item);
                    list.RemoveAt(i);
                    i--;
                    OnRemoveItem?.Invoke(item);
                }
            }
        }


        public void Remove(Predicate<T> predicate)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (!hash.Contains(item))
                {
                    list.RemoveAt(i);
                    i--;
                    OnRemoveItem?.Invoke(item);
                    continue;
                }

                if (predicate(list[i]))
                {
                    hash.Remove(item);
                    list.RemoveAt(i);
                    OnRemoveItem?.Invoke(item);
                    continue;
                }

            }

        }
        public List<T> ToList()
        {
            return list;
        }

        public T Item(int index)
        {
            return list[index];
        }

        public void Clear()
        {
            hash = new();
            for(int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                list.RemoveAt(i);
                OnRemoveItem?.Invoke(item);
                i--;
            }
        }

        public bool Contains(T item)
        {
            return hash.Contains(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
