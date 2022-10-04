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
        public bool preventUntilStart;
        
        
        void Start()
        {
            HandleStart();
            ArchAction.Delay(() => HandlePreventUntilStart(), 1f);
            HandleTrailEmission();
        }

        void HandlePreventUntilStart()
        {
            if (!preventUntilStart) return;

            var clickable = workInfoTarget.GetComponent<Clickable>();

            for(int i = 0; i < clickable.options.Count; i++)
            {
                var option = clickable.options[i];
                if (option.text != taskWorkString) continue;

                var index = i;
                OnStartEvent += delegate (EventListener listener)
                {
                    clickable.options.Insert(index, new() { text = taskWorkString });
                };

                clickable.options.RemoveAt(i);
                i--;
            }

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
                
            };

            if (simple)
            {
                stringList.Add($"Use the {taskWorkString} action on {workInfoTarget.workName}.");
            }
            else
            {
                stringList.Add($"Use the move action for a party member on {workInfoTarget.workName} and have them work on the task '{taskWorkString}'");
            }

            return ArchString.NextLineList(stringList);
        }


        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
