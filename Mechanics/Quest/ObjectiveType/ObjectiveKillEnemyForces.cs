using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class ObjectiveKillEnemyForces : Objective
    {
        [Header("Objective Kill Entities")]
        public List<string> entitiesToKill;
        public List<EntityInfo> entities;

        [Range(0, 1)]
        public float currentPercent = 0;
        [Range(0,1)]
        public float percentageNeeded = 1;

        //public int currentEntitiesSlain;
        //public int killGoal;

        public float currentHealthSlain { get; set; }
        public float totalHealthPool { get; set; }

        void Start()
        {
        }

        public void HandleEntity(EntityInfo entity)
        {
            if (entities == null) entities = new();
            if (entitiesToKill == null) entitiesToKill = new();

            if (!entities.Contains(entity))
            {
                entities.Add(entity);
            }

            if (!entitiesToKill.Contains(entity.entityName))
            {
                entitiesToKill.Add(entity.entityName);
            }


            entity.entityDescription = ArchString.NextLine(entity.entityDescription);

            totalHealthPool += entity.maxHealth;

            //UpdateSlainPercentPrompt();
        }

        public override void Activate()
        {
            base.Activate();
            EntityDeathHandler.active.OnEntityDeath += OnEntityDeath;


            foreach (var entity in entities)
            {
                entity.infoEvents.OnUpdateObjectives += OnUpdateObjectives;
            }

            questInfo.rewards.experience += totalHealthPool * .25f;
            UpdateSlainPercentPrompt();


        }

        public void UpdateSlainPercentPrompt(bool completed = false)
        {
            if(completed)
            {

                prompt = $"Enemy forces slain: (100%)";
                return;
            }

            prompt = $"Enemy forces slain: ({Mathg.Round((currentPercent/percentageNeeded)*100, 2)}%)";

            UpdateCurrentEntities();

        }

        public void UpdateCurrentEntities()
        {
            for (int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                if (entity == null)
                {
                    entities.RemoveAt(i);
                    i--;
                    continue;
                }

                entity.UpdateObjectives(this);
            }
        }

        public void OnUpdateObjectives(List<string> objectives)
        {
            if (questInfo == null) GetDependencies(); 
            var prompt = $"{questInfo.questName}\n";

            prompt += $"- {this.prompt}";

            objectives.Add(prompt);
        }

        public void OnEntityDeath(CombatEventData eventData)
        {
            if (!isActive) return;
            if (!entitiesToKill.Contains(eventData.target.entityName)) return;

            for (int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];

                var remove = false;
                if (entity == null)
                {
                    remove = true;
                }

                if (entity == eventData.target)
                {
                    remove = true;
                    ArchAction.Yield(() => {
                        entity.infoEvents.OnUpdateObjectives -= OnUpdateObjectives;
                    });
                }

                if (remove == true)
                {
                    entities.RemoveAt(i);
                    i--;
                }
            }

            currentHealthSlain += eventData.target.maxHealth;

            currentPercent = currentHealthSlain / totalHealthPool;

            UpdateSlainPercentPrompt();
            if(currentPercent >= percentageNeeded)
            {
                UpdateSlainPercentPrompt(true);
                CompleteObjective();
            }

            HandleObjectiveChange();
        }




    }
}
