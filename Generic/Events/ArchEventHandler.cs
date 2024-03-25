using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Architome.Events
{
    [Serializable]
    public class ArchEventHandler<T, E>  where T: Enum
    {
        #region Common Data
        [SerializeField] List<EventItem> eventItems;
        Dictionary<T, List<EventItem>> subsets;
        Component source;
        MonoActor actor;
        Dictionary<T, Action<E>> eventDict;

        List<Func<Task>> tasksToFinish { get; set; }
        List<bool> checks;
        bool doingTasks;
        LogicType defaultLogic;
        #endregion

        #region Initialization
        public ArchEventHandler(Component source, LogicType defaultLogic = LogicType.NoFalse)
        {
            this.source = source;
            eventDict ??= new();
            this.defaultLogic = defaultLogic;



            CreateSubsets();
        }

        public ArchEventHandler(MonoActor actor, LogicType defaultLogic = LogicType.NoFalse)
        {
            this.actor = actor;
            this.source = actor;
            eventDict ??= new();
            this.defaultLogic = defaultLogic;

            CreateSubsets();
        }

        void CreateSubsets()
        {
            eventItems ??= new();
            subsets ??= new();

            foreach (var item in eventItems)
            {
                if (!subsets.ContainsKey(item.trigger))
                {
                    subsets.Add(item.trigger, new());
                }

                subsets[item.trigger].Add(item);

                AddListener(item.trigger, (E data) => {
                    item.action?.Invoke(data);
                }, source);
            }
        }
        #endregion

        #region Listener

        public Action AddListener(T eventType, Action<E> action, MonoActor actor, bool listenToActor = true)
        {

            if (!eventDict.ContainsKey(eventType))
            {
                eventDict.Add(eventType, null);
            }

            Action unsubscribe = () => { };

            var unsubscribed = false;

            Action unsubScribeFromListener = () => { };

            if (listenToActor && actor != source)
            {
                unsubScribeFromListener = actor.AddListener(eMonoEvent.OnDestroy, unsubscribe, this.actor);

            }

            eventDict[eventType] += MiddleWare;

            unsubscribe += () => {
                if (unsubscribed) return;
                unsubscribed = true;

                eventDict[eventType] -= MiddleWare;
                unsubScribeFromListener();
            };

            Debugger.System(1042, $"Successfully added listener for {actor}");

            return unsubscribe;

            void MiddleWare(E data)
            {
                action(data);
            }
        }

        public Action AddListener(T eventType, Action action, MonoActor actor, bool listenToActor = true)
        {
            return AddListener(eventType, (E eventData) => { action(); }, actor, listenToActor);
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

        public Action AddListenerLimit(T eventType, Action<E> action, Component listener, int count = 1)
        {
            Action unsubscribe = () => { };
            var currentCount = 0;
            unsubscribe = AddListener(eventType, MiddleWare, listener);

            return unsubscribe;

            void MiddleWare(E data)
            {
                action(data);
                currentCount++;
                if (currentCount >= count) unsubscribe();
            }
        }

        public Action AddListenerLimit(T eventType, Action action, Component listener, int count = 1)
        {
            return AddListenerLimit(eventType, (E e) => { action(); }, listener, count);
        }

        public Action AddListenerInterval(T eventType, Action<E> action, Component listener, int interval = 1)
        {
            var current = 0;
            return AddListener(eventType, (E data) => {
                current++;

                if (current < interval) return;
                current = 0;
                action(data);

            }, listener);
        }

        public Action AddListenerInterval(T eventType, Action action, Component listener, int interval)
        {
            return AddListenerInterval(eventType, (E data) => {
                action();
            }, listener, interval);
        }

        public Action AddListenerTask(T eventType, Action<E, List<Func<Task>>> action, Component listener)
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

        public Action AddListenerCheck(T eventType, Action<E, List<bool>> action, MonoActor listener)
        {
            return AddListener(eventType, (E data) => {
                action(data, checks);
            }, listener);
        }

        public Action AddListenerCheck(T eventType, Action<E, List<bool>, List<Func<Task>>> action, MonoActor listener)
        {
            return AddListener(eventType, (E data) => {
                action(data, checks, tasksToFinish);
            }, listener);
        }


        public Action AddListenerPredicate(T eventType, Predicate<E> action, Component listener)
        {
            return AddListener(eventType, (E data) => {
                if (checks == null) return;
                checks.Add(action(data));
            }, listener);
        }
        #endregion

        #region Invoker

        public List<Func<Task>> Invoke(T eventType, E eventData)
        {
            tasksToFinish = new();
            if (source == null) return tasksToFinish.ToList();
            if (!eventDict.ContainsKey(eventType)) return tasksToFinish.ToList();

            eventDict[eventType]?.Invoke(eventData);

            return tasksToFinish.ToList();
        }

        public async Task InvokeAsyncParallel(T eventType, E eventData)
        {
            var functions = Invoke(eventType, eventData);
            
            foreach(var func in functions)
            {
                await func();
            }
        }

        public async Task InvokeAsyncSeq(T eventType, E eventData)
        {
            var functions = Invoke(eventType, eventData);

            var tasks = new List<Task>();

            foreach(var func in functions)
            {
                tasks.Add(func());
            }

            await Task.WhenAll(tasks);
        }

        public bool InvokeCheck(T eventType, E eventData)
        {
            checks = new();
            Invoke(eventType, eventData);

            return new ArchLogic(checks).Valid(defaultLogic);
        }

        public async Task<bool> UntilInvokeCheck(T eventType, E eventData, TaskType taskType)
        {
            checks = new();
            tasksToFinish = new();
            Invoke(eventType, eventData);

            await tasksToFinish.HandleTasks(taskType);

            return new ArchLogic(checks).Valid(defaultLogic);
        }

        public bool InvokeCheck(T eventType, E eventData, LogicType logicType)
        {
            checks = new();
            Invoke(eventType, eventData);

            return new ArchLogic(checks).Valid(logicType);
        }

        public bool InvokeCheck(T eventType, E eventData, bool targetValue, LogicType logicType)
        {
            checks = new();
            Invoke(eventType, eventData);

            return checks.ValidateLogic(targetValue, logicType);
        }

        #endregion

        #region Sub Classes
        [Serializable]
        public class EventItem
        {
            public T trigger;
            public UnityEvent<E> action;
        }
        #endregion

    }
}
