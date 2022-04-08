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
        public static List<WorkInfo> workObjects;

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

            if (workObjects == null)
            {
                workObjects = new List<WorkInfo>();
            }
            workObjects.Add(this);

        }

        public void SetClickable()
        {
            clickable.ClearOptions();

            if(tasks.Count == 0) { return; }

            foreach(var task in tasks)
            {
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
        void Start()
        {
            GetDependencies();
        }

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
    }

}