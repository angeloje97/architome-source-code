using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Architome
{
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

            foreach(var task in tasks)
            {
                clickable.AddOption(task.workString);
            }
        }

        public void CreateTask(TaskInfo task)
        {
            tasks.Add(task);
            clickable.AddOption(task.workString);
        }

        public void OnSelectAction(Clickable eventData)
        {


            var task = Task(eventData.selectedString);

            if(task == null) { return; }

            if(task.currentState != Enums.TaskState.Available)
            {
                return;
            }

            var entities = eventData.clickedEntities;

            var newTaskEvent = new TaskEventData() {
                workers = entities,
                workInfo = this,
                task = task
            };

            for(int i = 0; i < task.maxWorkers; i++)
            {
                if(i >= entities.Count)
                {
                    break;
                }

                entities[i].TaskEvents().OnStartingTask?.Invoke(newTaskEvent);
            }
        }

        public void MoveEntity(EntityInfo entity)
        {
            if(workSpot == null)
            {
                entity.Movement().MoveTo(transform, 2);
                return;
            }


        }

        
        // Start is called before the first frame update
        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public TaskInfo Task(string taskString)
        {
            return tasks.Find(task => task.workString.Equals(taskString));
        }
    }

}