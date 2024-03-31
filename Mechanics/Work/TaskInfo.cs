using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Threading.Tasks;
using Architome.Enums;
using System.Linq;
using JetBrains.Annotations;

namespace Architome
{
    [Serializable]
    public class TaskInfo
    {
        #region Common Data

        #region Properties

        [Serializable]
        public struct TaskProperties
        {
            [Header("Task Information")]
            public string workString;
            public string completionMessage;
            public WorkType workType;
            public WorkInfo station;

            public float workDone;
            public float workAmount;
            public float deltaWork;

            [Header("Task Rules")]
            public int maxWorkers;
            public bool noHostileMobsInRoom;
            public bool noCombat;
            public bool canRepeat;
            public bool autoRepeat;
            public bool allowLinger;
            public bool damageCancelsTask;
            public bool hideFromPlayers;

            [Header("Progression Rules")]
            public bool noCompletion;
            public bool resetOnCancel;
            public bool fallsOnNoWorkers;
            public float percentPerSecond;
        }
        #endregion

        [Serializable]
        public class Effects
        {
            public int taskAnimationID;
            public int lingeringAnimationID;
            public AudioClip workingSound, completionSound;
        }

        [Serializable]
        struct TaskWorkers
        {
            public int maxWorkers;
            public List<EntityInfo> working;
            public List<EntityInfo> onTheWay;
            public List<EntityInfo> lingering;


            public void Reset()
            {
                working = new();
                onTheWay = new();
            }

            public int Total()
            {
                if (working == null) working = new();
                if (onTheWay == null) onTheWay = new();
                if (lingering == null) lingering = new();


                return working.Count + onTheWay.Count + lingering.Count;
            }
        }

        public TaskProperties properties;
        public Effects effects;

        #region Event Structs

        [Serializable]
        public struct TaskStates
        {
            public bool isBeingWorkedOn;
            public TaskState currentState;
        }
        [Serializable]
        struct TaskCompletionEvent
        {
            public UnityEvent<TaskEventData> OnCompleted;
            public UnityEvent<TaskEventData> OnStartWork;
            public UnityEvent<TaskEventData> OnFailed;
            public UnityEvent<TaskEventData> WhileBeingWorkedOn;
            public UnityEvent<TaskEventData> OnLingeringStart;
            public UnityEvent<TaskEventData> OnLingeringEnd;
        }

        [Serializable]
        struct TaskProgressEvents
        {
            public UnityEvent<float> OnProgressChange;
            public UnityEvent<TaskEventData> OnMaxProgress;
            public UnityEvent<TaskEventData> OnProgressDepleted;

        }

        [Serializable] struct WorkerEvents
        {
            public UnityEvent<TaskEventData, EntityInfo> OnNewWorker, OnNewWorkerOnTheWay, OnNewLingerer;
        }



        [SerializeField] TaskWorkers workers;
        [SerializeField] public TaskStates states;
        [SerializeField] WorkerEvents workerEvents;
        [SerializeField] TaskCompletionEvent completionEvents;
        [SerializeField] TaskProgressEvents progressEvents;
        
        #endregion

        //[SerializeField] TaskCompletionEvent events;

        [HideInInspector] public List<(bool, string)> checks;

        #endregion


        public void OnValidate()
        {
            //events = completionEvents;
        }

        public void Update()
        {
            properties.deltaWork = properties.workDone - previousWorkDone;
            if(previousWorkDone != properties.workDone)
            {

                progressEvents.OnProgressChange?.Invoke(properties.workDone / properties.workAmount);

                previousWorkDone = properties.workDone;
            }

        }

        //Private variables
        float previousWorkDone { get; set; }


        public List<EntityInfo> CurrentWorkers => workers.working;
        public List<EntityInfo> CurrentLingering => workers.lingering;
        public bool BeingWorkedOn => states.isBeingWorkedOn;


        public TaskInfo(WorkInfo station)
        {
            properties.station = station;
            properties.workType = WorkType.Use;
            states.currentState = TaskState.Available;
            properties.maxWorkers = 1;
            properties.workDone = 0;
            properties.workAmount = 5;
            properties.canRepeat = true;
            properties.resetOnCancel = true;

            workers.working = new List<EntityInfo>();
            workers.onTheWay = new List<EntityInfo>();
        }

        public TaskInfo(TaskProperties properties)
        {
            properties.maxWorkers = properties.maxWorkers == 0 ? 1 : properties.maxWorkers;

            this.properties = properties;

            states.currentState = TaskState.Available;

            workers.onTheWay = new();
            workers.working = new();
            
        }

        #region Main Process

        async void HandleWork(Action<TaskEventData> onSuccess)
        {
            if (states.isBeingWorkedOn) return;
            
            states.isBeingWorkedOn = true;
            var taskEventData = new TaskEventData(this);

            completionEvents.OnStartWork?.Invoke(taskEventData);
            properties.station.taskEvents.OnStartTask?.Invoke(taskEventData);
            
            var success = await WhileWorking();
            properties.station.taskEvents.OnEndTask?.Invoke(taskEventData);

            if (success)
            {
                onSuccess?.Invoke(taskEventData);
                HandleComplete();
                await Task.Delay(125);
                if (properties.canRepeat)
                {
                    properties.workDone = 0;
                }
                
            }
            else
            {
                HandleTaskCancel();
            }
        }

