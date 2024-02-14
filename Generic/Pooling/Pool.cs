using Language.Lua;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class Pool
    {
        #region Common Data

        MonoActor source;

        public PoolItem item;
        public Queue<(PoolItem, GameObject)> itemQueue;
        public int startingObjects;
        public int maxObjects = 100;

        #endregion

        #region Initiation

        public void Start(MonoActor source)
        {
            this.source = source;
            itemQueue = new();

            for(int i = 0; i < startingObjects; i++)
            {
                AddItem();
            }
        }


        #endregion

        #region Common Functions

        (PoolItem, GameObject) AddItem()
        {
            var newItem = Object.Instantiate(item, source.transform);
            var gObject = newItem.gameObject;

            gObject.SetActive(false);

            var tuple = (newItem, gObject);

            itemQueue.Enqueue(tuple);
            return tuple;
        }

        public PoolItem Spawn()
        {
            if(itemQueue.Count == 0)
            {
                return AddItem().Item1;
            }
            else
            {
                var first = itemQueue.Dequeue();
                first.Item2.SetActive(true);
                return first.Item1;
            }
        }

        public void Return(PoolItem poolItem)
        {
            var poolObj = poolItem.gameObject;
            itemQueue.Enqueue((poolItem, poolObj));
            poolObj.SetActive(false);
            poolObj.transform.SetParent(source.transform);
        }

        #endregion
    }
}