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
    public class EntityCard : MonoBehaviour
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
        public ToolTip currentToolTip;

        void Start()
        {
            GetDependencies();
        }

        void GetDependencies()
        {
            manager = GuildManager.active;
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
