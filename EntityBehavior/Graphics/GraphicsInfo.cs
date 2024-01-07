using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Threading.Tasks;
using System;

namespace Architome
{
    public class GraphicsInfo : EntityProp
    {
        // Start is called before the first frame update
        public GameObject entityObject;
        public SpriteRenderer entityIcon;


        public Quaternion currentDirection;
        public Quaternion counterDirection;

        public List<GameObject> properties;

        bool isHidden;

        public override async Task GetDependenciesTask()
        {
            entityObject = entityInfo.gameObject;

            entityInfo.OnLifeChange += OnLifeChange;
            entityInfo.OnHiddenChange += OnHiddenChange;

            entityInfo.portalEvents.OnPortalEnter += OnPortalEnter;

            entityInfo.portalEvents.OnPortalExit += OnPortalExit;

            GetChildren();

            await Task.Delay(125);
            UpdateGraphics();
        }

        public void GetChildren()
        {
            foreach (Transform child in transform)
            {
                properties.Add(child.gameObject);
            }

        }

        private void Awake()
        {
            var gameManager = GMHelper.GameManager();

            if (gameManager.GameState != GameState.Play)
            {
                gameObject.SetActive(false);
            }
        }

        public void OnLifeChange(bool isAlive)
        {
            if (TargetCollider())
            {
                TargetCollider().transform.localPosition = isAlive ? new Vector3(0, 0, 0) : new Vector3(0, -.5f, 0);
            }
        }

        void OnHiddenChange(bool isHidden)
        {
            this.isHidden = isHidden;
            if (!entityInfo.isAlive && !isHidden) return;

            ShowCanvases(!isHidden);
        }

        public void ShowGraphics()
        {
            foreach (var canvas in GetComponentsInChildren<Canvas>())
            {
                canvas.enabled = true;
            }
        }

        async void ShowCanvases(bool val)
        {
            foreach (var canvas in GetComponentsInChildren<Canvas>())
            {
                if (canvas == null) return;

                if (canvas.enabled != val)
                {
                    canvas.enabled = val;
                    await Task.Yield();
                }
            }
        }

        public void OnPortalEnter(PortalInfo portal, GameObject obj)
        {
            ShowCanvases(false);
        }
        public void OnPortalExit(PortalInfo portal, GameObject obj)
        {
            if (isHidden) return;
            ShowCanvases(true);
        }


        public void UpdateGraphics()
        {
            UpdateIcon();
            void UpdateIcon()
            {
                if (entityIcon == null || entityInfo == null) { return; }
                var color = Color.white;
                switch (entityInfo.npcType)
                {
                    case NPCType.Friendly:
                        color = Color.green;
                        break;
                    case NPCType.Neutral:
                        color = Color.yellow;
                        break;
                    case NPCType.Hostile:
                        color = Color.red;
                        break;
                }

                entityIcon.color = color;
            }
        }
        public GameObject TargetCollider()
        {
            foreach (GameObject property in properties)
            {
                if (property.CompareTag("TargetableCollider")) { return property; }
            }
            return null;
        }
        public EntityClusterAgent EntityClusterAgent()
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<EntityClusterAgent>())
                {
                    return child.GetComponent<EntityClusterAgent>();
                }
            }

            return null;
        }
    }

}