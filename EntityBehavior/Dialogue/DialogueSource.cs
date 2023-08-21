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
        [Serializable]
        struct OptionEvent
        {
            public string name;
            public UnityEvent<DialogueEventData> OnTriggerEvent;
        }
        public DialogueDataSet initialDataSet;
        ArchDialogueManager dialogueManager;
        [Header("Dialogue Source Properties")]
        [SerializeField] float minDistance = 5f;


        public UnityEvent OnStartConversationEvent;
        public UnityEvent OnDialogueDisabledEvent;
        public Action OnStartConversation;
        public Action OnDialogueDisabled;

        [SerializeField] List<OptionEvent> optionEvents;
        Dictionary<string, UnityEvent<DialogueEventData>> optionEventsMap;
        bool disabledCheck;
        void Start()
        {
            GetDependencies();

            ArchAction.Delay(() => {
                disabledCheck = !initialDataSet.disabled;
                HandleDisabled();
            }, 1f);
        }

        [SerializeField] bool validate;
        
        private void OnValidate()
        {
            if (!validate) return;
            validate = false;
            var datas = initialDataSet.data;

            for(int i = 0; i < datas.Count; i++)
            {
                var data = datas[i];

                data.name = $"({i}) {data.text}";

                var options = data.dialogueOptions;

                for(int j = 0; j < options.Count; j++)
                {
                    var option = options[j];
                    var nextTarget = option.nextTarget;
                    var targetString = nextTarget.ToString();

                    switch (nextTarget)
                    {
                        case -1:
                            targetString = "Next";
                            break;
                        case -2:
                            targetString = "Stay";
                            break;
                    }
                    option.name = $"({targetString}) {option.text}";
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public override void GetDependencies()
        {
            base.GetDependencies();
            dialogueManager = ArchDialogueManager.active;

            optionEventsMap = new();
            if(optionEvents != null)
            {
                foreach(var optionEvent in optionEvents)
                {
                    optionEventsMap.Add(optionEvent.name, optionEvent.OnTriggerEvent);
                }
            }
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
            OnStartConversation?.Invoke();
            OnStartConversationEvent?.Invoke();


            dialogueManager.StartDialogue(dialogueEventData);
        }

        public void InvokeOption(string triggerString, DialogueEventData eventData)
        {
            if (!optionEventsMap.ContainsKey(triggerString)) return;
            optionEventsMap[triggerString]?.Invoke(eventData);
        }

        public void FollowListener(DialogueEventData eventData)
        {
            var source = eventData.sourceEntity;
            var listener = eventData.listener;

            var movement = source.Movement();

            _= movement.MoveToAsync(listener.transform, 5f);
        }

        public void HandleDisabled()
        {
            if(disabledCheck != initialDataSet.disabled)
            {
                disabledCheck = initialDataSet.disabled;

                if (initialDataSet.disabled)
                {
                    OnDialogueDisabled?.Invoke();
                }
            }
        }
    }
}
