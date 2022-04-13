using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Threading.Tasks;
using Architome.Enums;
using System.Linq;

namespace Architome
{
    [Serializable]
    public class TaskInfo
    {
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
            public bool resetOnCancel;

            [Header("Effects")]
            public int taskAnimationID;
            public int lingeringAnimationID;
            public AudioClip workingSound;
            public AudioClip completionSound;


            //Private variables
        }

        [Serializable]
        struct TaskWorkers
        {
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
                return working.Count + onTheWay.Count + lingering.Count;
            }
        }

        [Serializable]
        public struct TaskStates
        {
            public bool isBeingWorkedOn;
            public TaskState currentState;
        }

        [Serializable]
        struct TaskCompletionEvent
        {
            public UnityEvent OnCompleted;
            public UnityEvent OnFailed;
        }

        public TaskProperties properties;
        [SerializeField]
        TaskWorkers workers;
        [SerializeField]
        public TaskStates states;
        [SerializeField]
        TaskCompletionEvent completionEvents;

        //Private variables
        float previousWorkDone;



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


        async void HandleWork()
        {
            if (states.isBeingWorkedOn) return;
            
            states.isBeingWorkedOn = true;

            properties.station.taskEvents.OnStartTask?.Invoke(new TaskEventData(this));
            var success = await WhileWorking();

            properties.station.taskEvents.OnEndTask?.Invoke(new TaskEventData(this));

            properties.deltaWork = 0;
            

            if (success)
            {
                HandleComplete();
                await Task.Delay(125);
                properties.workDone = 0;
            }
            else
            {
                HandleTaskCancel();
            }
        }

        public async Task<bool> WhileWorking()
        {
            var success = false;

            while(states.isBeingWorkedOn)
            {
                await Task.Yield();

                HandleWork();
                HandleWorkerEvents();
                HandleWorkerCount();
            }

            return success;

            void HandleWork()
            {
                if(properties.workDone < properties.workAmount)
                {
                    properties.workDone += Time.deltaTime * workers.working.Count;

                    properties.deltaWork = properties.workDone - previousWorkDone;

                    previousWorkDone = properties.workDone;
                    
                }
                else if(properties.workDone >= properties.workAmount)
                {
                    states.isBeingWorkedOn = false;
                    success = true;
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

        public void HandleTaskCancel()
        {
            if(properties.resetOnCancel)
            {
                properties.workDone = 0;
            }
        }

        public void HandleComplete()
        {

            states.currentState = properties.canRepeat ? TaskState.Available : TaskState.Done;
            states.isBeingWorkedOn = false;
            var eventData = new TaskEventData(this);

            HandleLingering();
            HandleWorkerEvents();
            HandleStationEvents();

            completionEvents.OnCompleted?.Invoke();

            void HandleLingering()
            {
                if (!properties.allowLinger) return;
                workers.lingering = workers.working.ToList();

                ArchAction.Delay(() => {

                    foreach (var worker in workers.lingering)
                    {
                        worker.taskEvents.OnLingeringStart?.Invoke(eventData);
                    }

                    WhileLingering();

                }, .0625f);
                
            }

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


        public async void WhileLingering()
        {
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
                        worker.taskEvents.OnLingeringEnd?.Invoke(new TaskEventData(this));
                        workers.lingering.RemoveAt(i);
                        i--;

                        UpdateState();
                    }
                }
            }

        }

        public bool CanStartWork(EntityInfo entity)
        {
            return false;
        }

        public bool CanWork(EntityInfo entity)
        {
            TaskEventData eventData = new(this);
            if (states.currentState != TaskState.Available)
            {
                entity.taskEvents.OnCantWorkOnTask?.Invoke(eventData, "Task not available");
                properties.station.taskEvents.OnCantWorkOnTask?.Invoke(eventData, "Task not available");
                return false;
            }

            if (properties.noCombat && entity.isInCombat)
            {
                entity.taskEvents.OnCantWorkOnTask?.Invoke(eventData, "Must be out of combat");
                properties.station.taskEvents.OnCantWorkOnTask?.Invoke(eventData, "Must be out of combat");
                return false;
            }



            if (HostileEntitiesInRoom() && properties.noHostileMobsInRoom)
            {
                entity.taskEvents.OnCantWorkOnTask?.Invoke(eventData, "Hostile mobs are still in this room");
                properties.station.taskEvents.OnCantWorkOnTask?.Invoke(eventData, "Hostile mobs are still in this room");
                return false;
            }

            return true;
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

        public bool AddWorkerOnTheWay(EntityInfo entity)
        {
            if (workers.onTheWay == null)
            {
                workers.onTheWay = new();
            }

            if (!CanWork(entity)) return false;


            workers.onTheWay.Add(entity);

            UpdateState();
            return true;
        }

        public bool AddWorker(EntityInfo entity)
        {
            if (workers.working == null) workers.working = new();

            if(!workers.onTheWay.Contains(entity))
            {
                return false;
            }



            workers.onTheWay.Remove(entity);
            
            workers.working.Add(entity);
            HandleWork();
            UpdateState();

            return true;
        }

        public void RemoveWorker(EntityInfo entity)
        {


            if(workers.onTheWay.Contains(entity))
            {
                workers.onTheWay.Remove(entity);
                entity.taskEvents.OnCancelMovingToTask?.Invoke(new TaskEventData(this));
            }

            if(workers.working.Contains(entity))
            {
                workers.working.Remove(entity);
                entity.taskEvents.OnEndTask?.Invoke(new TaskEventData(this));
            }

            UpdateState();
        }

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

        public TaskEventData(TaskInfo task)
        {
            this.task = task;
        }

        public bool TaskComplete { 
            get { return task.properties.workAmount <= task.properties.workDone; } 
        }

    }

    [SerializeField]
    public struct TaskEvents
    {

        public Action<TaskEventData> OnMoveToTask;
        public Action<TaskEventData> OnStartTask;
        public Action<TaskEventData> WhileWorkingOnTask;
        public Action<TaskEventData> OnTaskComplete;

        //For Entities
        public Action<TaskInfo, TaskInfo> OnNewTask;
        public Action<TaskEventData> OnEndTask;
        public Action<TaskEventData> OnLingeringStart;
        public Action<TaskEventData> OnLingeringEnd;
        public Action<TaskEventData> OnCancelMovingToTask;
        public Action<TaskEventData, string> OnCantWorkOnTask;

    }
}