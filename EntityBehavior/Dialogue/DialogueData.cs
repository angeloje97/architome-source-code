using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Architome
{
    [Serializable]
    public class DialogueDataSet
    {
        public int currentDialogueData;
        public List<DialogueData> data;
        public List<DialogueEntry> entries;
    }

    [Serializable]
    public class DialogueData
    {
        [TextArea(5, 10)]
        public string text;

        public List<DialogueOption> dialogueOptions;
    }

    [Serializable]
    public class DialogueOption
    {
        [TextArea(3,6)]
        public string text;
        public string triggerString;
        public int nextTarget = -1;
        public bool endsDialogue;
    }

    [Serializable]
    public class DialogueEntry
    {
        public string speaker;
        public string text;
        public bool fromPlayer;

        public DialogueEntry(string speaker, string text, bool fromPlayer = false)
        {
            this.speaker = speaker;
            this.text = text;
            this.fromPlayer = fromPlayer;
        }

    }

}
