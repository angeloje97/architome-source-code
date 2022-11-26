using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class BossPager : MonoBehaviour
    {
        [Serializable]
        public struct Prefabs
        {
            public BossPage bossPage;
            public Transform bossPageParent;
        }

        [Serializable]
        public struct Components
        {
            public NavBar navBar;
            public List<BossPage> pages;
        }

        [SerializeField] Prefabs prefabs;
        [SerializeField] Components components;



        public MapEntityGenerator activeEntityGenerator;
        ArchSceneManager sceneManager;



        void Start()
        {
            if (prefabs.bossPage == null || prefabs.bossPageParent == null) return;
            GetDependencies();
        }

        void GetDependencies()
        {
            sceneManager = ArchSceneManager.active;

            if (sceneManager)
            {
                sceneManager.BeforeLoadScene += BeforeLoadScene;
                sceneManager.OnLoadScene += OnLoadScene;


            }
        }

        private void OnDestroy()
        {
            if (sceneManager)
            {
                sceneManager.BeforeLoadScene -= BeforeLoadScene;
                sceneManager.OnLoadScene -= OnLoadScene;
            }
        }

        void BeforeLoadScene(ArchSceneManager sceneManager)
        {
            if (activeEntityGenerator == null) return;

            ClearPages();

            activeEntityGenerator.OnGenerateEntity -= OnGenerateEntity;

            activeEntityGenerator = null;
        }

        void OnLoadScene(ArchSceneManager sceneManager)
        {
            activeEntityGenerator = MapEntityGenerator.active;

            if (activeEntityGenerator)
            {
                activeEntityGenerator.OnGenerateEntity += OnGenerateEntity;
            }
        }

        public void ClearPages()
        {
            components.navBar.ClearToggles();
            
            foreach (var page in components.pages)
            {
                Destroy(page.gameObject);
            }
        }

        public void OnGenerateEntity(MapEntityGenerator entityGenerator, EntityInfo entity)
        {
            if (entity.rarity != EntityRarity.Boss) return;

            var newBossPage = Instantiate(prefabs.bossPage, prefabs.bossPageParent).GetComponent<BossPage>();

            if (components.pages == null) components.pages = new();

            components.pages.Add(newBossPage);

            components.navBar.AddToggle(entity.entityName, newBossPage.gameObject);

            newBossPage.SetBossPage(entity);
        }
    }
}
