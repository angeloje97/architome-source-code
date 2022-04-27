using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace Architome
{
    public class PromptHandler : MonoBehaviour
    {
        // Start is called before the first frame update
        public static PromptHandler active { get; private set; }

        [Serializable]
        public struct Prefabs
        {
            public GameObject allDeathPrompt;
            public GameObject generalPrompt;
        }

        public Prefabs prefabs;

        public Vector3 position;

        public PromptInfo ActivatePrompt(GameObject prompt, Vector3 position = new Vector3())
        {
            var newPrompt = Instantiate(prompt, transform);

            var rectTransform = newPrompt.GetComponent<RectTransform>();

            if(position == new Vector3())
            {
                rectTransform.localPosition = this.position;
            }
            else
            {
                rectTransform.localPosition = position;
            }

            transform.SetAsLastSibling();

            return newPrompt.GetComponent<PromptInfo>();
        }
        public void GetDependencies()
        {
            try
            {
                GMHelper.EntityDeathHandler().OnAllPlayableEntityDeath += OnAllPlayableEntityDeath;
            }
            catch
            {

            }
        }
        public void Start()
        {
            GetDependencies();        
        }

        private void Awake()
        {
            active = this;
        }

        public void OnAllPlayableEntityDeath(List<EntityInfo> members)
        {
            ActivatePrompt(prefabs.allDeathPrompt, new Vector3(0, 270, 0));
        }

        async public Task<int> GeneralPrompt(PromptInfoData promptData)
        {
            var prompt = ActivatePrompt(prefabs.generalPrompt, new(0, 270, 0));

            prompt.SetPrompt(promptData);

            prompt.choicePicked = -1;

            while (prompt.choicePicked == -1)
            {
                await Task.Yield();

                if (!prompt.isActive) break;
            }

            return prompt.choicePicked;
        }

    }

    
}
