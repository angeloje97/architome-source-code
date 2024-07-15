using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome;
using System.Threading.Tasks;
using PixelCrushers.DialogueSystem.UnityGUI;

public class MapEntityGenerator : MonoBehaviour
{
    public static MapEntityGenerator active;
    // Start is called before the first frame update
    public MapInfo mapInfo;
    WorldActions world;
    bool initializedEntityGenerator { get; set; }

    public Transform entityList;
    public Transform miscEntities;
    public Transform pets;
    public Transform summons;

    public int entitiesSpawned;
    public int expectedEntities;
    public int targetYield;


    //Events
    public Action<MapEntityGenerator> OnEntitiesGenerated { get; set; }
    public Action<MapEntityGenerator, EntityInfo> OnGenerateEntity { get; set; }

    LayerMask strucutreLayerMask, groundLayerMask;


    DifficultyModifications difficulty;
    List<Dungeon.Rooms> dungeons;
    int dungeonIndex;


    void GetDependencies()
    {
        if(GetComponentInParent<MapInfo>())
        {
            mapInfo = GetComponentInParent<MapInfo>();
            if(mapInfo.RoomGenerator())
            {
                var roomGenerator = mapInfo.RoomGenerator();

                if (roomGenerator)
                {
                    roomGenerator.OnRoomsGenerated += OnRoomsGenerated;
                    roomGenerator.BeforeEndGeneration += BeforeRoomsGenerated;

                }
            }
        }

        world = WorldActions.active;

        var layerMasksData = LayerMasksData.active;

        if (layerMasksData)
        {
            groundLayerMask = layerMasksData.walkableLayer;
            strucutreLayerMask = layerMasksData.structureLayerMask;
        }
    }

    void Awake()
    {
        active = this;
    }
    void Start()
    {
        GetDependencies();

    }

    public void BeforeRoomsGenerated(MapRoomGenerator generator)
    {

        foreach (var room in generator.roomsInUse)
        {
            if (room == null) continue;
            var info = room.GetComponent<RoomInfo>();
            if (info == null) continue;

            var roomPool = info.pool;
            if (roomPool == null) continue;

            SpawnRandomChestInPosition(roomPool.chests, info.chestPos, info);
        }
    }

    public async void OnRoomsGenerated(MapRoomGenerator roomGenerator)
    {
        if (Core.currentDungeon != null)
        {
            dungeonIndex = Core.dungeonIndex;
            dungeons = Core.currentDungeon;
        }

        difficulty = DifficultyModifications.active;

        if(mapInfo.generateEntities)
        {
            await Task.Delay(1000);

            expectedEntities = ExpectedEntities();
            await HandleEntities();
        }

        initializedEntityGenerator = true;
        OnEntitiesGenerated?.Invoke(this);
    }

    public int ExpectedEntities()
    {
        int count = 0;

        foreach (var info in mapInfo.rooms)
        {
            var bossRoom = info.GetComponent<BossRoom>();
            if (info == null) continue;

            var spawnPositions = info.EntitySpawnPositions();

            foreach(var spawnPos in spawnPositions)
            {
                count += spawnPos.parent.childCount;
            }

            if (info.patrolGroups)
            {
                foreach (Transform child in info.patrolGroups)
                {
                    count += child.childCount;
                }
            }
        }

        return count;
    }

    async Task HandleEntities()
    {
        await SpawnEnemies();
        

        async Task SpawnEnemies()
        {
            //if (tier1Entities.Count == 0 &&
            //    tier2Entities.Count == 0)
            //{ return; }
            var enemySpots = new List<Transform>();

            Debugger.System(67979, $"Running Task to spawn enemies");
            foreach (RoomInfo roomInfo in mapInfo.rooms)
                {
                    if (roomInfo == null) { continue; }

                    var pool = roomInfo.pool;

                    if (pool == null) continue;

                    var patrolGroups = pool.patrolGroups;
                    var chests = pool.chests;

                    Debugger.System(67980, $"Handling tier list from {roomInfo}");

                    bool spawnedBoss = false;

                    await pool.HandleTierLists(async (list) => {
                        if (list.tier == EntityTier.Boss)
                        {
                            if (roomInfo.GetType() == typeof(BossRoom))
                            {
                                spawnedBoss = true;
                            }
                            else
                            {
                                return;

                            }
                        }
                        await SpawnRandomInPosition(list.entities, roomInfo.SpawnPositionFromTier(list.tier));
                    });


                    if (roomInfo.patrolGroups != null &&
                        patrolGroups.Count > 0)
                    {

                        for (int i = 0; i < roomInfo.patrolGroups.childCount; i++)
                        {
                            
                            var patrolGroup = roomInfo.patrolGroups.GetChild(i);
                            var randomGroup = ArchGeneric.RandomItem(patrolGroups);
                            for (int j = 0; j < patrolGroup.childCount; j++)
                            {
                                var spot = patrolGroup.GetChild(j);
                                if (j < randomGroup.entityMembers.Count)
                                {

                                    SpawnPatrolEntity(randomGroup.entityMembers[j], spot);
                                    await Task.Yield();
                                }
                                
                            }
                        }
                    }

                    if (spawnedBoss) return;
                    await HandleBossRoom(roomInfo);


                }

        }
        
    }

