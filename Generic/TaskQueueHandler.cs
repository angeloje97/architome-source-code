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
        public bool busy { get; private set; }

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



        public async Task UntilTasksFinished()
        {
            while (busy)
            {
                await Task.Yield();
            }
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
