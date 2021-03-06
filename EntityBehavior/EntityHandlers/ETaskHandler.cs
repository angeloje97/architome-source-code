using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;

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
        void Start()
        {
            GetDependencies();
        }
        public new void GetDependencies()
        {
            base.GetDependencies();

            if(entityInfo.Movement())
            {
                movement = entityInfo.Movement();
                movement.OnNewPathTarget += OnNewPathTarget;
                movement.OnEndMove += OnEndMove;
            }

            entityInfo.taskEvents.OnNewTask += OnNewTask;
            entityInfo.taskEvents.OnTaskComplete += OnTaskComplete;
            entityInfo.OnDamageTaken += OnDamageTaken;
            
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
                entityInfo.taskEvents.OnNewTask?.Invoke(previousTask, currentTask);

                previousTask = currentTask;
            }

            if(currentState != entityInfo.workerState)
            {
                entityInfo.workerState = currentState;
            }
        }
        public void OnArrival(Movement movement, Transform target)
        {
        }

        public void OnEndMove(Movement movement)
        {
            //var target = movement.Target();
            //ArchAction.Delay(() => {
            //    if (target == null) { return; }
            //    if (currentTask == null) { return; }
            //    if (currentTask.properties.station == null) { return; }
            //    if (!movement.IsInRangeFromTarget()) { return; }

            //    movement.StopMoving();
            //    WorkOn(currentTask); 
            //}, .125f);
            
            

        }

        public void OnNewTask(TaskInfo previous, TaskInfo current)
        {
            HandlePreviousTask();

            void HandlePreviousTask()
            {
                if (previous == null) return;

                previous.RemoveWorker(entityInfo);
            }
        }

        public void OnTaskComplete(TaskEventData eventData)
        {
            StopTask();
        }


        public void OnNewPathTarget(Movement movement, Transform previousTarget, Transform currentTarget)
        {
            if (currentTask == null) { return; }

            if (!IsCurrentWorkStation(currentTarget))
            {
                StopTask();
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

        bool IsCurrentWorkStation(Transform targetCheck)
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
            currentState = WorkerState.Working;
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

            var successful = true;

            if(hasWorkSpot)
            {
                //movement.MoveTo(currentStation.workSpot);
                successful = await movement.MoveToAsync(currentStation.workSpot);
            }
            else
            {
                successful = await movement.MoveToAsync(currentStation.transform, 3f);
                //movement.MoveTo(currentStation.transform, 3f);
            }

            if (!successful)
            {
                StopTask();
            }

            if (!successful) return;

            WorkOn(task);
        }

    }

}
