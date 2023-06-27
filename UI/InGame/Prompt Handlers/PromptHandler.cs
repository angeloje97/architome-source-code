using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace Architome
{
    public class PromptHandler : MonoBehaviour
    {
        // Start is called before the first frame update
        public static PromptHandler active { get; private set; }

        PromptInfo currentPrompt;

        [Serializable]
        public struct Prefabs
        {
            public GameObject allDeathPrompt;
            public GameObject generalPrompt;
            public GameObject inputPrompt;
            public GameObject messagePrompt;
            public GameObject sliderPrompt;
            public PromptInfo dialoguePrompt;
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

            var promptInfo = newPrompt.GetComponent<PromptInfo>();

            currentPrompt = promptInfo;

            return promptInfo;
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

        public async Task FinishCurrentPrompt()
        {
            while (true)
            {
                if (currentPrompt == null) return;
                if (currentPrompt.destroyed) return;
                await Task.Yield();
            }
        }

        async public Task<PromptChoiceData> GeneralPrompt(PromptInfoData promptData)
        {
            Debugger.InConsole(3498, $"Opening general prompt");
            if (prefabs.generalPrompt == null)
            {
                return null;
            }

            await FinishCurrentPrompt();

            var prompt = ActivatePrompt(prefabs.generalPrompt, new(0, 270, 0));

            prompt.SetPrompt(promptData);


            return await prompt.UserChoice();
        }

        public async Task<PromptInfo> SequentialPrompt(PromptInfoData initialData)
        {
            if (prefabs.generalPrompt == null) return null;
            await FinishCurrentPrompt();

            var prompt = ActivatePrompt(prefabs.generalPrompt, new(0, 270, 0));

            initialData.autoClose = false;
            prompt.SetPrompt(initialData);

            HandleNonClosingPrompt();

            return prompt;


            async void HandleNonClosingPrompt()
            {
                
                while(prompt != null)
                {
                    await prompt.UserChoice();
                    await Task.Delay(250);
                    if (prompt.optionEnded) break;
                }

                prompt.ClosePrompt();
            }
        }
        
        async public Task<PromptChoiceData> SliderPrompt(PromptInfoData promptData)
        {
            if (!prefabs.sliderPrompt)
            {
                return null;
            }
            await FinishCurrentPrompt();


            var prompt = ActivatePrompt(prefabs.sliderPrompt, new Vector3(0, 270, 0));

            prompt.sliderInfo.enable = true;

            prompt.SetPrompt(promptData);

            return await prompt.UserChoice();
        }

        async public Task<PromptChoiceData> InputPrompt(PromptInfoData promptData)
        {
            if (prefabs.inputPrompt == null) return null;

            await FinishCurrentPrompt();


            var prompt = ActivatePrompt(prefabs.inputPrompt, new Vector3(0, 270, 0));

            prompt.SetPrompt(promptData);

            return await prompt.UserChoice();
        }

        public async Task<PromptChoiceData> MessagePrompt(PromptInfoData promptData)
        {
            if (prefabs.messagePrompt == null) return null;

            await FinishCurrentPrompt();

            var prompt = ActivatePrompt(prefabs.messagePrompt, new Vector3(0, 270, 0));

            prompt.SetPrompt(promptData);

            return await prompt.UserChoice();

        }
    }

    
}
