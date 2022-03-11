using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
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
        public bool resetOnCancel;
        
        public WorkType workType;
        public TaskState currentState;

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

            UpdateState();

            return true;
        }

        public void RemoveWorker(EntityInfo entity)
        {


            if(workersOnTheWay.Contains(entity))
            {
                workersOnTheWay.Remove(entity);
            }

            if(workers.Contains(entity))
            {
                workers.Remove(entity);
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
        public List<EntityInfo> workers;
        public WorkInfo workInfo;
        public TaskInfo task;
    }

    [SerializeField]
    public class TaskEvents
    {

        public Action<TaskEventData> OnMoveToTask;
        public Action<TaskEventData> OnStartTask;
        public Action<TaskEventData> WhileWorkingOnTask;
        public Action<TaskEventData> OnTaskComplete;

        //For Entities
        public Action<TaskInfo, TaskInfo> OnNewTask;
        public Action<TaskInfo> OnCancelTask;
    }
}