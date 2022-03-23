using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Threading.Tasks;
using Architome.Enums;

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
            public WorkType workType;
            public WorkInfo station;

            public float workDone;
            public float workAmount;
            public float deltaWork;

            [Header("Task Rules")]
            public int maxWorkers;
            public bool canRepeat;
            public bool autoRepeat;
            public bool resetOnCancel;

            [Header("Effects")]
            public int taskAnimationID;
            public AudioClip workingSound;
            public AudioClip completionSound;


            //Private variables
        }

        [Serializable]
        struct TaskWorkers
        {
            public List<EntityInfo> working;
            public List<EntityInfo> onTheWay;

            public void Reset()
            {
                working = new();
                onTheWay = new();
            }

            public int Total()
            {
                return working.Count + onTheWay.Count;
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

            HandleWorkerEvents();
            HandleStationEvents();

            completionEvents.OnCompleted?.Invoke();

            void HandleWorkerEvents()
            {
                for (int i = 0; i < workers.working.Count; i++)
                {
                    workers.working[i].taskEvents.OnTaskComplete?.Invoke(new TaskEventData(this));
                }
            }

            void HandleStationEvents()
            {
                properties.station.taskEvents.OnTaskComplete?.Invoke(new TaskEventData(this));
            }


        }


        public bool CanStartWork(EntityInfo entity)
        {
            return false;
        }

        public bool CanWork(EntityInfo entity)
        {
            return states.currentState == TaskState.Available;
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
        public Action<TaskEventData> OnCancelMovingToTask;
    }
}