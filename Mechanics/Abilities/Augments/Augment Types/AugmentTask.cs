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

        public int validReceiver;
        public bool requiresTaskActivator;

        private async void Start()
        {

            await GetDependencies(() => {
                EnableTasks();
                buffManager = augment.entity.Buffs();
            });
        }

        protected override string Description()
        {
            return $"This ability is activated when the unit completes a task";
        }

        public override async void HandleStartTask(TaskEventData eventData)
        {
            var taskHandler = augment.entity.TaskHandler();
            if (taskHandler == null) return;

            if (!CheckTaskActivator(eventData.workInfo)) return;

            var augmentEvent = new Augment.AugmentEventData(this) 
            { 
                active = true,
                hasEnd = true,
            };

            augment.ActivateAugment(augmentEvent);

            await taskHandler.FinishWorking();

            augmentEvent.active = false;
        }

        public override void HandleTaskComplete(TaskEventData eventData)
        {
            if (augment.ability.abilityType != AbilityType.Use) return;
            if (!CheckTaskActivator(eventData.workInfo)) return;

            augment.ability.HandleAbilityType();
            augment.TriggerAugment(new(this));
        }

        public bool CheckTaskActivator(Component componentCheck, bool triggerIncrement = false)
        {
            var activator = componentCheck.GetComponent<AugmentTaskActivator>();
            if (!requiresTaskActivator) return true;
            if (activator == null) return false;

            var valid = activator.ValidAugment(this);

            if (triggerIncrement && valid)
            {
                ArchAction.Delay(() => {
                    activator.IncrementActivated();
                }, .125f);
            }

            return valid;
            
        }
    }
}
