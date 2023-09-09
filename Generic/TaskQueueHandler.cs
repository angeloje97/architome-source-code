
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public enum TaskType
    {
        Parallel,
        Sequential
    }
    public class TaskQueueHandler
    {
        Queue<Func<Task>> currentTasks;
        List<Task> currentTasksList;
        Task activeTask;
        public bool busy { get; private set; }

        TaskType type;

        public TaskQueueHandler(TaskType type)
        {
            this.type = type;
            currentTasks = new();
            currentTasksList = new();
        }

        public void AddTask(Func<Task> task)
        {
            HandleSequential();
            HandleParallel();

            async void HandleSequential()
            {
                if (type != TaskType.Sequential) return;
                currentTasks.Enqueue(task);
                if (busy) return;
                busy = true;
                while (currentTasks.Count > 0)
                {
                    var nextTask = currentTasks.Dequeue();

                    activeTask = nextTask();

                    await activeTask;
                }

                busy = false;
            }

            async void HandleParallel()
            {
                if (type != TaskType.Parallel) return;
                currentTasksList.Add(task());
                if (busy) return;
                busy = true;

                await Task.WhenAll(currentTasksList);

                busy = false;

            }
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
