using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class DialogueSource : EntityProp
    {
        [Header("Dialogue Source Properties")]
        [SerializeField] float minDistance = 5f;
        void Start()
        {
            base.GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public async void StartDialogue(Clickable.ChoiceData choiceData)
        {
            var entities = choiceData.entitiesClickedWith;
            if (entities == null || entities.Count == 0) return;
            var entity = entities[0];

            var movement = entity.Movement();

            var success = await movement.MoveToAsync(entityInfo.transform, minDistance);
        }
    }
}
