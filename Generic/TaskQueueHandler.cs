
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        static int taskHandlers;

        int taskHandlerID;

        public TaskQueueHandler(TaskType type)
        {
            this.type = type;
            currentTasks = new();
            currentTasksList = new();
            
            taskHandlers++;
            taskHandlerID = taskHandlers;
        }


        int paralellTasks = 0;

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
                busy = true;

                paralellTasks++;
                Debugger.System(5100, $"Task {taskHandlerID} Added to parallel task count: {paralellTasks}");
                await task();
                paralellTasks--;

                Debugger.System(5100, $"Task {taskHandlerID} Removed Parallel task: {paralellTasks}");


                if (paralellTasks <= 0) busy = false;

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

    public static class TaskListExtensions
    {
        public static async Task HandleTasks<E>(this List<Func<Task<E>>> tasks, TaskType taskType)
        {
            if(taskType == TaskType.Parallel)
            {
                List<Task> currentTasks = new();

                foreach(var task in tasks)
                {
                    currentTasks.Add(task.Invoke());
                }

                await Task.WhenAll(currentTasks);
            }
            else
            {
                foreach(var task in tasks)
                {
                    await task.Invoke();
                }
            }
        }

        public static async Task HandleTasks(this List<Func<Task>> tasks, TaskType taskType)
        {
            if (tasks.Count == 0) return;
            var alteredTasks = new List<Func<Task<bool>>>();

            foreach(var task in tasks)
            {
                alteredTasks.Add(async () => {
                    await task();
                    return true;
                });
            }

            await HandleTasks(alteredTasks, taskType);
        }
    }
}
