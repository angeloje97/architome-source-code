using DungeonArchitect.Samples.ShooterGame;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Architome
{
    public class LRUCache<T>
    {
        Dictionary<T, LinkedListNode<T>> hashes;
        LinkedList<T> list;
        int capacity = -1;

        public T Last => list.Last.Value;
        public T First => list.First.Value;
        public int Count => list.Count;


        public LRUCache(int capacity)
        {
            this.capacity = capacity;
            list = new();
            hashes = new();
        }

        public IEnumerator<T> GetEnumerator()
        {
            var current = list.First;

            while(current != null)
            {
                yield return current.Value;
                current = current.Next;
            }
        }

        public T Pop(T defaultValue)
        {
            if(list.Count > 0)
            {
                var last = list.Last.Value;
                list.RemoveLast();
                return last;
            }
            return defaultValue;
        }

        public bool Contains(T item)
        {
            if (hashes.ContainsKey(item))
            {
                var node = hashes[item];

                if(node.List != null)
                {
                    return true;
                }
                else
                {
                    hashes.Remove(item);
                }
            }
            return false;
        }

        public void Remove(T item)
        {
            if (hashes.ContainsKey(item))
            {
                var node = hashes[item];
                if (node.List == null) return;
                hashes.Remove(item);
                list.Remove(item);
            }
        }


        public void Put(T item)
        {
            if (hashes.ContainsKey(item))
            {
                var node = hashes[item];

                if (node.List != null)
                {
                    list.Remove(node);
                    list.AddLast(node);
                    node.Value = item;
                    return;
                }

                hashes.Remove(item);
            }

            if(capacity != -1)
            {
                list.RemoveLast();
            }

            var newNode = list.AddFirst(item);
            hashes.Add(item, newNode);
        }


    }
}
