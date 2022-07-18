using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Architome
{
    [RequireComponent(typeof(Clickable))]
    public class WorkInfo : MonoBehaviour
    {
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
                SetClickable();
            }

            workStationsDebugging = workStations;
        }
        void Start()
        {
            GetDependencies();

        }

        private void Awake()
        {
            HandleGlobal(true);
        }

        private void OnDestroy()
        {
            HandleGlobal(false);
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

        public void SetClickable()
        {
            clickable.ClearOptions();

            if(tasks.Count == 0) { return; }

            foreach(var task in tasks)
            {
                if (task.properties.hideFromPlayers) continue;
                clickable.AddOption(task.properties.workString);
            }
        }

        public void CreateTask(TaskInfo task)
        {
            task.properties.station = this;

            tasks.Add(task);


            
            clickable.AddOption(task.properties.workString);
            
        }

        public void OnSelectAction(Clickable eventData)
        {
            var task = Task(eventData.selectedString);

            if(task == null) { return; }

            var entities = eventData.clickedEntities;

            if (entities.Count == 0) return;

            foreach (var entity in entities)
            {
                entity.TaskHandler().StartWork(task);
            }
        }
        
        
        // Start is called before the first frame update

        private void OnValidate()
        {
            foreach(var task in tasks)
            {
                task.properties.station = this;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public TaskInfo Task(string taskString)
        {
            return tasks.Find(task => task.properties.workString.Equals(taskString));
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

        public void RemoveAllLingers()
        {
            foreach (var task in tasks)
            {
                task.RemoveAllLingerers();
            }
        }
    }

}