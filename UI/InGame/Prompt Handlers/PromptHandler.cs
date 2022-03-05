using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class PromptHandler : MonoBehaviour
    {
        // Start is called before the first frame update
        public GameObject allDeathPrompt;
        public Vector3 position;

        public void ActivatePrompt(GameObject prompt, Vector3 position = new Vector3())
        {
            var inGameUI = GetComponentInParent<IGGUIInfo>();

            var newPrompt = Instantiate(prompt, inGameUI.transform);

            var rectTransform = newPrompt.GetComponent<RectTransform>();

            if(position == new Vector3())
            {
                rectTransform.localPosition = this.position;
            }
            else
            {
                rectTransform.localPosition = position;
            }
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

        public void OnAllPlayableEntityDeath(List<EntityInfo> members)
        {
            ActivatePrompt(allDeathPrompt, new Vector3(0, 270, 0));
        }
    }

}