        #region Background Processes

        List<Func<Task<bool>>> backgroundProcesses;
        
        async Task<bool> HandleBackgroundProcesses()
        {
            foreach(var task in backgroundProcesses)
            {
                var result = await task();
                if (!result) return false;
            }

            return true;
        }

        public void AddBackgroundProcess(Func<Task<bool>> backgroundProcess)
        {
            backgroundProcesses ??= new();
            backgroundProcesses.Add(backgroundProcess);
        }
        #endregion
        public async Task<bool> WhileWorking()
        {
            var success = false;
            var fullProgress = false;
            backgroundProcesses = new();

            while(true)
            {
                if (!states.isBeingWorkedOn)
                {

                    break;
                }
                await Task.Yield();
                await HandleWork();
                HandleWorkerEvents();
                HandleWorkerCount();



                completionEvents.WhileBeingWorkedOn?.Invoke(new(this));


            }

            return success;

            async Task HandleWork()
            {
                if(properties.workDone < properties.workAmount)
                {
                    properties.workDone += World.deltaTime * workers.working.Count;

                    
                }
                else if(properties.workDone >= properties.workAmount)
                {
                    var result = await HandleBackgroundProcesses();

                    Debugger.System(4013, $"Result for background processes pass: {result}");
                    

                    if (!fullProgress)
                    {
                        fullProgress = true;
                        progressEvents.OnMaxProgress?.Invoke(new(this));
                    }

                    if (properties.noCompletion) return;
                    states.isBeingWorkedOn = false;
                    success = result;
                }
            }

            void HandleWorkerEvents()
            {
                for (int i = 0; i < workers.working.Count; i++)
                {
                    workers.working[i].taskEvents.WhileWorkingOnTask?.Invoke(new TaskEventData(this));
                }
            }

            void HandleWorkerCount()
            {
                if (workers.working.Count == 0)
                {
                    states.isBeingWorkedOn = false;
                    success = false;
                }
            }

        }

        public async Task UntilDone()
        {
            await ArchAction.WaitUntil(() => states.isBeingWorkedOn, false);
        }


        public void HandleTaskCancel()
        {
            if(properties.resetOnCancel)
            {
                properties.workDone = 0;
            }

            HandleFallsOnNoWorkers();


            async void HandleFallsOnNoWorkers()
            {
                if (!properties.fallsOnNoWorkers) return;

                while (!states.isBeingWorkedOn)
                {
                    await Task.Yield();
                    if (properties.workDone <= 0)
                    {
                        properties.workDone = 0f;

                        progressEvents.OnProgressDepleted?.Invoke(new(this));
                        return;
                    }

                    var valueFromPercent = properties.workAmount * properties.percentPerSecond;
                    properties.workDone -= World.deltaTime * valueFromPercent;

                }

            }
        }

        public void HandleComplete()
        {

            states.currentState = properties.canRepeat ? TaskState.Available : TaskState.Done;
            states.isBeingWorkedOn = false;
            var eventData = new TaskEventData(this);

            HandleWorkerEvents();
            HandleStationEvents();

            Debugger.Environment(0671, $"Workers {workers.working.Count}");
            completionEvents.OnCompleted?.Invoke(eventData);


            void HandleWorkerEvents()
            {
                for (int i = 0; i < workers.working.Count; i++)
                {
                    workers.working[i].taskEvents.OnTaskComplete?.Invoke(eventData);
                }
            }

            void HandleStationEvents()
            {
                properties.station.taskEvents.OnTaskComplete?.Invoke(eventData);
            }


        }

        #endregion

        public async void WhileLingering()
        {
            var taskEvent = new TaskEventData(this);
            completionEvents.OnLingeringStart?.Invoke(taskEvent);
            while (workers.lingering.Count > 0)
            {
                await Task.Yield();

                for (int i = 0; i < workers.lingering.Count; i++)
                {
                    var worker = workers.lingering[i];

                    bool remove = false;

                    if (!properties.station.IsOfWorkStation(worker.Target()))
                    {
                        remove = true;
                    }

                    if (remove)
                    {
                        worker.taskEvents.OnLingeringEnd?.Invoke(taskEvent);
                        workers.lingering.RemoveAt(i);
                        i--;

                        UpdateState();
                    }
                }
            }
            completionEvents.OnLingeringEnd?.Invoke(taskEvent);
        }

        bool HostileEntitiesInRoom()
        {
            var room = Entity.Room(properties.station.transform.position);

            if (room == null) return false;

            var hostileEntities = room.entities.HostilesInRoom;

            if (hostileEntities.Count > 0)
            {
                return true;
            }

            return false;
        }


