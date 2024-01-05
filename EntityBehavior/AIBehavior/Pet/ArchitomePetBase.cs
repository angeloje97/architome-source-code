using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace Architome
{
    public class ArchitomePetBase : EntityProp
    {
        // Start is called before the first frame update

        AIBehavior behavior;
        MapEntityGenerator entityGenerator;
        CharacterInfo character;

        [Serializable]
        public struct Pet
        {
            public GameObject prefab;
            public Transform followSpot;
            public EntityInfo info;
            public ArchitomePetAgent agent;
        }

        [SerializeField] Pet pet;

        public Transform petSpot { get { return pet.followSpot; } }

        public override void GetDependencies()
        {
            behavior = GetComponentInParent<AIBehavior>();
            entityGenerator = MapEntityGenerator.active;
            character = entityInfo.CharacterInfo();

            if (character)
            {
                pet.followSpot = character.PetSpot();
            }

            SummonPet();

        }

        void SummonPet()
        {
            if (pet.prefab == null) return;
            if (pet.prefab.GetComponent<EntityInfo>() == null) return;
            if (pet.info == null) return;
            if (entityGenerator.pets == null) return;
            if (pet.followSpot == null) return;


            pet.info = Instantiate(pet.prefab, entityGenerator.pets).GetComponent<EntityInfo>();
            pet.info.transform.position = pet.followSpot.position;

            pet.agent = pet.info.AIBehavior().CreateBehavior<ArchitomePetAgent>();

            pet.agent.SetAgent(this);
        }
    }

}