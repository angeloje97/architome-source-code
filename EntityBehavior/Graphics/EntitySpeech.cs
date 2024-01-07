using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Architome
{
    public class EntitySpeech : EntityProp
    {
        public enum SpeechType
        {
            Say,
            Yell,
            Whisper
        }

        [Serializable]
        public struct SpeechData
        {
            public string text;
            public SpeechType type;
            public float time;
        }

        // Start is called before the first frame update
        ChatBubblesManager chatManager;
        ArchChatBubble current;

        public Queue<SpeechData> speechQueue;
        public List<SpeechData> speechData;

        public bool isSpeaking;

        
        public override void GetDependencies()
        {
                chatManager = ChatBubblesManager.active;

        }

        private void Awake()
        {
            speechQueue = new();
        }

        public void Say(string text, float time = 3f)
        {
            speechQueue.Enqueue(new() { text = text, type = SpeechType.Say, time = time });
            
            StartSpeech();
        }

        public void Yell(string text, float time = 3f)
        {
            var newSpeechData = new SpeechData() { text = text, type = SpeechType.Yell, time = time };
            speechData.Add(newSpeechData);
            speechQueue.Enqueue(newSpeechData);

            //speechData.Add(new() { text = text, type = SpeechType.Yell });
            //speechQueue.Enqueue(new() { text = text, type = SpeechType.Yell });
            StartSpeech();
        }

        public void Whisper(string text, float time = 3f)
        {
            speechQueue.Enqueue(new() { text = text, type = SpeechType.Whisper, time = time });
            StartSpeech();
        }

        async void StartSpeech()
        {
            if (isSpeaking) return;
            //speechData = speechQueue.ToList();

            isSpeaking = true;

            while (speechQueue.Count > 0)
            {
                var next = speechQueue.Dequeue();

                current = chatManager.ProcessSpeech(transform, next.text, next.type, next.time);
                while (current != null)
                {
                    await Task.Yield();
                }
            }

            isSpeaking = false;
        }
    }

}