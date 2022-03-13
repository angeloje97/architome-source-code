using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Architome.Enums;

namespace Architome
{
    [Serializable]
    public class TaskInfo
    {
        public string workString;
        public WorkInfo station;
        
        public float currentWork;
        public float workAmount;

        public int maxWorkers;
        public List<EntityInfo> workers;
        public List<EntityInfo> workersOnTheWay;

        public bool canRepeat;
        public bool autoRepeat;
        public bool resetOnCancel;
        
        public WorkType workType;
        public TaskState currentState;
        


        public bool isBeingWorkedOn;


        public TaskInfo(WorkInfo station)
        {
            this.station = station;
            workType = WorkType.Use;
            currentState = TaskState.Available;
            maxWorkers = 1;
            currentWork = 0;
            workAmount = 5;
            canRepeat = true;
            resetOnCancel = true;

            workers = new List<EntityInfo>();
            workersOnTheWay = new List<EntityInfo>();
        }

        public void HandleWork()
        {
            if (isBeingWorkedOn) return;
            isBeingWorkedOn = true;
            WhileWorking();


        }

        public async void WhileWorking()
        {
            while(isBeingWorkedOn)
            {
                await Task.Yield();

                HandleWork();
                HandleWorkerEvents();
                HandleWorkerCount();


                
            }

            void HandleWork()
            {
                if(currentWork < workAmount)
                {
                    currentWork += Time.deltaTime;
                }
                else if(currentWork >= workAmount)
                {
                    currentWork = 0;
                    HandleComplete();
                }
            }

            void HandleWorkerEvents()
            {
                for (int i = 0; i < workers.Count; i++)
                {
                    workers[i].taskEvents.WhileWorkingOnTask?.Invoke(new TaskEventData(this));
                }
            }

            void HandleWorkerCount()
            {
                if (workers.Count == 0)
                {
                    isBeingWorkedOn = false;
                    HandleTaskCancel();
                }
            }
        }

        public void HandleTaskCancel()
        {
            if(resetOnCancel)
            {
                currentWork = 0;
            }
        }

        public void HandleComplete()
        {

            currentState = canRepeat ? TaskState.Available : TaskState.Done;
            isBeingWorkedOn = false;

            HandleWorkerEvents();
            HandleStationEvents();

            void HandleWorkerEvents()
            {
                for (int i = 0; i < workers.Count; i++)
                {
                    workers[i].taskEvents.OnTaskComplete?.Invoke(new TaskEventData(this));
                }
            }

            void HandleStationEvents()
            {
                station.taskEvents.OnTaskComplete?.Invoke(new TaskEventData(this));
            }

            

            
        }



        public bool CanStartWork(EntityInfo entity)
        {
            return false;
        }

        public bool CanWork(EntityInfo entity)
        {
            return currentState == TaskState.Available;
        }

        public int TotalWorkers()
        {
            return workers.Count + workersOnTheWay.Count;
        }

        public bool AddWorkerOnTheWay(EntityInfo entity)
        {
            if (!CanWork(entity)) return false;

            workersOnTheWay.Add(entity);

            UpdateState();
            return true;
        }

        public bool AddWorker(EntityInfo entity)
        {
            if(!workersOnTheWay.Contains(entity))
            {
                return false;
            }

            workersOnTheWay.Remove(entity);
            
            workers.Add(entity);
            HandleWork();
            UpdateState();

            return true;
        }

        public void RemoveWorker(EntityInfo entity)
        {


            if(workersOnTheWay.Contains(entity))
            {
                workersOnTheWay.Remove(entity);
                entity.taskEvents.OnCancelMovingToTask?.Invoke(new TaskEventData(this));
            }

            if(workers.Contains(entity))
            {
                workers.Remove(entity);
                entity.taskEvents.OnEndTask?.Invoke(new TaskEventData(this));
            }



            UpdateState();
        }

        public void UpdateState()
        {
            if(TotalWorkers() == maxWorkers)
            {
                currentState = TaskState.Occupied;
                return;
            }

            if(currentState == TaskState.Occupied)
            {
                currentState = TaskState.Available;
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