using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.Tutorial
{
    public class TaskListener : EventListener
    {
        [Header("Task Properties")]
        public WorkInfo workInfoTarget;
        public string taskWorkString;
        public int taskIndex;
        
        void Start()
        {
            HandleStart();
        }

        private void OnValidate()
        {
            if (workInfoTarget == null) return;
            var tasks = workInfoTarget.tasks;

            if (tasks == null) return;
            if (taskIndex < 0 || taskIndex >= tasks.Count) return;

            taskWorkString = tasks[taskIndex].properties.workString;
        }

        public override void StartEventListener()
        {
            base.StartEventListener();

            var targetTask = workInfoTarget.tasks.Find(task => task.properties.workString.Equals(taskWorkString));
            
            Action<TaskEventData> action = (TaskEventData eventData) => {
                if (eventData.task != targetTask) return;
                CompleteEventListener();
            };

            workInfoTarget.taskEvents.OnTaskComplete += action;

            OnSuccessfulEvent += (EventListener listener) => {
                workInfoTarget.taskEvents.OnTaskComplete -= action;
            };

            
        }

        public override string Directions()
        {
            var stringList = new List<string>() {
                base.Directions(),
                $"Use the move action for a party member on ${workInfoTarget.workName} and click {taskWorkString} to have them work on it."
            };

            return ArchString.NextLineList(stringList);
        }


        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
