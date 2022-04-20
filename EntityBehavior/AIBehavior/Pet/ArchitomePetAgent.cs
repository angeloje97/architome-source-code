using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

        new void GetDependencies()
        {
            base.GetDependencies();

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
                    entityInfo.npcType = petBase.entityInfo.npcType;
                }
            }

            
        }
        void Start()
        {
            GetDependencies();
        }

        public void SetAgent(ArchitomePetBase petBase)
        {
            this.petBase = petBase;

            followSpot = petBase.petSpot;
            
            
        }

        void OnCombatChange(bool isInCombat)
        {
            if (isInCombat) return;

            movement.MoveTo(followSpot);
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