using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using Architome.Enums;
using TMPro;
using UnityEngine.EventSystems;

namespace Architome
{
    public class BuffIconBehavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // Start is called before the first frame update

        public BuffInfo buff;
        public bool isActive;

        [Serializable]
        public struct Dependencies
        {
            public CanvasGroup canvasGroup;
            public Image mainSprite;
            public Image backGroundSprite;
            public Image borderSprite;
            public TextMeshProUGUI stacks;
            public Image stackBackDrop;
        }

        [Serializable]
        public struct BorderColors
        {
            public Color harmColor;
            public Color assistColor;
            public Color neutralColor;
        }

        public Dependencies dependencies;
        public BorderColors borderColors;


        public ToolTip toolTip;
        public ToolTipManager manager;

        void Start()
        {
            GetDependencies();
        }

        void GetDependencies()
        {
            manager = ToolTipManager.active;
        }

        // Update is called once per frame
        void Update()
        {
            if (!isActive) { return; }

            if(buff == null) { DestroySelf(); }
            UpdateMainSprite();
        }

        public void UpdateMainSprite()
        {
            if(dependencies.mainSprite == null) { return; }

            dependencies.mainSprite.fillAmount = buff.progress;
        }

        public void SetBuff(BuffInfo buff)
        {
            this.buff = buff;

            SetBorderColor();
            SetSpriteIcons();
            SetEventListeners();
            ManageStackNum();

            isActive = true;


            

            void SetBorderColor()
            {
                if(dependencies.borderSprite == null) { return; }
                var buffColor = DetermineColor(buff.buffTargetType);
                dependencies.borderSprite.color = buffColor;
            }

            void ManageStackNum()
            {
                ArchAction.Delay(() => { }, .125f);
                if (!buff.properties.canStack) return;
                dependencies.stackBackDrop.gameObject.SetActive(buff.properties.canStack);
                dependencies.stacks.enabled = true;
                dependencies.stacks.text = $"{buff.stacks}";
            }


            void SetSpriteIcons()
            {
                UpdateMainSprite();

                SetSprite(buff.Icon());
                //if (buff.buffIcon != null)
                //{
                //    SetSprite(buff.buffIcon);
                //    return;
                //}

                //if (buff.sourceCatalyst.catalystIcon != null)
                //{
                //    SetSprite(buff.sourceCatalyst.catalystIcon);
                //    return;
                //}


            }

            void SetEventListeners()
            {
                buff.OnBuffEnd += OnBuffEnd;
                buff.hostInfo.OnLifeChange += OnLifeChange;
                buff.OnStack += OnStack;
            }



        }

        public Color DetermineColor(BuffTargetType targetType)
        {
            var color = borderColors.neutralColor;

            switch (targetType)
            {
                case BuffTargetType.Assist:
                    color = borderColors.assistColor;
                    break;
                case BuffTargetType.Harm:
                    color = borderColors.harmColor;
                    break;
            }

            return color;
        }

        public void OnLifeChange(bool isAlive)
        {
            if (!isAlive)
            {                
                DestroySelf();
            }
        }

        public void OnStack(BuffInfo buff, int stackNum, float deltaValue)
        {
            dependencies.stacks.text = $"{stackNum}";
        }

        public void SetSprite(Sprite sprite)
        {
            dependencies.mainSprite.sprite = sprite;
            dependencies.backGroundSprite.sprite = sprite;
        }

        public void OnBuffEnd(BuffInfo buffInfo)
        {
            DestroySelf();
        }

        public void DestroySelf()
        {
            buff.OnBuffEnd -= OnBuffEnd;
            buff.hostInfo.OnLifeChange -= OnLifeChange;
            Destroy(gameObject);

            OnPointerExit(null);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (toolTip != null) return;
            if (manager == null)
            {
                manager = ToolTipManager.active;
                if (manager == null) return;
            }


            var subHeadline = buff.buffTargetType == BuffTargetType.Harm ? "Debuff" : "Buff";


            toolTip = manager.GeneralHeader();

            toolTip.adjustToMouse = true;

            var timer = buff.properties.time == -1 ? "" : buff.buffTimer.ToString();

            toolTip.SetToolTip(new()
            {
                icon = dependencies.mainSprite.sprite,
                name = buff.name,
                subeHeadline = subHeadline,
                description = buff.Description(),
                attributes = buff.TypesDescription(),
                value = timer
            });

            ToolTipRoutine();
        }

        async public void ToolTipRoutine()
        {

            while (toolTip != null)
            {
                toolTip.components.value.text = $"{(int) buff.buffTimer}s";
                toolTip.components.attributes.text = buff.TypesDescription();
                await Task.Delay(1000);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (toolTip == null) return;

            toolTip.DestroySelf();
            toolTip = null;
        }
    }

}