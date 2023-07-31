using System.Collections;
using UnityEngine;

namespace Architome
{
    public class AugmentMovement : AugmentType
    {
        Movement entityMovement;
        public float movementOffset;
        
        protected async new void GetDependencies()
        {
            await base.GetDependencies();
            entityMovement = augment.entity.Movement();
        }
        public override async void HandleAbility(AbilityInfo ability, bool start)
        {
            if (start) return;
            if (entityMovement == null) return;
            var reset = entityMovement.SetOffSetMovementSpeed(movementOffset, this);
            await ability.EndActivation();
            reset();
        }
    }
}