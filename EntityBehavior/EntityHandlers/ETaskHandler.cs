using System;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class ETaskHandler : EntityProp
{
        // Start is called before the first frame update
        public WorkerState currentState;
        public Movement movement;
        public WorkInfo currentStation;
        private TaskInfo currentTask;

        //Event Event Triggers
        private TaskInfo previousTask;
        BuffsManager buffManager;
        void Start()
        {
            GetDependencies();
            previousTask = null;
            currentTask = null;
        }
        void Update()
        {
            HandleEvents();
        }
        public new void GetDependencies()
        {
            base.GetDependencies();

            if(entityInfo.Movement())
            {
                movement = entityInfo.Movement();
            }

            entityInfo.combatEvents.OnStatesChange += OnEntityStateChanged;

            buffManager = entityInfo.Buffs();

            entityInfo.taskEvents.OnNewTask += OnNewTask;
            entityInfo.taskEvents.OnTaskComplete += OnTaskComplete;
            entityInfo.OnDamageTaken += OnDamageTaken;
            
        }

        void OnEntityStateChanged(List<EntityState> previous, List<EntityState> current)
        {
            if (currentTask == null) return;

            var interruptionStates = new List<EntityState>()
            {
                EntityState.Stunned,
                EntityState.MindControlled,
                EntityState.Taunted
            };

            var intersection = current.Intersect(interruptionStates);

            if (intersection.Count() > 0)
            {
                StopTask();
            }
        }
        public void HandleEvents()
        {
            if (previousTask == null && currentTask == null) return;
            if(previousTask != currentTask)
            {
                entityInfo.taskEvents.OnNewTask?.Invoke(previousTask, currentTask);

                previousTask = currentTask;
            }

            if(currentState != entityInfo.workerState)
            {
                entityInfo.workerState = currentState;
            }
        }
        public void OnNewTask(TaskInfo previous, TaskInfo current)
        {
            if (!Application.isPlaying) return;
            HandlePreviousTask();

            void HandlePreviousTask()
            {
                if (previous == null) return;

                previous.RemoveWorker(entityInfo);
            }
        }
        public async void OnTaskComplete(TaskEventData eventData)
        {
            await Lingering();
            
            StopTask();

            async Task Lingering()
            {
                if (eventData.task.properties.allowLinger)
                {
                    var target = eventData.workInfo.StationTarget();

                    eventData.task.RemoveWorker(entityInfo);

                    currentState = WorkerState.Lingering;
                    entityInfo.taskEvents.OnLingeringStart?.Invoke(eventData);
                    eventData.task.AddLingering(entityInfo);

                    while (entityInfo.Target() == target)
                    {
                        await Task.Yield();
                    }

                    entityInfo.taskEvents.OnLingeringEnd?.Invoke(eventData);
                    currentState = WorkerState.Idle;
                }
            }
        }
        public void OnDamageTaken(CombatEventData eventData)
        {
            if (currentTask == null) return;
            if (currentTask.properties.damageCancelsTask)
            {
                StopTask();
            }
        }
        public void StopTask()
        {
            if(currentTask == null) { return; }

            currentTask = null;
            currentStation = null;
            currentState = WorkerState.Idle;
        }
        public bool IsCurrentWorkStation(Transform targetCheck)
        {
            if(currentStation == null) { return false; }

            if (currentStation.IsOfWorkStation(targetCheck))
            {
                return true;
            }

            return false;
        }
        public void WorkOn(TaskInfo task)
        {
            if (!task.AddWorker(entityInfo)) { return; }

            entityInfo.taskEvents.OnStartTask?.Invoke(new TaskEventData(task));
        }

        async public Task FinishWorking()
        {
            while (currentTask != null)
            {
                await Task.Yield();
            }
        }

        async public Task<bool> StartWorkAsync(TaskInfo task)
        {
            if (task == null) return false;

            StartWork(task);

            if (currentTask != task) return false;

            bool successful = false;

            Action<TaskEventData> action = (TaskEventData eventData) =>
            {
                successful = true;
            };

            entityInfo.taskEvents.OnTaskComplete += action;

            while (currentTask == task)
            {
                if (successful) break;
                await Task.Yield();
            }

            entityInfo.taskEvents.OnTaskComplete -= action;

            return successful;
        }

        async public void StartWork(TaskInfo task)
        {
            Debugger.InConsole(18964, $"{task}");
            if(task == null) { return; }
            if(!task.AddWorkerOnTheWay(entityInfo)) { return; }

            currentTask = task;
            currentStation = task.properties.station;

            var hasWorkSpot = currentStation.workSpot != null;

            var newTaskEvent = new TaskEventData(task);

            entityInfo.taskEvents.OnMoveToTask?.Invoke(newTaskEvent);
            currentState = WorkerState.MovingToWork;


            var workSpot = currentStation.workSpot;
            float distance = 0f;
            if (workSpot == null)
            {
                workSpot = currentStation.transform;
                distance = 3f;
            }

            var successful = await movement.MoveToAsync(workSpot, distance);


            if (!successful)
            {
                StopTask();
                return;
            }

            entityInfo.SetTarget(task.properties.station.transform);

            WorkOn(task);

            currentState = WorkerState.Working;


            var newPathTarget = await movement.NextPathTarget();

            if (currentTask != null)
            {
                StopTask();
            }

        }
    }

}
