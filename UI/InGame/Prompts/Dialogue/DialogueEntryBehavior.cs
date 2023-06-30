using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Architome
{
    public class DialogueEntryBehavior : MonoBehaviour
    {
        [Serializable]
        public struct Info
        {
            public TextMeshProUGUI name, content;
            public SizeFitter sizeFitter;
            public CanvasGroup canvasGroup;
        }

        [SerializeField] Info info;

        public async Task SetEntry(DialogueEntry entry)
        {
            info.name.text = entry.speaker;
            info.content.text = entry.text;

            info.canvasGroup.SetCanvas(false);

            var state = false;
            gameObject.SetActive(state);


            for (int i = 0; i < 3; i++)
            {
                await Task.Yield();
                gameObject.SetActive(!state);
                if (state)
                {
                    info.sizeFitter.AdjustToSize();
                }
                state = !state;

            }

            gameObject.SetActive(true);
            info.canvasGroup.SetCanvas(true);

            if (!entry.fromPlayer)
            {
                await CrawlText(entry.text);
            }
        }

        public async Task CrawlText(string targetText)
        {
            var current = "";

            for(int i  = 0; i < targetText.Length; i++)
            {
                current += targetText[i];
                info.content.text = current;
                
                for(int j = 0; j < 3; j++)
                {
                    await Task.Yield();
                }

                if (targetText[i].Equals('.'))
                {
                    await Task.Delay(500);
                }

            }
        }
    }
}
