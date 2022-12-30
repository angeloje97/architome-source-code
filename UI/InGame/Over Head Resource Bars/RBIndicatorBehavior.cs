using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using TMPro;
using System;

namespace Architome
{

    public class RBIndicatorBehavior : MonoBehaviour
    {
        // Start is called before the first frame update
        //I have no idea what the RB stands for in RBIndicatorBehavior.
        // It stands for Resource Bar

        public EntityInfo entityInfo;
        public CombatBehavior combatBehavior;
        public CanvasGroup aggroCanvas;
        public TextMeshProUGUI identityText;

        [Header("Indicator Settings")]
        public bool showAggroIndicator;

        static readonly Dictionary<EntityRarity, int> rarityStars = new() 
        {
            { EntityRarity.Player, 0 },
            { EntityRarity.Common, 0 },
            { EntityRarity.Elite, 2 },
            { EntityRarity.Rare, 3 },
            { EntityRarity.Boss, 4 }
        };

        static readonly HashSet<EntityRarity> ignoreRarities = new()
        {
            EntityRarity.Player,
            EntityRarity.Common
        };

        void Start()
        {
            GetDependencies();
            UpdateIdentity();
        }



        public void GetDependencies()
        {
            entityInfo = GetComponentInParent<EntityInfo>();
            if (entityInfo)
            {
                combatBehavior = entityInfo.CombatBehavior() ? entityInfo.CombatBehavior() : null;
                entityInfo.OnChangeNPCType += OnChangeNPCType;

                if (entityInfo.npcType == NPCType.Hostile) { showAggroIndicator = true; }

                if (combatBehavior)
                {
                    combatBehavior.OnNewTarget += OnNewTarget;
                }

                entityInfo.infoEvents.OnRarityChange += HandleRarityChange;
                entityInfo.partyEvents.OnAddedToParty += HandleAddedToParty;
            }
        }

        private void HandleAddedToParty(PartyInfo party)
        {
            UpdateIdentity();
        }

        void HandleRarityChange(EntityRarity before, EntityRarity after)
        {
            UpdateIdentity();
        }

        void UpdateIdentity()
        {
            if (identityText == null) return;

            var result = "";

            var archClass = entityInfo.archClass;
            var rarity = entityInfo.rarity;
            var party = entityInfo.partyEvents.currentParty;

            if (archClass && archClass.classIconId != -1)
            {
                result += $"<sprite={archClass.classIconId}>";
            }

            if (!ignoreRarities.Contains(rarity))
            {
                result += $"<sprite={rarityStars[rarity]}>";
            }

            if (party)
            {
                var index = party.members.IndexOf(entityInfo);

                result += $"<sprite=10>{index+1}";
            }

            identityText.text = result;
        }


        public void OnNewTarget(EntityInfo previousTarget, EntityInfo newTarget)
        {

            HandleAggroIndicator();

            void HandleAggroIndicator()
            {
                if (!showAggroIndicator) { return; }
                if (newTarget == null)
                {
                    SetAggroIndicator(false);
                    return;
                }
                if (!entityInfo.CanAttack(newTarget)) { return; }

                var isTank = newTarget.GetComponent<EntityInfo>().role == Role.Tank;

                SetAggroIndicator(isTank ? false : true);
            }
        }

        public void OnChangeNPCType(NPCType before, NPCType after)
        {
            showAggroIndicator = after == NPCType.Hostile;

            if (!showAggroIndicator)
            {
                SetAggroIndicator(false);
            }
        }

        void SetAggroIndicator(bool val)
        {
            //roleAggroIndicator.GetComponent<CanvasGroup>().alpha = val? 1 : 0;
            ArchUI.SetCanvas(aggroCanvas, val);
        }

        // Update is called once per frame
        void Update()
        {

        }


    }
}