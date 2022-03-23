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
        
        public void GetDependencies1()
        {
        }

        void Start()
        {
            GetDependencies();
        }

        public void HandleEntity(EntityInfo target)
        {
            targetEntity = target;
            targetEntity.OnDeath += OnEntityDeath;
            prompt = $"Slay {target.entityName}";
        }

        public void OnEntityDeath(CombatEventData eventData)
        {
            if (!isActive) return;

            if(eventData.target == targetEntity)
            {
                CompleteObjective();
            }



            HandleObjectiveChange();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
