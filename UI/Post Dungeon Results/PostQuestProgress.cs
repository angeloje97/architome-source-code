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
        public List<InventorySlot> slots;
        public TextMeshProUGUI title, experienceGained;
        public CanvasGroup failedQuestGroup;


        ToolTip toolTip;
        ToolTipManager toolTipManager;

        bool active;

        void Start()
        {
            GetDependencies();
        }

        void GetDependencies()
        {
            var slotHandler = GetComponent<ItemSlotHandler>();

            if (slotHandler)
            {
                slotHandler.OnChangeItem += OnChangeItem;
            }
        }

        private void OnValidate()
        {
            if (!update) return; update = false;
            slots = GetComponentsInChildren<InventorySlot>().ToList();
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


            if (quest.info.state == QuestState.Failed)
            {
                ArchUI.SetCanvas(failedQuestGroup, true);
            }

            HandleExperienceGained();
            //HandleItemIcons();
            HandleItemRewards();
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
        

        async public Task QuestProgress()
        {
            while (active)
            {
                await Task.Yield();
            }
        }

        void OnChangeItem(ItemEventData eventData)
        {
            
            if (eventData.previousItem)
            {
                eventData.previousItem.OnItemAction -= OnItemAction;
            }

            if (eventData.newItem)
            {
                eventData.newItem.OnItemAction += OnItemAction;
            }
        }


        public void OnItemAction(ItemInfo info)
        {

        }

        void HandleItemIcons()
        {
            var items = quest.rewards.items;
            //for (int i = 0; i < icons.Count; i++)
            //{
            //    if (i >= items.Count)
            //    {
            //        icons[i].gameObject.SetActive(false);
            //        continue;
            //    }

            //    icons[i].SetIcon(new()
            //    {
            //        sprite = items[i].item.itemIcon,
            //        amount = items[i].amount > 1 ? $"{items[i].amount}" : "",
            //        data=  items[i].item
            //    });

            //    icons[i].OnHoverIcon += OnHoverItemIcon;

            //}
        }

        void HandleItemRewards()
        {
            var itemPrefab = World.active.prefabsUI.item;
            if (itemPrefab == null) return;

            var items = quest.rewards.items;
            for (int i = 0; i < slots.Count; i++)
            {
                if (i >= items.Count)
                {

                    continue;
                }

                var slot = slots[i];

                var newItem = Instantiate(itemPrefab, transform);

                newItem.ManifestItem(items[i], true);

                newItem.HandleNewSlot(slot);

                ArchAction.Yield(() => newItem.ReturnToSlot());
            }

            ArchAction.Delay(() => {
                foreach (var slot in slots)
                {
                    slot.interactable = false;
                }
            }, .0625f);
        }

        public void LootAll()
        {

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
