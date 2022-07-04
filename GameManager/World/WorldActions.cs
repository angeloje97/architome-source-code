using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;

namespace Architome
{
    public class WorldActions : MonoBehaviour
    {
        // Start is called before the first frame update
        public static WorldActions active;

        //events

        public Action<GameObject> OnWorldReviveB;
        public Action<GameObject> OnWorldReviveA;

        public bool devToolsActive;
        public float value;
        public string stringValue;
        public bool affectAllPlayableEntities;
        public bool giveExperience;
        public bool damage;
        public bool forceLoadScene;
        public bool completeAllQuests;
        private void Update()
        {
            HandleLoadScene();
            HandleCompleteQuests();
            HandleAllPlayableEntities();
        }

        void HandleAllPlayableEntities()
        {
            if (!affectAllPlayableEntities) return;
            affectAllPlayableEntities = false;

            var playableEntities = GameManager.active.playableEntities;

            foreach (var entity in playableEntities)
            {
                OnSelectTarget(entity.gameObject);
            }
        }
        void HandleLoadScene()
        {
            if (!forceLoadScene) return;
            forceLoadScene = false;
            var sceneManager = ArchSceneManager.active;
            if (sceneManager == null) return;

            sceneManager.LoadScene(stringValue, true);
        }
        void GetDependencies()
        {
            var targetManager = ContainerTargetables.active;

            targetManager.OnSelectTarget += OnSelectTarget;
        }

        public void Awake()
        {
            active = this;

        }

        private void Start()
        {
            GetDependencies();
        }

        public void Restore(GameObject entity, float health = 1, float mana = 1)
        {
            var entityInfo = entity.GetComponent<EntityInfo>();
            if (entityInfo == null) { return; }

            entityInfo.health = health * entityInfo.maxHealth;
            entityInfo.mana = mana * entityInfo.maxMana;


            if (!entityInfo.isAlive)
            {
                entityInfo.isAlive = true;
            }

        }

        void OnSelectTarget(GameObject target)
        {
            HandleSelectEntity(target);
        }

        void HandleSelectEntity(GameObject target)
        {
            if (!devToolsActive) return;
            var info = target.GetComponent<EntityInfo>();
            if (info == null) return;

            HandleExperience();
            HandleDamage();

            void HandleExperience()
            {
                if (!giveExperience) return;
                if (value <= 0)
                {
                    value = 0;
                    return;
                }

                info.GainExperience(this, value);
                //var experienceHandler = info.GetComponentInChildren<EExperienceHandler>();
                //if (experienceHandler == null) return;
                //experienceHandler.GainExp(value);
            }

            void HandleDamage()
            {
                if (!damage) return;
                if (value <= 0) return;

                info.Damage(new(info) { value = value });
            }
        }

        void HandleCompleteQuests()
        {
            if (!completeAllQuests) return;
            completeAllQuests = false;

            var questManager = QuestManager.active;
            if (questManager == null || questManager.quests == null) return;
            foreach (var quests in questManager.quests)
            {
                if (quests.info.state != QuestState.Active) continue;
                quests.ForceComplete();
            }
        }

        public void Revive(GameObject entity, Vector3 position = new Vector3(), float health = 1, float mana = 1)
        {
            var entityInfo = entity.GetComponent<EntityInfo>();
            if (entityInfo == null) { return; }

            OnWorldReviveB?.Invoke(entity);

            if (position != new Vector3())
            {
                entity.transform.position = position;
                entityInfo.currentRoom = entityInfo.CurrentRoom();
            }


            Restore(entity, health, mana);

            OnWorldReviveA?.Invoke(entity);


        }

        public void Kill(GameObject entity)
        {
            var entityInfo = entity.GetComponent<EntityInfo>();
            if (entityInfo == null) { return; }
            entityInfo.Die();
            entityInfo.health = 0;
            entityInfo.isAlive = false;
        }

        public void ReviveAtSpawnBeacon(GameObject entity)
        {
            var entityInfo = entity.GetComponent<EntityInfo>();
            var lastSpawnBeacon = GMHelper.WorldInfo().currentSpawnBeacon;

            if (entityInfo == null) { return; }
            if (lastSpawnBeacon == null) { return; }

            var randomPosition = lastSpawnBeacon.RandomPosition();

            Revive(entity, randomPosition, .25f, .25f);
            entityInfo.OnReviveThis?.Invoke(new(lastSpawnBeacon) { percentValue = .25f });

            lastSpawnBeacon.spawnEvents.OnSpawnEntity?.Invoke(entityInfo);
        }

        public GameObject SpawnEntity(GameObject entity, Vector3 position)
        {
            var collider = entity.GetComponent<Collider>();
            var character = entity.GetComponentInChildren<CharacterInfo>();
            var deltaHeight = -collider.bounds.center.y;
            var sizeOffset = collider.bounds.size.y;

            var positionOffset = -character.transform.localPosition.y;

            var node = AstarPath.active.GetNearest(position).node;

            if (true)
            {
                var newPosition = ((Vector3)node.position) + new Vector3(0, positionOffset, 0);
                return Instantiate(entity, newPosition, new Quaternion());
            }
        }
    }

}