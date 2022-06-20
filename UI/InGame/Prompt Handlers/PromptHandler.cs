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
            public GameObject inputPrompt;
        }

        public Prefabs prefabs;

        public Vector3 position;

        PromptInfo ActivatePrompt(GameObject prompt, Vector3 position = new Vector3())
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
            var entityDeathHandler = GMHelper.EntityDeathHandler();

            if (entityDeathHandler == null) return;

            try
            {
                entityDeathHandler.OnAllPlayableEntityDeath += OnAllPlayableEntityDeath;
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
            Debugger.InConsole(3498, $"Opening general prompt");
            if (prefabs.generalPrompt == null)
            {
                return -1;
            }

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

        async public Task<(int, string)> InputPrompt(PromptInfoData promptData)
        {
            if (prefabs.inputPrompt == null) return (-1, "");

            var prompt = ActivatePrompt(prefabs.generalPrompt, new Vector3(0, 270, 0));

            prompt.SetPrompt(promptData);


            while (prompt.choicePicked == -1)
            {
                await Task.Yield();

                if (!prompt.isActive) break;
            }


            return (prompt.choicePicked, prompt.userInput);
        }

    }

    
}
