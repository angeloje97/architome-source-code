using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;

namespace Architome
{
    public class DungeonDescription : MonoBehaviour
    {
        [Serializable]
        public struct Components
        {
            public TextMeshProUGUI title, description;
            public List<Icon> bossIcons;
            public CanvasGroup mainCanvas;

            [Header("Boss Detail Components")]
            public TextMeshProUGUI bossName;
            public Icon bossIcon;
            public List<Icon> bossAbilityIcons;
            public CanvasGroup bossDetailsCanvasGroup;

            public List<Transform> layoutGroupTargets;
            
        }

        [Serializable]
        public struct Prefabs
        {
            public GameObject icon;
        }

        public Components components;
        public Prefabs prefabs;
        public DungeonTable table;
        public EntityInfo selectedEntity;
        public ToolTip abilityToolTip;

        public Action<EntityInfo> OnSelectEntity;
        void Start()
        {
            GetDependencies();
            SetBossCanvas(false);
            SetDetailsCanvas(false);
        }
        private void OnValidate()
        {
            table = GetComponentInParent<DungeonTable>();
        }
        void Update()
        {
            
        }

        void GetDependencies()
        {
            if (table)
            {
                table.OnSelectedDungeonChange += OnDungeonChange;
            }


            foreach (var icon in components.bossIcons)
            {
                icon.OnSelectIcon += OnSelectBossIcon;
                icon.gameObject.SetActive(false);
            }

            foreach (var icon in components.bossAbilityIcons)
            {
                icon.OnHoverIcon += OnHoverAbilityIcon;
                icon.gameObject.SetActive(false);
            }
        }

        void OnDungeonChange(Dungeon before, Dungeon after)
        {
            ArchAction.Yield(() => {
                SetDetailsCanvas(true);
            });
            
            SetBossCanvas(false);
            

            if (after == null) return;

            UpdateIcons();
            UpdateDescription();
            UpdateName();

            void UpdateIcons()
            {
                var bosses = after != null ? after.Bosses() : new List<EntityInfo>();

                for (int i = 0; i < components.bossIcons.Count; i++)
                {
                    bool isInRange = true;

                    if (bosses.Count <= i)
                    {
                        isInRange = false;
                    }

                    if (components.bossIcons[i].gameObject.activeSelf != isInRange)
                    {
                        components.bossIcons[i].gameObject.SetActive(isInRange);
                    }


                    if (isInRange)
                    {
                        //components.bossIcons[i].SetIconImage(bosses[i].entityPortrait);
                        components.bossIcons[i].SetIcon(new() {
                            sprite = bosses[i].PortraitIcon(),
                            data = bosses[i]
                        });
                        
                    }

                }
            }

            void UpdateName()
            {
                components.title.text = after.dungeonInfo.sets[0].dungeonSetName;
            }

            void UpdateDescription()
            {
                components.description.text = $"Size: {after.size} \n" +
                                              $"Rooms: {after.DungeonRoomsCount()}\n" +
                                              $"Floors: {after.levels.Count}\n";

                for (int i = 0; i < after.levels.Count; i++)
                {
                    components.description.text += $"Floor {i+1} seed: {after.levels[i].levelSeed}\n";
                }
            }
        }
        public void OnSelectBossIcon(Icon icon)
        {
            if (icon == null || icon.data == null) return;
            if (!typeof(EntityInfo).IsAssignableFrom(icon.data.GetType())) return;
            var entity = (EntityInfo) icon.data;
            OnSelectEntity?.Invoke(entity);
            selectedEntity = entity;

            SetBoss(entity);
        }
        public void SetBossCanvas(bool value)
        {
            ArchUI.SetCanvas(components.bossDetailsCanvasGroup, value);
            UpdateLayouts();
        }
        public void SetDetailsCanvas(bool val)
        {
            ArchUI.SetCanvas(components.mainCanvas, val);
            UpdateLayouts();
        }
        public void UpdateLayouts()
        {
            

            for (int i = 0; i < 3; i++)
            {
                foreach (var trans in components.layoutGroupTargets)
                {
                    var layoutGroups = trans.GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>();

                    foreach (var group in layoutGroups)
                    {
                        group.enabled = false;

                        ArchAction.Delay(() => {
                            group.enabled = true;
                        }, .125f);

                    }
                }
            }
        }
        public void SetBoss(EntityInfo entity)
        {
            ArchAction.Delay(() => SetBossCanvas(true), .125f);

            var abilities = entity.GetComponentsInChildren<AbilityInfo>();

            Debugger.InConsole(3295, $"{entity} has {abilities.Length} abilities");

            //components.bossIcon.SetIconImage(entity.entityPortrait);
            components.bossIcon.SetIcon(new() { sprite = entity.PortraitIcon() });
            components.bossName.text = entity.entityName;

            for (int i = 0; i < components.bossAbilityIcons.Count; i++)
            {
                var isInRange = i >= 0 && i < abilities.Length;

                if (components.bossAbilityIcons[i].gameObject.activeSelf != isInRange)
                {
                    components.bossAbilityIcons[i].gameObject.SetActive(isInRange);
                }

                if (!isInRange) continue;

                if (abilities[i].isAttack)
                {
                    components.bossAbilityIcons[i].gameObject.SetActive(false);
                    continue;
                    
                }

                var catalyst = abilities[i].catalyst.GetComponent<CatalystInfo>();

                //components.bossAbilityIcons[i].SetIconImage(catalyst.catalystIcon);
                components.bossAbilityIcons[i].SetIcon(new(){
                    sprite = catalyst.catalystIcon,
                    data = abilities[i]
                });

            }
        }
        public void OnHoverAbilityIcon(Icon icon, bool hovering) //Display tooltip for boss ability here
        {
            Debugger.InConsole(1239, $"{icon.data.GetType()}");
            if (!typeof(AbilityInfo).IsAssignableFrom(icon.data.GetType())) return;
            var ability = (AbilityInfo)icon.data;

            Debugger.InConsole(1092, $"{icon} is being hovered {hovering}");


            HandleHover();
            HandleHoverExit();

            void HandleHover()
            {
                if (!hovering) return;
                var manager = ToolTipManager.active;
                if (manager == null) return;

                abilityToolTip = manager.GeneralHeader();

                abilityToolTip.adjustToMouse = true;

                abilityToolTip.SetToolTip(ability.ToolTipData(false));

            }

            void HandleHoverExit()
            {
                if (hovering) return;
                if (abilityToolTip == null) return;
                abilityToolTip.DestroySelf();

            }
        }

        
    }
}
