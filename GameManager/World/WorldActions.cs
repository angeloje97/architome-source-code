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

        public Action<EntityInfo> OnWorldReviveB { get; set; }
        public Action<EntityInfo> OnWorldReviveA { get; set; }

        public bool devToolsActive;
        public float value;
        public string stringValue;
        public ArchScene sceneToLoad;
        public bool affectAllPlayableEntities;
        public bool giveExperience;
        public bool damage;
        public bool forceLoadScene;
        public bool completeAllQuests;
        public bool forceNextLevel;


        private void Update()
        {
            UpdateDevActions();
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

            sceneManager.LoadScene(sceneToLoad);
        }
        void GetDependencies()
        {
            var targetManager = ContainerTargetables.active;

            if (targetManager)
            {
                targetManager.OnSelectTarget += OnSelectTarget;

            }
        }

        public void Awake()
        {
            active = this;

        }

        private void Start()
        {
            GetDependencies();
        }

        #region Dev Actions

        void UpdateDevActions()
        {
            if (!devToolsActive) return;
            HandleLoadScene();
            HandleCompleteQuests();
            HandleAllPlayableEntities();
            HandleForceNextLevel();
        }

        public void Restore(EntityInfo entity, float health = 1, float mana = 1)
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
                var healthEvent = new HealthEvent(info, info, value);
                info.Damage(healthEvent);
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
        void HandleForceNextLevel()
        {
            if (!forceNextLevel) return;
            forceNextLevel = false;

            var playableEntitiesManager = PlayableEntitiesManager.active;
            if (playableEntitiesManager) playableEntitiesManager.NextLevel();
        }

        #endregion

        #region Creating Prefabs
        public ItemInfo DropItem(ItemData data, Vector3 position, bool instantlyLootable = true, bool reveal = false)
        {
            var worldItem = World.active.prefabsUI.worldItem;

            if (worldItem == null) return null;

            var newItem = Instantiate(worldItem, position, new());

            newItem.ManifestItem(data, true);

            if (!instantlyLootable)
            {
                newItem.DelayLooting(2f);
            }

            if (reveal)
            {
                var fx = newItem.GetComponent<ItemFXHandler>();
                if (fx != null)
                {
                    fx.Reveal();
                }
            }

            newItem.ThrowRandomly();

            return newItem;
        }

        public ItemInfo CreateItemUI(ItemData data, Transform parent, bool draggable = true)
        {
            var itemTemplate = World.active.prefabsUI.item;
            if (itemTemplate == null) return null;
            var createdItem = Instantiate(itemTemplate, parent);
            createdItem.ManifestItem(data, true);

            createdItem.SetMovable(draggable);

            return createdItem;


        }

        public ItemInfo CreateItemUI(ItemData data, InventorySlot slot, ItemInfo template, bool draggable = true)
        {
            var createdItem = Instantiate(template, slot.transform);

            createdItem.ManifestItem(data, true);
            createdItem.HandleNewSlot(slot);
            createdItem.ReturnToSlot(3);
            if (!draggable)
            {
                createdItem.SetMovable(false);
            }

            return createdItem;
        }

        public InventorySlot CreateInventorySlot(Transform parent)
        {
            var inventorySlot = World.active.prefabsUI.inventorySlot;

            if (inventorySlot == null) return null;

            var newSlot = Instantiate(inventorySlot, parent);

            return newSlot;
        }

        #endregion

        #region Generic Actions
        public void Revive(EntityInfo entity, Vector3 position = new Vector3(), float health = 1, float mana = 1)
        {
            if (entity == null) { return; }

            OnWorldReviveB?.Invoke(entity);

            if (position != new Vector3())
            {
                entity.transform.position = position;
                entity.currentRoom = entity.CurrentRoom();
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
        public void ReviveAtSpawnBeacon(EntityInfo entity)
        {
            var entityInfo = entity.GetComponent<EntityInfo>();
            var lastSpawnBeacon = GMHelper.WorldInfo().currentSpawnBeacon;

            if (entityInfo == null) { return; }
            if (lastSpawnBeacon == null) { return; }

            var randomPosition = lastSpawnBeacon.RandomPosition();

            Revive(entityInfo, randomPosition, .25f, .25f);
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

        public EntityInfo MoveEntity(EntityInfo entity, Vector3 position, LayerMask groundLayer)
        {
            var offset = entity.GetComponent<BoxCollider>().size.y / 2;

            var node = AstarPath.active.GetNearest(position);

            var newPosition = node.position;

            var groundPosition = V3Helper.GroundPosition(newPosition, groundLayer, 0, offset);


            entity.Move(groundPosition);
            //entity.transform.position = groundPosition;
            //entity.infoEvents.OnSignificantMovementChange?.Invoke(position);

            return entity;
        }
        #endregion
    }

}