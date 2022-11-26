using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

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
            public GameObject messagePrompt;
            public GameObject sliderPrompt;
        }

        public Sprite allDeathIcon;

        public Prefabs prefabs;

        public Vector3 position;

        [Header("Settings")]
        public bool ignoreAllPartyMembersDead;
        public bool ignoreDeadMembersIfNoBeacon;

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
                if (!ignoreAllPartyMembersDead)
                {
                    entityDeathHandler.OnAllPlayableEntityDeath += OnAllPlayableEntityDeath;

                }
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

        public async void OnAllPlayableEntityDeath(List<EntityInfo> members)
        {
            var playerSpawnBeacon = GMHelper.WorldInfo().currentSpawnBeacon;

            if (!playerSpawnBeacon && ignoreDeadMembersIfNoBeacon) return;

            var userChoice = await GeneralPrompt(new() {
                icon = allDeathIcon,
                title = "Party Members Dead",
                blocksScreen = true,
                options = new()
                {
                    new("Revive", (option) => HandleRevive()) { timer = 3 },
                    
                },
                question = "All party members are dead." + playerSpawnBeacon != null ? "Do you wish to revive at the most recent spawn beacon?" : "",
                
            });


            void HandleRevive()
            {
                if (playerSpawnBeacon == null)
                {
                    var worldActions = WorldActions.active;
                    if (worldActions)
                    {
                        foreach (var member in members)
                        {
                            worldActions.Revive(member, member.transform.position, .50f, .50f);
                        }
                    }
                    return;
                }

                var spawnBeaconHandler = playerSpawnBeacon.GetComponentInChildren<PlayerSpawnBeaconHandler>();

                spawnBeaconHandler.ReviveDeadPartyMembers();
            }

            
        }

        async Task<PromptChoiceData> UserChoice(PromptInfo prompt)
        {

            while (!prompt.optionEnded)
            {
                await Task.Yield();

                if (!prompt) break;
                if (!prompt.isActive) break;
            }


            
            return prompt.choiceData;
        }

        async public Task<PromptChoiceData> GeneralPrompt(PromptInfoData promptData)
        {
            Debugger.InConsole(3498, $"Opening general prompt");
            if (prefabs.generalPrompt == null)
            {
                return null;
            }

            var prompt = ActivatePrompt(prefabs.generalPrompt, new(0, 270, 0));

            prompt.SetPrompt(promptData);


            return await UserChoice(prompt);
        }

        async public Task<PromptChoiceData> SliderPrompt(PromptInfoData promptData)
        {
            if (!prefabs.sliderPrompt)
            {
                return null;
            }

            var prompt = ActivatePrompt(prefabs.sliderPrompt, new Vector3(0, 270, 0));

            prompt.sliderInfo.enable = true;

            prompt.SetPrompt(promptData);

            return await UserChoice(prompt);
        }

        async public Task<PromptChoiceData> InputPrompt(PromptInfoData promptData)
        {
            if (prefabs.inputPrompt == null) return null;

            var prompt = ActivatePrompt(prefabs.inputPrompt, new Vector3(0, 270, 0));

            prompt.SetPrompt(promptData);


            

            return await UserChoice(prompt);
        }

        public async Task<PromptChoiceData> MessagePrompt(PromptInfoData promptData)
        {
            if (prefabs.messagePrompt == null) return null;
            var prompt = ActivatePrompt(prefabs.messagePrompt, new Vector3(0, 270, 0));

            prompt.SetPrompt(promptData);

            return await UserChoice(prompt);


        }
    }

    
}
