using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class ETaskHandler : EntityProp
{
        // Start is called before the first frame update
        public TaskInfo currentTask;
        private TaskInfo previousTask;
        void Start()
        {
            GetDependencies();
        }
        public new void GetDependencies()
        {
            base.GetDependencies();

            if(entityInfo.Movement())
            {
                entityInfo.Movement().OnChangePath += OnChangePath;
            }

            entityInfo.TaskEvents().OnStartingTask += OnStartingTask;
            entityInfo.TaskEvents().OnNewTask += OnNewTask;
        }

        // Update is called once per frame
        void Update()
        {
            HandleEvents();
        }

        public void HandleEvents()
        {
            if(previousTask != currentTask)
            {
                entityInfo.TaskEvents().OnNewTask?.Invoke(previousTask, currentTask);

                previousTask = currentTask;
            }
        }


        public void OnStartingTask(TaskEventData eventData)
        {
            if(eventData.workInfo == null) { return; }
            var workInfo = eventData.workInfo;
            currentTask = eventData.task;

            currentTask.workersOnTheWay.Add(entityInfo);
            workInfo.MoveEntity(entityInfo);



        }

        public void OnNewTask(TaskInfo previous, TaskInfo current)
        {
            if(previous != null)
            {
                if(previous.workersOnTheWay.Contains(entityInfo))
                {
                    previous.workersOnTheWay.Remove(entityInfo);
                }
            }
        }

        public void OnChangePath(Movement movement)
        {
            if(currentTask != null && currentTask.workersOnTheWay.Contains(entityInfo))
            {
                currentTask.workersOnTheWay.Remove(entityInfo);
            }
        }


    }

}