        #region Worker Functions
        public bool CanStartWork(EntityInfo entity)
        {
            return false;
        }
        public bool CanWork(EntityInfo entity, bool invokeEvents = true)
        {
            TaskEventData eventData = new(this);
            var station = properties.station;

            if (!entity.isAlive)
            {
                if(invokeEvents)
                { 
                
                    InvokeCantWorkEvent("Cannot work on task when dead.");
                }
                return false;
            }

            if (states.currentState != TaskState.Available)
            {
                if (MaxWorkersReached())
                {

                    InvokeCantWorkEvent("Max workers reached");
                    return false;
                }

                InvokeCantWorkEvent("Task not available");
                return false;
            }

            if (properties.noCombat && entity.isInCombat)
            {
                InvokeCantWorkEvent("Must be out of combat");
                return false;
            }



            if (properties.noHostileMobsInRoom && HostileEntitiesInRoom())
            {
                InvokeCantWorkEvent("Hostile mobs are still in this room");
                return false;
            }

            return true;

            void InvokeCantWorkEvent(string reason)
            {
                if (!invokeEvents) return;
                entity.taskEvents.OnCantWorkOnTask?.Invoke(eventData, reason);
                station.taskEvents.OnCantWorkOnTask?.Invoke(eventData, reason);

            }
        }
        public void AddLingering(EntityInfo entity)
        {
            if (workers.lingering == null) workers.lingering = new();

            if (workers.lingering.Contains(entity)) return;

            workers.lingering.Add(entity);

            workerEvents.OnNewLingerer?.Invoke(new TaskEventData(this), entity);
        }
        public bool AddWorkerOnTheWay(EntityInfo entity)
        {
            if (workers.onTheWay == null)
            {
                workers.onTheWay = new();
            }

            if (!CanWork(entity)) return false;


            workers.onTheWay.Add(entity);
            var taskEvent = new TaskEventData(this);

            workerEvents.OnNewWorkerOnTheWay?.Invoke(taskEvent, entity);

            UpdateState();
            return true;
        }
        public bool AddWorker(EntityInfo entity, Action<TaskEventData> onSuccess)
        {
            if (workers.working == null) workers.working = new();

            if(!workers.onTheWay.Contains(entity))
            {
                return false;
            }



            workers.onTheWay.Remove(entity);
            
            workers.working.Add(entity);
            
            HandleWork((eventData) => {
                if (workers.working.Contains(entity)) onSuccess?.Invoke(eventData);
            });
            var taskEventData = new TaskEventData(this);
            workerEvents.OnNewWorker?.Invoke(taskEventData, entity);
            UpdateState();

            return true;
        }
        public void RemoveWorker(EntityInfo entity)
        {

            if (this == null) return;
            if (entity == null) return;
            


            if(workers.onTheWay != null && workers.onTheWay.Contains(entity))
            {
                workers.onTheWay.Remove(entity);
                entity.taskEvents.OnCancelMovingToTask?.Invoke(new TaskEventData(this));
            }

            if(workers.working != null &&  workers.working.Contains(entity))
            {
                workers.working.Remove(entity);
                entity.taskEvents.OnEndTask?.Invoke(new TaskEventData(this));
            }

            if (workers.lingering != null && workers.lingering.Contains(entity))
            {
                workers.lingering.Remove(entity);
            }


            UpdateState();
        }
        public void RemoveAllWorkers()
        {
            foreach(var entity in workers.working)
            {
                var taskHandler = entity.TaskHandler();
                taskHandler.StopTask(this);

            }
        }

        public void RemoveAllLingerers()
        {
            foreach (var entity in workers.lingering)
            {
                if (entity.Movement())
                {
                    entity.Movement().StopMoving(true);
                }
            }
        }
        public bool MaxWorkersReached()
        {
            return workers.Total() >= properties.maxWorkers;
        }
        #endregion

        public void UpdateState()
        {
            if(workers.Total() == properties.maxWorkers)
            {
                states.currentState = TaskState.Occupied;
                return;
            }

            if(states.currentState == TaskState.Occupied)
            {
                states.currentState = TaskState.Available;
            }
        }


    }

    public class TaskEventData
    {
        public TaskInfo task;
        public WorkInfo workInfo;

        public TaskEventData(TaskInfo task)
        {
            this.task = task;
            workInfo = task.properties.station;

        }

        public float WorkPercent()
        {
            return task.properties.workDone / task.properties.workAmount;
        }

        public bool TaskComplete { 
            get { return task.properties.workAmount <= task.properties.workDone; } 
        }

    }

    [SerializeField]
    public struct TaskEvents
    {
        //For Tasks
        public Action<TaskEventData> OnStartTask;
        public Action<TaskEventData> WhileWorkingOnTask;
        public Action<TaskEventData> OnTaskComplete;

        //For Entities
        public Action<TaskEventData> OnMoveToTask { get; set; }
        public Action<TaskInfo, TaskInfo> OnNewTask { get; set; }
        public Action<TaskEventData> OnEndTask { get; set; }
        public Action<TaskEventData> OnLingeringStart { get; set; }
        public Action<TaskEventData> OnLingeringEnd;
        public Action<TaskEventData> OnCancelMovingToTask;
        public Action<TaskEventData, string> OnCantWorkOnTask;

    }
}