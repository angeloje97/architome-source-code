using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    public class EntitySpeech : EntityProp
    {
        // Start is called before the first frame update
        ChatBubblesManager chatManager;

        
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

        }

        public void Yell(string text)
        {

        }

        public void Whisper(string text)
        {

        }
    }

}