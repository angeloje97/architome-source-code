using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Architome
{
    [Serializable]
    public class DialogueEventData {
        public EntityInfo sourceEntity, listener;
        public DialogueSource source;
        public DialogueDataSet dataSet;
    }

    public class DialogueSource : EntityProp
    {

        public DialogueDataSet initialDataSet;
        ArchDialogueManager dialogueManager;
        [Header("Dialogue Source Properties")]
        [SerializeField] float minDistance = 5f;


        public UnityEvent OnStartConverstation;
        public UnityEvent<Transform> OnGetListener;
        public UnityEvent<Transform> OnGetSource;

        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public override void GetDependencies()
        {
            base.GetDependencies();
            dialogueManager = ArchDialogueManager.active;
        }

        public async void StartDialogue(Clickable.ChoiceData choiceData)
        {
            var entities = choiceData.entitiesClickedWith;
            if (entities == null || entities.Count == 0) return;
            var entity = entities[0];

            var movement = entity.Movement();

            
            var success = await movement.MoveToAsync(entityInfo.transform, minDistance);

            if (!success) return;

            var dialogueEventData = new DialogueEventData()
            {
                listener = entity,
                source = this,
                sourceEntity = entityInfo,
                dataSet = initialDataSet,
            };

            OnGetSource?.Invoke(entityInfo.transform);
            OnGetListener?.Invoke(entity.transform);
            OnStartConverstation?.Invoke();


            dialogueManager.StartDialogue(dialogueEventData);
        }
    }
}
