using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using Architome.Enums;
using UnityEngine;


namespace Architome
{
    public class AugmentTask : AugmentType
    {
        [Header("Task Properties")]
        BuffsManager buffManager;

        private void Start()
        {
            GetDependencies();
        }

        new async void GetDependencies()
        {
            await base.GetDependencies();

            EnableTasks();

            buffManager = augment.entity.Buffs();

        }

        protected override string Description()
        {
            return $"This ability is activated when the unit completes a task";
        }

        public override async void HandleStartTask(TaskEventData eventData)
        {
            var taskHandler = augment.entity.TaskHandler();
            if (taskHandler == null) return;

            var augmentEvent = new Augment.AugmentEventData(this) { active = true };

            augment.ActivateAugment(augmentEvent);

            await taskHandler.FinishWorking();

            augmentEvent.active = false;
        }

        public override void HandleTaskComplete(TaskEventData eventData)
        {
            if (augment.ability.abilityType != AbilityType.Use) return;
            augment.ability.HandleAbilityType();
            augment.TriggerAugment(new(this));
        }
    }
}
