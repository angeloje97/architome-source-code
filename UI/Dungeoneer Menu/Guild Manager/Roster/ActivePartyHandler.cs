using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Architome
{
    public class ActivePartyHandler : MonoBehaviour
    {
        public List<Icon> memberIcons;
        public List<EntitySlot> memberSlots;

        public TextMeshProUGUI partyLevel;

        DungeoneerManager manager;
        GuildManager guildManager;

        private void Start()
        {
            GetDependencies();
            HandleMemberIcons();
        }

        void GetDependencies()
        {
            manager = DungeoneerManager.active;
            if (manager)
            {
                manager.OnSetSelectedMembers += HandleSelectedMembers;
            }
            guildManager = GuildManager.active;

            if (guildManager)
            {

            }
        }

        void HandleSelectedMembers(DungeoneerManager manager, List<EntityInfo> members)
        {
            UpdateIcons();
        }

        void HandleMemberIcons()
        {
            if (memberIcons == null) return;
            if (memberIcons.Count == 0) return;
            foreach(var icon in memberIcons)
            {
                icon.OnIconAction += HandleIconAction;
                icon.SetIcon(new() { data = null });
            }
        }

        void HandleIconAction(Icon icon)
        {
            if (icon.data == null) return;
            if (icon.data.GetType() != typeof(EntityInfo)) return;
            var entity = (EntityInfo)icon.data;

            guildManager.HandleEntityAction(entity);
        }

        void UpdateIcons()
        {
            if (memberIcons == null || memberIcons.Count == 0) return;
            
            var selectedEntities = manager.selectedEntities;
            if (selectedEntities == null) return;


            for (int i = 0; i < memberIcons.Count; i++)
            {
                if (i >= selectedEntities.Count)
                {
                    memberIcons[i].SetIcon(new() { data = null });
                    continue;
                }

                memberIcons[i].SetIcon(new()
                {
                    sprite = selectedEntities[i].PortraitIcon(),
                    data = selectedEntities[i]
                });
            }

            UpdatePartyLevelText();
        }

        void UpdatePartyLevelText()
        {
            if (partyLevel == null) return;
            partyLevel.text = $"Party Level: {manager.partyLevel}";
        }
    }
}
