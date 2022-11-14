using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Threading.Tasks;

namespace Architome
{
    public class EntityCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        public EntityInfo entity;
        [Serializable]
        public struct Info
        {
            public Image entityIcon;
            public TextMeshProUGUI entityName, healthMana, level, role, entityClass, exp;
        }

        [SerializeField] Info info;

        public bool isActive = true;

        public Action<EntityCard> OnSelect { get; set; }
        public Action<EntityCard> OnAction { get; set; }
        public Action<bool> OnActiveChange {get; set;}

        public UnityEvent OnActiveFalse;
        public UnityEvent OnActiveTrue;

        public GuildManager manager;
        public ToolTipManager toolTipManager;
        public ToolTip currentToolTip;
        public KeyBindings keybindManager;

        bool hovering;

        void Start()
        {
            GetDependencies();
        }

        void GetDependencies()
        {
            toolTipManager = ToolTipManager.active;

            keybindManager = KeyBindings.active;
            manager = GuildManager.active;

        }

        public async void OnPointerEnter(PointerEventData eventData)
        {
            if (!toolTipManager) return;


            hovering = true;

            var rightClickIndex = keybindManager.SpriteIndex(KeyCode.Mouse1);

            var currentToolTip = toolTipManager.General();
            currentToolTip.followMouse = true;
            currentToolTip.SetToolTip(new() {
                name = $"{entity}",
                description = $"Use <sprite={rightClickIndex}> for more options."
            });

            Action<bool> action = delegate (bool isChoosing) {
                currentToolTip.SetVisibility(!isChoosing);
            };

            manager.states.OnChoosingActionChange += action;
            currentToolTip.SetVisibility(!manager.states.choosingCardAction);



            while (hovering)
            {

                await Task.Yield();
            }


            manager.states.OnChoosingActionChange -= action;


            currentToolTip.DestroySelf();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovering = false;
        }

        public void SetEntity(EntityInfo entity)
        {
            this.entity = entity;
            info.entityIcon.sprite = entity.PortraitIcon();
            info.entityName.text = $"{entity.entityName}";
            info.healthMana.text = $"Health: {entity.maxHealth} Mana: {entity.maxMana}";
            info.level.text = $"Level: {entity.entityStats.Level}";
            info.role.text = $"Role: {entity.role}";
            info.entityClass.text = info.entityClass != null ? $"Class: {entity.archClass.className}" : "";
            info.exp.text = $"Exp: {entity.entityStats.experience}/{entity.entityStats.experienceReq}";
        }

        public void SelectCard()
        {
            if (!isActive) return;
            OnPointerExit(null);
            OnSelect?.Invoke(this);
        }

        public void SetCard(bool available)
        {
            isActive = available;
            OnActiveChange?.Invoke(isActive);

            var action = available ? OnActiveTrue : OnActiveFalse;

            action?.Invoke();
        }


        public void CardAction()
        {
            if (!isActive) return;
            OnAction?.Invoke(this);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
