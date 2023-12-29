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

        public int validReceiver;
        public bool requiresTaskActivator;

        public bool requiresActivationAmount;
        public int targetActivationAmount;

        public HashSet<AugmentTaskActivator> currentTaskActivators;

        private async void Start()
        {

            await GetDependencies(() => {
                EnableTasks();
                EnableCombatChange();
                augment.OnRemove += HandleRemoveAugment;
            });

            currentTaskActivators = new();
        }

        protected override string Description()
        {
            return $"This ability is activated when the unit completes a task";
        }

        bool taskInProgress;

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


            await taskHandler.FinishWorking();

            augmentEvent.active = false;
        }

        public override void HandleTaskComplete(TaskEventData eventData)
        {
            if (augment.ability.abilityType != AbilityType.Use) return;
            if (!CheckTaskActivator(eventData.workInfo)) return;

            augment.ability.HandleAbilityType();
            augment.ActivateAugment(augmentEvent);

            augment.TriggerAugment(new(this));
        }

        void HandleRemoveAugment(Augment augment)
        {
            ClearActivators();
        }

        protected override void HandleCombatChange(bool isInCombat)
        {
            if (isInCombat) return;
            ClearActivators();
        }

        public bool CheckTaskActivator(Component componentCheck, bool triggerIncrement = false)
        {
            var activator = componentCheck.GetComponent<AugmentTaskActivator>();
            var valid = true;
            
            if (requiresTaskActivator)
            {
                if (activator == null)
                {
                    valid = false;
                }
                else
                {
                    valid = activator.ValidAugment(this);
                }
            }

            if (triggerIncrement && valid && activator != null)
            {
                var nextAmount = activator.amountActivated + 1;
                ArchAction.Delay(() => {
                    activator.IncrementActivated(this);
                }, .125f);

                if (requiresActivationAmount)
                {
                    valid = nextAmount == targetActivationAmount;
                }
            }

            if (requiresActivationAmount && activator == null) valid = false;

            return valid;
        }

        void ClearActivators()
        {
            foreach(var activator in currentTaskActivators)
            {
                activator.HandleRemoveAugment(this);
            }

            currentTaskActivators = new();
        }
    }
}
