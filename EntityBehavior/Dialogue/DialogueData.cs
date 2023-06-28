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
        public List<DialogueData> data;


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
        public string optionText;
        public int nextTarget = -1;

        public UnityEvent OnPickOption;
    }
}
