using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class ObjectiveKillEntities : Objective
    {
        [Header("Objective Kill Entities")]
        public List<string> entitiesToKill;

        [Range(0,1)]
        public float percentageNeeded = 1;

        public int currentEntitiesSlain;
        public int killGoal;

        void Start()
        {
            EntityDeathHandler.active.OnEntityDeath += OnEntityDeath;
        }

        public void HandleEntity(EntityInfo entity)
        {

            if (!entitiesToKill.Contains(entity.entityName))
            {
                entitiesToKill.Add(entity.entityName);
            }

            killGoal++;

            UpdateSlainPercentPrompt();
        }

        public void UpdateSlainPercentPrompt(bool completed = false)
        {
            if(completed)
            {

                prompt = $"Enemy forces slain: (100%)";
                return;
            }

            var currentSlain = (float)currentEntitiesSlain;
            var goal = (float)killGoal;


            prompt = $"Enemy forces slain: ({Mathg.Round((currentSlain/goal)*100, 2)}%)";

        }

        public void OnEntityDeath(CombatEventData eventData)
        {
            if (!isActive) return;
            if(entitiesToKill.Contains(eventData.target.entityName))
            {
                currentEntitiesSlain++;

                UpdateSlainPercentPrompt();
                if(currentEntitiesSlain == killGoal)
                {

                    UpdateSlainPercentPrompt(true);
                    CompleteObjective();
                }

                HandleObjectiveChange();
            }


        }


    }
}
