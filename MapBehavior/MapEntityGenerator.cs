using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome;
using System.Threading.Tasks;
using UnityEngine.Events;

public class MapEntityGenerator : MonoBehaviour
{
    public static MapEntityGenerator active;
    // Start is called before the first frame update
    public MapInfo mapInfo;
    WorldActions world;
    public bool generatedEntities;

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

    public void OnRoomsGenerated(MapRoomGenerator roomGenerator)
    {
        if (Core.currentDungeon != null)
        {
            dungeonIndex = Core.dungeonIndex;
            dungeons = Core.currentDungeon;
        }

        difficulty = DifficultyModifications.active;

        if(mapInfo.generateEntities)
        {
            ArchAction.Delay(() => 
            {
                expectedEntities = ExpectedEntities();
                HandleEntities();
            }, 1f);
        }
        else
        {
            OnEntitiesGenerated?.Invoke(this);
        }
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

    async void HandleEntities()
    {


        await SpawnEnemies();
        generatedEntities = true;
        OnEntitiesGenerated?.Invoke(this);

        async Task SpawnEnemies()
        {
            //if (tier1Entities.Count == 0 &&
            //    tier2Entities.Count == 0)
            //{ return; }
            var enemySpots = new List<Transform>();

            await HandleEntitiesAsync();

            async Task HandleEntitiesAsync()
            {
                foreach (RoomInfo roomInfo in mapInfo.rooms)
                {
                    if (roomInfo == null) { continue; }

                    var pool = roomInfo.pool;

                    if (pool == null) continue;

                    var tier1Entities = pool.tier1Entities;
                    var tier2Entities = pool.tier2Entities;
                    var tier1Spawners = pool.tier1Spawners;
                    var tier2Spawners = pool.tier2Spawners;
                    var patrolGroups = pool.patrolGroups;
                    var chests = pool.chests;

                    //await SpawnRandomInPosition(tier1Entities, roomInfo.SpawnPositionFromTier(EntityTier.Tier1));
                    //await SpawnRandomInPosition(tier2Entities, roomInfo.SpawnPositionFromTier(EntityTier.Tier2));
                    //await SpawnRandomInPosition(tier1Spawners, roomInfo.SpawnPositionFromTier(EntityTier.Tier1Spawner));
                    //await SpawnRandomInPosition(tier2Spawners, roomInfo.SpawnPositionFromTier(EntityTier.Tier2Spawner));

                    await pool.HandleTierLists(async (tier, list) => {
                        await SpawnRandomInPosition(list, roomInfo.SpawnPositionFromTier(tier));
                    });


                    if (roomInfo.patrolGroups != null &&
                        patrolGroups.Count > 0)
                    {

                        for (int i = 0; i < roomInfo.patrolGroups.childCount; i++)
                        {
                            
                            var patrolGroup = roomInfo.patrolGroups.GetChild(i);
                            var randomGroup = patrolGroups[UnityEngine.Random.Range(0, patrolGroups.Count)];
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

                    await HandleBossRoom(roomInfo);


                }
            }
        }
        
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
