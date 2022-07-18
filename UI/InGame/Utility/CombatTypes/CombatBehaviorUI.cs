using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Architome.Enums;
using System;

namespace Architome
{
    public class CombatBehaviorUI : MonoBehaviour
    {
        // Start is called before the first frame update
        EntityInfo entity;
        public CombatBehavior combatBehavior;
        public AIBehavior behavior;

        [Serializable]
        public struct Info
        {
            public TMP_Dropdown dropDown;
            public Image entityIcon;
            public Image classBorder;
        }

        public Info info;

        void Start()
        {
            
        }

        public void OnValidate()
        {
            if (info.dropDown == null) return;

            info.dropDown.options.Clear();

            foreach (CombatBehaviorType type in Enum.GetValues(typeof(CombatBehaviorType)))
            {
                info.dropDown.options.Add(new() { text = type.ToString() });
            }
        }

        public void ChangeOption(TMP_Dropdown dropDown)
        {
            var value = dropDown.value;

            var enums = Enum.GetValues(typeof(CombatBehaviorType));

            if (enums.Length < value) return;

            var behaviorType = (CombatBehaviorType)enums.GetValue(value);

            behavior.combatType = behaviorType;
        }

        public void SetEntity(EntityInfo entity)
        {
            this.entity = entity;
            behavior = entity.GetComponentInChildren<AIBehavior>();



            info.entityIcon.sprite = entity.PortraitIcon();
            info.classBorder.color = entity.archClass.classColor;


            var index = Array.IndexOf(Enum.GetValues(typeof(CombatBehaviorType)), behavior.combatType);

            info.dropDown.SetValueWithoutNotify(index);
        }
    }
}
