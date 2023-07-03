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

        public bool crawlText { get; set; }

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

            if (crawlText)
            {
                await CrawlText(entry.text);
            }
        }

        public async Task CrawlText(string targetText)
        {
            var current = "";
            bool crawling = true;
            _= World.UpdateAction(ContinueCrawl);

            for(int i  = 0; i < targetText.Length; i++)
            {
                current += targetText[i];
                info.content.text = current;
                if (!crawling) break;
                
                for(int j = 0; j < 2; j++)
                {
                    if (!crawling) break;
                    await Task.Yield();
                }



                if (targetText[i].Equals('.'))
                {
                    var delay = .25f;

                    while(delay > 0f)
                    {
                        if (!crawling) break;
                        await Task.Yield();
                        delay -= Time.deltaTime;
                    }

                }
            }

            info.content.text = targetText;

            bool ContinueCrawl(float time)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    crawling = false;
                }

                return crawling;
            }
        }
    }
}
