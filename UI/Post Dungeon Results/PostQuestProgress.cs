using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Architome.Enums;

namespace Architome
{
    public class PostQuestProgress : MonoBehaviour
    {
        Quest quest;
        [SerializeField] bool update;
        [Header("Components")]
        public List<Icon> icons;
        public TextMeshProUGUI title, experienceGained, goldGained;
        public CanvasGroup failedQuestGroup;


        ToolTip toolTip;
        ToolTipManager toolTipManager;

        bool active;

        void Start()
        {
        
        }

        private void OnValidate()
        {
            if (!update) return; update = false;
            icons = GetComponentsInChildren<Icon>().ToList();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetQuest(Quest quest)
        {
            this.quest = quest;
            title.text = quest.questName;
            experienceGained.text = quest.rewards.experience > 0 ? $"Experience: {quest.rewards.experience}" : "";
            goldGained.text = quest.rewards.gold > 0 ? $"Gold: {quest.rewards.gold}" : "";


            if (quest.info.state == QuestState.Failed)
            {
                ArchUI.SetCanvas(failedQuestGroup, true);
            }

            HandleExperienceGained();
            HandleGoldGained();
            HandleItemIcons();
        }

        void HandleExperienceGained()
        {
            if (quest.info.state != QuestState.Completed) return;
            var manager = GetComponentInParent<PostDungeonManager>();
            var experienceAmount = quest.rewards.experience;

            foreach (var entity in manager.entities)
            {
                entity.GainExperience(this, experienceAmount);
            }


        }
        
        void HandleGoldGained()
        {
            if (quest.info.state != QuestState.Completed) return;
            var currentSave = Core.currentSave;
            if (currentSave == null) return;

            currentSave.guildData.AddGold(quest.rewards.gold);
        }

        async public Task QuestProgress()
        {
            while (active)
            {
                await Task.Yield();
            }
        }


        void HandleItemIcons()
        {
            var items = quest.rewards.items;
            for (int i = 0; i < icons.Count; i++)
            {
                if (i >= items.Count)
                {
                    icons[i].gameObject.SetActive(false);
                    continue;
                }

                icons[i].SetIcon(new()
                {
                    sprite = items[i].item.itemIcon,
                    amount = items[i].amount > 1 ? $"{items[i].amount}" : "",
                    data=  items[i].item
                });

                icons[i].OnHoverIcon += OnHoverItemIcon;

            }
        }

        ToolTipManager Manager()
        {
            if (toolTipManager == null)
            {
                toolTipManager = ToolTipManager.active;
            }

            return toolTipManager;
        }

        void OnHoverItemIcon(Icon icon, bool isHovering)
        {
            var manager = Manager();
            if (manager == null) return;
            if (!icon.data.GetType().IsAssignableFrom(typeof(Item))) return;
            var item = (Item)icon.data;


            HandleHoverOn();
            HandleHoverOff();

            void HandleHoverOn()
            {
                if (!isHovering) return;
                if (toolTip != null) return;

                toolTip = manager.GeneralHeader();
                toolTip.SetToolTip(item.ToolTipData());

            }

            void HandleHoverOff()
            {
                if (isHovering) return;
                if (toolTip == null) return;

                toolTip.DestroySelf();
            }
        }
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        
    }
}