    public async Task UntilEntityGeratorInitialized()
    {
        await ArchAction.WaitUntil((deltaTime) => initializedEntityGenerator, true);
    }

    async Task SpawnRandomInPosition<T>(List<T> randomEntities, RoomSpawnPositions spawnPosition, float chance = 85f) where T: EntityInfo
    {
        if (randomEntities == null) return;
        if (randomEntities.Count == 0) return;
        if (spawnPosition == null) return;
        var parent = spawnPosition.parent;
        if (parent == null) return;

        foreach (Transform trans in parent)
        {
            if (!ArchGeneric.RollSuccess(chance)) continue;

            var pickedEntity = ArchGeneric.RandomItem(randomEntities);
            await SpawnEntity(pickedEntity, trans);
        }
    }

    async Task<bool> HandleBossRoom(RoomInfo room)
    {
        if (room.GetType() != typeof(BossRoom)) return false;
        var bossRoom = (BossRoom)room;
        var bossPosition = bossRoom.bossPosition;
        var boss = ArchGeneric.RandomItem(bossRoom.possibleBosses);


        if (Core.currentDungeon != null)
        {
            var dungeon = Core.currentDungeon[Core.dungeonIndex];

            if (dungeon.selectedBoss)
            {
                boss = dungeon.selectedBoss;
            }

        }

        if (boss == null) return false;


        var entity = await SpawnEntity(boss, bossPosition);


        return true;
    }


    public void SpawnRandomChestInPosition(List<ArchChest> chests, Transform spots, RoomInfo room, float chance = 25f)
    {
        if (spots == null) return;
        if (chests == null || chests.Count == 0) return;

        foreach (Transform position in spots)
        {
            var roll = UnityEngine.Random.Range(0f, 100f);
            if (roll > chance) continue;

            var randomChest = ArchGeneric.RandomItem(chests);

            SpawnChest(randomChest, position, room);
        }
    }

    ArchChest SpawnChest(ArchChest chestObject, Transform spot, RoomInfo room)
    {
        var groundPosition = V3Helper.GroundPosition(spot.transform.position, groundLayerMask, 1f);
        var rotation = spot.transform.rotation;

        var newChest = Instantiate(chestObject, groundPosition, rotation);

        newChest.transform.SetParent(room.transform);

        HandleDungeonPreset();

        return newChest;
        
        void HandleDungeonPreset()
        {
            if (dungeons == null) return;
            var chestLevel = dungeons[dungeonIndex].level + (dungeonIndex * 2);
            newChest.info.level = chestLevel;   
        }
    }

    async void SpawnPatrolEntity(EntityInfo entity, Transform spot)
    {
        var newEntity = await SpawnEntity(entity, spot);
        newEntity.GetComponentInChildren<NoCombatBehavior>().patrolSpot = spot;
    }

    async Task<EntityInfo> SpawnEntity(EntityInfo entity, Transform spot)
    {
        var roomInfo = spot.GetComponentInParent<RoomInfo>();

        var newEntity = world.SpawnEntity(entity, spot.position);
        newEntity.transform.SetParent(entityList, true);
            
        newEntity.CharacterInfo().gameObject.transform.rotation = spot.rotation;

        HandleDungeonLevels();

        entitiesSpawned++;

        if (targetYield == 0)
        {
            await newEntity.UntilGatheredDependencies();
        }
        else if(entitiesSpawned % targetYield == 0)
        {
            await newEntity.UntilGatheredDependencies();
        }

        OnGenerateEntity?.Invoke(this, newEntity);
        return newEntity;

        void HandleDungeonLevels()
        {
            if (dungeons == null) return;
            var current = dungeons[dungeonIndex];

            float multiplier = 1 + (dungeonIndex * difficulty.settings.dungeonCoreMultiplier);

            //float multiplier = 1 + (Core.dungeonIndex * difficulty.settings.dungeonCoreMultiplier);

            newEntity.entityStats.Level = current.level + (Core.dungeonIndex * 2);
            newEntity.entityStats.MultiplyCoreStats(multiplier);


        }
    }
}
