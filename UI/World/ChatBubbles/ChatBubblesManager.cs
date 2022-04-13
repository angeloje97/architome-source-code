using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

        private void Awake()
        {
            active = this;
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