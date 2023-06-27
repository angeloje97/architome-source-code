using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class DialoguePrompt : MonoBehaviour
    {
        PromptInfo prompt;
        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void GetDependencies()
        {
            prompt = GetComponent<PromptInfo>();
        }

        public void EndDialogue()
        {
            prompt.ClosePrompt();
        }
    }
}
