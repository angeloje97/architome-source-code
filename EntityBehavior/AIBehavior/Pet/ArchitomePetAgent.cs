using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace Architome
{
    public class ArchitomePetAgent : EntityProp
    {
        // Start is called before the first frame update

        ArchitomePetBase petBase;
        Movement movement;
        Transform followSpot;
        CharacterInfo character;




        public EntityInfo master { get { return petBase.entityInfo; } }

        public override async Task GetDependencies(Func<Task> extension)
        {
            await base.GetDependencies(async () => {

                if (entityInfo)
                {
                    entityInfo.OnCombatChange += OnCombatChange;

                    character = entityInfo.CharacterInfo();
                    movement = entityInfo.Movement();

                    if (movement)
                    {
                        movement.OnEndMove += OnEndMove;
                    }

                    if (petBase)
                    {
                        entityInfo.ChangeNPCType(petBase.entityInfo.npcType);
                    }
                }

                await extension(); 
            });

        }

        public void SetAgent(ArchitomePetBase petBase)
        {
            this.petBase = petBase;

            followSpot = petBase.petSpot;
            
            
        }

        async void OnCombatChange(bool isInCombat)
        {
            if (isInCombat) return;

            await movement.MoveToAsync(followSpot);
        }

        void OnEndMove(Movement movement)
        {
            if (movement.Target() != followSpot) return;
            if (character == null) return;
            if (master == null) return;

            character.CopyRotation(master.CharacterInfo().transform.eulerAngles);
        }

        
    }

}