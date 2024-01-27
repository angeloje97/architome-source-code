using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;

namespace Architome
{
    public class ChatBubblesManager : MonoBehaviour
    {

        public static ChatBubblesManager active;

        [Serializable]
        public struct Prefabs
        {
            public GameObject whisper;
            public GameObject say;
            public GameObject yell;
        }

        public struct Colors
        {

        }

        public Colors colors;
        public Prefabs prefabs;

        public Dictionary<SpeechType, GameObject> speechChatBubbles;

        private void Awake()
        {
            active = this;

            speechChatBubbles = new() 
            {
                { SpeechType.Say, prefabs.say },
                { SpeechType.Whisper, prefabs.whisper },
                { SpeechType.Yell, prefabs.yell },
            };
        }

        public ArchChatBubble ProcessSpeech(Transform target, string text, SpeechType type, float time = 5f)
        {

            var prefab = prefabs.say;

            if (speechChatBubbles.ContainsKey(type))
            {
                prefab = speechChatBubbles[type];
            }


            var chatBubble = Instantiate(prefab,transform).GetComponent<ArchChatBubble>();

            chatBubble.SetBubble(target, text, time);

            return chatBubble;

            
        }

        public ArchChatBubble Say(Transform target, string text, float time = 1f)
        {
            if (prefabs.say == null) return null;
            if (!prefabs.say.GetComponent<ArchChatBubble>()) return null;

            return Instantiate(prefabs.say, transform).GetComponent<ArchChatBubble>().SetBubble(target, text, time);
        }

        public ArchChatBubble Whisper(Transform target, string text, float time = 1f)
        {
            if (prefabs.whisper == null) return null;
            if (!prefabs.whisper.GetComponent<ArchChatBubble>()) return null;

            return Instantiate(prefabs.whisper, transform).GetComponent<ArchChatBubble>().SetBubble(target, text, time);
        }

        public ArchChatBubble Yell(Transform target, string text, float time = 1f)
        {
            if (prefabs.yell == null) return null;
            if (!prefabs.yell.GetComponent<ArchChatBubble>()) return null;

            return Instantiate(prefabs.yell, transform).GetComponent<ArchChatBubble>().SetBubble(target, text, time);
        }

    }

}