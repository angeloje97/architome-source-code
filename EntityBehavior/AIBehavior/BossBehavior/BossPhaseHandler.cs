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


        bool doingTask;
        int taskAmount;


        void GetDependenices()
        {
            behavior = GetComponentInParent<BossBehavior>();
            behavior.OnPhase += OnPhase;

            var entityInfo = GetComponentInParent<EntityInfo>();

            if (entityInfo)
            {
                speech = entityInfo.Speech;
            }
        }
        void Start()
        {
            GetDependenices();
        }

        // Update is called once per frame
        void Update()
        {

        }

        async void OnPhase(BossBehavior.Phase phase)
        {

            HandleAbility();
            HandleSpeech();



            if (phase.refillsMana)
            {
                behavior.entityInfo.GainResource(behavior.entityInfo.maxMana);
            }

            await HandlePhaseWork();


            async Task HandlePhaseWork()
            {
                if (!phase.usesBossStation) return;
                var bossRoom = behavior.bossRoom;
                if (bossRoom == null) return;
                if (bossRoom.bossStation == null) return;
                if (bossRoom.bossStation.tasks.Count == 0) return;

                await Task.Delay(500);

                taskAmount++;

                if (doingTask) return;

                doingTask = true;

                await abilityManager.CastingEnd();

                while (taskAmount > 0)
                {
                    taskAmount--;
                    await taskHandler.StartWorkAsync(bossRoom.bossStation.tasks[0]);
                }
                doingTask = false;
            }

            void HandleSpeech()
            {
                if (!speech) return;

                speech.Yell(phase.activationPhrase);
            }

            void HandleAbility()
            {
                if (phase.phaseAbility.ability == null) return;
                behavior.combatBehavior.specialAbilities.Insert(0, phase.phaseAbility);
            }
        }
    }

}