using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public class BossPhaseHandler : MonoBehaviour
    {

        [SerializeField] BossBehavior behavior;
        [SerializeField] EntitySpeech speech;
        [SerializeField] ETaskHandler taskHandler;
        AbilityManager abilityManager;



        Queue<Func<Task>> tasks;
        bool taskActive;



        void GetDependenices()
        {
            behavior = GetComponentInParent<BossBehavior>();
            behavior.OnPhase += OnPhase;

            var entityInfo = GetComponentInParent<EntityInfo>();

            if (entityInfo)
            {
                speech = entityInfo.Speech();
            }
        }
        void Start()
        {
            GetDependenices();
            tasks = new();
        }

        // Update is called once per frame
        void Update()
        {

        }
        
        TaskInfo BossRoomTask(BossBehavior.Phase phase)
        {
            try
            {
                var bossRoom = behavior.bossRoom;
                var stations = bossRoom.bossStations;
                var stationIndex = phase.stationIndex % stations.Count;
                var station = stations[stationIndex];
                var taskIndex = phase.taskIndex % station.tasks.Count;

                return station.tasks[taskIndex];
            }
            catch
            {
                return null;
            }

        }

        void OnPhase(BossBehavior.Phase phase)
        {

            behavior.AddTask(HandlePhase);

            async Task HandlePhase()
            {
                HandleAbility();
                HandleSpeech();

                if (phase.refillsMana)
                {
                    behavior.entityInfo.GainResource(behavior.entityInfo.maxMana);
                }

                await HandlePhaseWork();
            }

            async Task HandlePhaseWork()
            {
                if (!phase.usesBossStation) return;

                var targetTask = BossRoomTask(phase);
                if (targetTask == null) return;

                await Task.Delay(500);

                await abilityManager.CastingEnd();

                taskHandler.AddTask(targetTask);

                await taskHandler.FinishTasks();
            }

            void HandleSpeech()
            {
                if (!speech) return;

                speech.Yell(phase.activationPhrase);
            }

            void HandleAbility()
            {
                if (phase.phaseAbility.ability == null) return;
                var combatBehavior = behavior.combatBehavior;
                combatBehavior.specialAbilities.Insert(0, phase.phaseAbility);
                combatBehavior.OnAddedSpecialAbility?.Invoke(phase.phaseAbility);
            }
        }
    }

}