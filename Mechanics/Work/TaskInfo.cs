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
        
        public float currentWork;
        public float workAmount;

        public int maxWorkers;
        public List<EntityInfo> workers;
        public List<EntityInfo> workersOnTheWay;

        public bool canRepeat;
        public bool resetOnCancel;
        
        public WorkType workType;
        public TaskState currentState;

        public TaskInfo()
        {
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
            return false;
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
        
        public Action<TaskEventData> OnStartingTask;
        public Action<TaskEventData> OnStartTask;
        public Action<TaskEventData> WhileWorkingOnTask;
        public Action<TaskEventData> OnTaskComplete;

        //For Entities
        public Action<TaskInfo, TaskInfo> OnNewTask;
        public Action<TaskInfo> OnCancelTask;
    }
}