using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public class ArchEventHandler<T, E>  where T: Enum
    {
        Component source;
        Dictionary<T, Action<E>> eventDict;

        List<Func<Task>> tasksToFinish;
        List<bool> checks;
        bool doingTasks;
        LogicType defaultLogic;
        public ArchEventHandler(Component source, LogicType defaultLogic = LogicType.NoFalse)
        {
            this.source = source;
            eventDict = new();
            this.defaultLogic = defaultLogic;
        }

        public Action AddListener(T eventType, Action<E> action, Component listener)
        {
            if (source == null) return () => { };
            if (!eventDict.ContainsKey(eventType))
            {
                eventDict.Add(eventType, null);
            }

            bool unsubscribed = false;
            eventDict[eventType] += MiddleWare;


            return Unsubscribe;

            void MiddleWare(E eventData)
            {
                if(listener == null)
                {
                    Unsubscribe();
                    return;
                }

                action(eventData);
            }

            void Unsubscribe()
            {
                if (unsubscribed) return;
                unsubscribed = true;
                if (source == null) return;

                eventDict[eventType] -= MiddleWare;
            }
        }


        public Action AddListener(T eventType, Action action, Component listener)
        {
            return AddListener(eventType, (E e) => {
                action();
            }, listener);
        }

        public Action AddListener(T eventType, Action<E, List<Func<Task>>> action, Component listener)
        {
            return AddListener(eventType, (E data) =>
            {
                action(data, tasksToFinish);
            }, listener);
        }

        public Action AddListeners(List<(T, Action<E>)> values, Component listener)
        {
            Action unsubscribe = () => { };

            foreach (var value in values)
            {
                var type = value.Item1;
                var action = value.Item2;

                unsubscribe += AddListener(type, action, listener);
            }

            return unsubscribe;
        }

        public Action AddListenerCheck(T eventType, Action<E, List<bool>> action, Component listener)
        {
            return AddListener(eventType, (E data) => {
                action(data, checks);
            }, listener);
        }

        public Action AddListenerPredicate(T eventType, Predicate<E> action, Component listener)
        {
            return AddListener(eventType, (E data) => {
                if (checks == null) return;
                checks.Add(action(data));
            }, listener);
        }

        public List<Func<Task>> Invoke(T eventType, E eventData)
        {
            tasksToFinish = new();
            if (source == null) return tasksToFinish.ToList();
            if (!eventDict.ContainsKey(eventType)) return tasksToFinish.ToList();
            eventDict[eventType]?.Invoke(eventData);

            return tasksToFinish.ToList();
        }

        public bool InvokeCheck(T eventType, E eventData)
        {
            checks = new();
            Invoke(eventType, eventData);

            return new ArchLogic(checks).Valid(defaultLogic);
        }

        public bool InvokeCheck(T eventType, E eventData, LogicType logicType)
        {
            checks = new();
            Invoke(eventType, eventData);

            return new ArchLogic(checks).Valid(logicType);
        }

    }
}
