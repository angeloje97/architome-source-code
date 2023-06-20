using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public class TaskQueueHandler
    {
        Queue<Func<Task>> currentTasks;
        Task activeTask;
        bool busy;

        public TaskQueueHandler()
        {
            currentTasks = new();
        }

        public async void AddTask(Func<Task> task)
        {
            currentTasks.Enqueue(task);
            if (busy) return;
            busy = true;

            while(currentTasks.Count > 0)
            {
                var nextTask = currentTasks.Dequeue();

                activeTask = nextTask();

                await activeTask;
            }

            busy = false;
        }

        public void AddTaskImmediate(Func<Task> task)
        {
            ClearTasks();
            AddTask(task);
        }

        public void ClearTasks()
        {
            currentTasks = new();
            busy = false;
        }
    }
}
