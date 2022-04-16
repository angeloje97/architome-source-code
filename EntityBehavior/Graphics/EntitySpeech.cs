using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
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

        public struct SpeechData
        {
            public string text;
            public SpeechType type;
        }

        // Start is called before the first frame update
        ChatBubblesManager chatManager;
        ArchChatBubble current;

        public Queue<SpeechData> speechQueue;

        public bool isSpeaking;

        
        new void GetDependencies()
        {
            base.GetDependencies();

            chatManager = ChatBubblesManager.active;
        }
        void Start()
        {
            GetDependencies();
        }

        public void Say(string text)
        {
            speechQueue.Enqueue(new() { text = text, type = SpeechType.Say });
            StartSpeech();
        }

        public void Yell(string text)
        {
            speechQueue.Enqueue(new() { text = text, type = SpeechType.Yell });
            StartSpeech();
        }

        public void Whisper(string text)
        {
            speechQueue.Enqueue(new() { text = text, type = SpeechType.Whisper });
            StartSpeech();
        }

        async void StartSpeech()
        {
            if (isSpeaking) return;

            isSpeaking = true;

            while (speechQueue.Count > 0)
            {
                while (current != null)
                {
                    await Task.Yield();
                }

                var next = speechQueue.Dequeue();

                chatManager.ProcessSpeech(transform, next.text, next.type);
            }

            isSpeaking = false;
        }
    }

}