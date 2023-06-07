using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;
using System.Linq;
using UnityEditor;

namespace Architome
{
    [RequireComponent(typeof(Clickable))]
    [RequireComponent(typeof(WorkFX))]
    public class WorkInfo : MonoBehaviour
    {
        public string workName;

        [SerializeField] bool updateTasks;
        public static List<WorkInfo> workStations { get; set; }
        List<WorkInfo> workStationsDebugging;

        public List<TaskInfo> tasks;

        Clickable clickable;

        public TaskEvents taskEvents;

        public Transform workSpot;


        void GetDependencies()
        {
            if (GetComponent<Clickable>())
            {
                clickable = GetComponent<Clickable>();
                clickable.OnSelectOption += OnSelectAction;
                UpdateClickable();
            }

            workStationsDebugging = workStations;
        }
        void Start()
        {
            GetDependencies();
            HandleCount();

            updateTasks = true;
            UpdateTasks();

            taskEvents.OnTaskComplete += HandleTaskComplete;
        }

        private void Awake()
        {
            HandleGlobal(true);
        }

        private void OnDestroy()
        {
            HandleGlobal(false);
        }

        void OnValidate()
        {
            if (workName == null || workName.Trim().Length == 0)
            {
                workName = name;
            }
            UpdateTasks();
        }

        void Update()
        {
            if (tasks == null) return;
            foreach (var task in tasks)
            {
                task.Update();
            }
        }

        void UpdateTasks()
        {
            if (!updateTasks) return;
            updateTasks = false;

            tasks ??= new();

            foreach (var task in tasks)
            {
                task.properties.station = this;
                task.OnValidate();
            }
        }

        

        void HandleTaskComplete(TaskEventData eventData)
        {
            ArchAction.Yield(() => {
                UpdateTaskClickables();
            });
        }

        void UpdateTaskClickables()
        {
            foreach (var task in tasks)
            {
                if (task.states.currentState == TaskState.Available) continue;
                for (int i = 0; i < clickable.options.Count; i++)
                {
                    if (clickable.options[i].text == task.properties.workString)
                    {
                        clickable.options.RemoveAt(i);
                    }
                }
            }
        }

        void HandleCount()
        {
        }

        void HandleGlobal(bool adding)
        {
            if (workStations == null) workStations = new();
            var included = false;
            for (int i = 0; i < workStations.Count; i++)
            {
                var station = workStations[i];
                var remove = false;

                if (station == null)
                {
                    remove = true;
                }
                
                if (station == this)
                {
                    included = true;

                    if (!adding)
                    {
                        remove = true;
                    }
                }

                if (remove)
                {
                    workStations.RemoveAt(i);
                    i--;
                }
            }

            if (!included && adding)
            {
                workStations.Add(this);
            }
        }

        public void UpdateClickable()
        {
            if(tasks.Count == 0) { return; }
            var set = tasks.Select(task => task.properties.workString).ToHashSet();

            clickable.RemoveOptions((Clickable.Option option) => set.Contains(option.text));


            foreach(var task in tasks)
            {
                if (task.properties.hideFromPlayers) continue;
                clickable.AddOption(task.properties.workString);
            }
        }

        public int CreateTask(TaskInfo task)
        {
            task.properties.station = this;

            var index = tasks.Count;
            tasks.Add(task);

            clickable.AddOption(task.properties.workString);
            return index;   
        }

        public void OnSelectAction(Clickable eventData)
        {
            var task = Task(eventData.selectedString);

            if(task == null) { return; }

            var entities = eventData.clickedEntities.ToList();

            if (entities.Count == 0) return;

            int workersSet = 0;

            foreach (var entity in entities)
            {
                if(entities.Count == 1)
                {
                    entity.TaskHandler().AddTask(task, true);
                }
                else if(workersSet > 0 && task.MaxWorkersReached())
                {
                    return;
                }
                else if(entity.isAlive)
                {
                    entity.TaskHandler().AddTask(task, true);
                    workersSet++;
                }
            }
        }
        
        public void HideStationFromPlayers()
        {
            if (tasks == null) return;
            foreach(var task in tasks)
            {
                task.properties.hideFromPlayers = true;
            }
        }

        public void HideTaskFromPlayer(int taskIndex)
        {
            if (taskIndex >= tasks.Count) return;
            tasks[taskIndex].properties.hideFromPlayers = true;
            UpdateClickable();
        }

        public void ShowTaskToPlayer(int taskIndex)
        {
            if (taskIndex >= tasks.Count) return;
            tasks[taskIndex].properties.hideFromPlayers = false;
            UpdateClickable();
        }


        public TaskInfo Task(string taskString)
        {
            return tasks.Find(task => task.properties.workString.Equals(taskString));
        }

        public List<EntityInfo> Workers()
        {
            var workers = new List<EntityInfo>();

            foreach (var task in tasks)
            {
                foreach (var worker in task.CurrentWorkers)
                {
                    workers.Add(worker);
                }
            }

            return workers;
        }

        public bool IsOfWorkStation(Transform tran)
        {
            if (tran == null) return false;
            if (tran == transform) return true;

            var workInfo = tran.GetComponentInParent<WorkInfo>();

            if (workInfo && workInfo == this)
            {
                return true;
            }

            return false;
        }

        public Transform StationTarget()
        {
            if (workSpot == null)
            {
                return transform;
            }

            return workSpot;
        }

        public void RemoveAllLingers()
        {
            foreach (var task in tasks)
            {
                task.RemoveAllLingerers();
            }
        }
    }

}