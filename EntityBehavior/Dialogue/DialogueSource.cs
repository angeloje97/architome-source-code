using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace Architome
{
    [Serializable]
    public class DialogueEventData {
        public EntityInfo sourceEntity, listener;
        public DialogueSource source;
        public DialogueDataSet dataSet;
    }

    public struct DialogueChangeRequestData
    {
        public int entryIndex;
        public int choiceIndex;
        public bool choiceAvailability;
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


        public SafeEvent<DialogueChangeRequestData> OnSendRequestData { get; set; }

        [SerializeField] List<OptionEvent> optionEvents;
        Dictionary<string, UnityEvent<DialogueEventData>> optionEventsMap;

        bool disabledCheck;

        DialogueChangeRequestData currentRequestData;
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
        /// <summary>
        /// Sets the availabily of At entry id: X, option: Y with Z = 1(true) or 0(false)
        /// </summary>
        public void SetReqDataEntry(int index)
        {
            currentRequestData = new();
            currentRequestData.entryIndex = index;
        }

        public void SetReqDataOption(int index)
        {
            currentRequestData.choiceIndex = index;
        }

        public void SetReqDataValue(bool value)
        {
            currentRequestData.choiceAvailability = value;
        }

        public void SendReqData()
        {
            var entryIndex = currentRequestData.entryIndex;
            var choiceIndex = currentRequestData.choiceIndex;
            if (entryIndex >= initialDataSet.data.Count) return;
            if (entryIndex < 0) return;
            if (choiceIndex >= initialDataSet.data[entryIndex].dialogueOptions.Count) return;
            if (choiceIndex < 0) return;

            initialDataSet.data[entryIndex].dialogueOptions[choiceIndex].disabled = !currentRequestData.choiceAvailability;
            OnSendRequestData.Invoke(currentRequestData);

            currentRequestData = new();
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
