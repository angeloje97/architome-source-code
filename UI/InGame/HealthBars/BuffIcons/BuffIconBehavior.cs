using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Architome.Enums;

namespace Architome
{
    public class BuffIconBehavior : MonoBehaviour
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

        void Start()
        {

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

            dependencies.mainSprite.fillAmount = buff.buffTimer / buff.properties.time;
        }

        public void SetBuff(BuffInfo buff)
        {
            this.buff = buff;

            SetBorderColor();
            SetSpriteIcons();
            SetEventListeners();

            isActive = true;


            

            void SetBorderColor()
            {
                if(dependencies.borderSprite == null) { return; }
                var buffColor = DetermineColor(buff.buffTargetType);
                dependencies.borderSprite.color = buffColor;
            }

            void SetSpriteIcons()
            {
                UpdateMainSprite();

                if (buff.buffIcon != null)
                {
                    SetSprite(buff.buffIcon);
                    return;
                }

                if (buff.sourceCatalyst.catalystIcon != null)
                {
                    SetSprite(buff.sourceCatalyst.catalystIcon);
                    return;
                }


            }

            void SetEventListeners()
            {
                buff.OnBuffEnd += OnBuffEnd;
                buff.hostInfo.OnLifeChange += OnLifeChange;
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
        }
    }

}