using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class ObjectiveKillEntity : Objective
    {
        // Start is called before the first frame update
        [Header("Objective Kill Entity Properties")]
        public EntityInfo targetEntity;
        

        void Start()
        {
        }


        public override void Activate()
        {
            base.Activate();

            if (targetEntity == null) return;

            targetEntity.combatEvents.AddListenerHealth(eHealthEvent.OnDeath, OnEntityDeath, this);
            targetEntity.infoEvents.OnUpdateObjectives += OnEntityObjectiveCheck;
            questInfo.rewards.experience += targetEntity.maxHealth * .25f;

            UpdatePrompt();
            requirement = (object o) => !targetEntity.isAlive;
        }


        public void HandleEntity(EntityInfo target)
        {
            targetEntity = target;
            
        }



        void UpdatePrompt()
        {
            if (targetEntity == null)
            {
                return;
            }

            prompt = $"Slay {targetEntity.entityName}";

            targetEntity.UpdateObjectives(this);
        }

        void OnEntityObjectiveCheck(List<string> objectives)
        {
            if (questInfo == null) GetDependencies();
            var prompt = $"{questInfo.questName}\n";
            prompt += $"- {this.prompt}";
            objectives.Add(prompt);
        }

        //void OnUpdateEntityObjectives(List<string> objectives)
        //{
        //    if (questInfo == null) GetDependencies();

        //    var prompt = $"{questInfo.questName}\n";

        //    prompt += $"- {this.prompt}";

        //    objectives.Add(prompt);
        //}

        public void OnEntityDeath(HealthEvent eventData)
        {
            if (!isActive) return;

            //if(eventData.target == targetEntity)
            //{
            //    CompleteObjective();
            //}

            HandleObjectiveChange();
            UpdatePrompt();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
