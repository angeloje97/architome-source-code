using DungeonArchitect.Samples.ShooterGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class LRUCache<T>
    {
        Dictionary<T, LinkedListNode<T>> hashes;
        LinkedList<T> list;
        int capacity = -1;


        public LRUCache(int capacity)
        {
            this.capacity = capacity;
            list = new();
            hashes = new();
        }

        public T Pop()
        {
            var last = list.Last.Value;
            list.RemoveLast();
            return last;
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
