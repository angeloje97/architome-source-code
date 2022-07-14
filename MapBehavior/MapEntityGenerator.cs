using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome;
using System.Threading.Tasks;
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
    //[Serializable] public class PatrolGroup
    //{
    //    public List<GameObject> entityMembers;
    //}
    
    //[Header("Entity Properties")]
    //public List<GameObject> tier1Entities;
    //public List<GameObject> tier2Entities;
    //public List<GameObject> tier3Entities;
    //public List<GameObject> neutralEntities;
    //public List<GameObject> bossEntities;
    //public List<PatrolGroup> patrolGroups;

    //Events
    public Action<MapEntityGenerator> OnEntitiesGenerated;
    public Action<MapEntityGenerator, EntityInfo> OnGenerateEntity;



    void GetDependencies()
    {
        if(GetComponentInParent<MapInfo>())
        {
            mapInfo = GetComponentInParent<MapInfo>();

            if(mapInfo.RoomGenerator())
            {
                mapInfo.RoomGenerator().OnRoomsGenerated += OnRoomsGenerated;
            }
        }

        world = WorldActions.active;
    }

    void Awake()
    {
        active = this;
    }
    void Start()
    {
        GetDependencies();

    }


    public void OnRoomsGenerated(MapRoomGenerator roomGenerator)
    {
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

        foreach (var room in mapInfo.rooms)
        {
            var info = room.GetComponent<RoomInfo>();
            var bossRoom = room.GetComponent<BossRoom>();
            if (info == null) continue;

            if (info.tier1EnemyPos)
            {
                count += info.tier1EnemyPos.childCount;
            }

            if (info.tier2EnemyPos)
            {
                count += info.tier2EnemyPos.childCount;
            }

            if (info.patrolGroups)
            {
                foreach (Transform child in info.patrolGroups)
                {
                    count += child.childCount;
                }
            }

            if (bossRoom && bossRoom.bossPosition)
            {
                count += bossRoom.bossPosition.childCount;
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
                foreach (GameObject room in mapInfo.rooms)
                {
                    var roomInfo = room.GetComponent<RoomInfo>();
                    if (roomInfo == null) { continue; }

                    var pool = roomInfo.pool;

                    if (pool == null) continue;

                    var tier1Entities = pool.tier1Entities;
                    var tier2Entities = pool.tier2Entities;
                    var tier1Spawners = pool.tier1Spawners;
                    var tier2Spawners = pool.tier2Spawners;
                    var patrolGroups = pool.patrolGroups;

                    await SpawnRandomInPosition(tier1Entities, roomInfo.tier1EnemyPos);
                    await SpawnRandomInPosition(tier2Entities, roomInfo.tier2EnemyPos);
                    await SpawnRandomInPosition(tier1Spawners, roomInfo.tier1SpawnerPos);
                    await SpawnRandomInPosition(tier2Spawners, roomInfo.tier2SpawnerPos);


                    //if (roomInfo.tier1EnemyPos && tier1Entities.Count > 0)
                    //{
                    //    foreach (Transform trans in roomInfo.tier1EnemyPos)
                    //    {
                    //        await SpawnEntity(tier1Entities[UnityEngine.Random.Range(0, tier1Entities.Count)], trans);
                    //        //await Task.Yield();
                    //    }
                    //}

                
                    //if (roomInfo.tier2EnemyPos && tier2Entities.Count > 0)
                    //{
                    //    foreach (Transform trans in roomInfo.tier2EnemyPos)
                    //    {
                    //        await SpawnEntity (tier2Entities[0], trans);
                    //        //await Task.Yield();
                    //    }

                        
                    //}

                    //if (roomInfo.tier1SpawnerPos)
                    //{
                    //    if (tier1Spawners != null && tier1Spawners.Count > 0)
                    //    {
                    //        foreach (Transform trans in roomInfo.tier1SpawnerPos)
                    //        {
                    //            var randomSpawner = ArchGeneric.RandomItem(tier1Spawners);

                    //            await SpawnEntity(randomSpawner, trans);
                    //        }
                    //    }
                    //}
                    
                    
                    
                    //if (roomInfo.tier2SpawnerPos)
                    //{
                    //    if (tier2Spawners != null && tier2Spawners.Count > 0)
                    //    {
                    //        foreach (Transform trans in roomInfo.tier2SpawnerPos)
                    //        {
                    //            var randomSpawner = ArchGeneric.RandomItem(tier2Spawners);

                    //            await SpawnEntity(randomSpawner, trans);
                    //        }
                    //    }
                    //}
                

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

                    //if (roomInfo.GetType() == typeof(BossRoom))
                    //{
                    //    var bossRoom = (BossRoom)roomInfo;
                    //    var bossPosition = bossRoom.bossPosition;
                    //    var boss = bossRoom.bossToSpawn;

                    //}
               
                    //if (roomInfo.bossPos &&
                    //bossEntities.Count > 0)
                    //{
                    //    foreach (Transform trans in roomInfo.bossPos)
                    //    {
                    //        SpawnEntity(bossEntities[0], trans);
                    //        await Task.Yield();

                            
                    //    }
                    //}

                }
            }
        }
        
    }


    async Task SpawnRandomInPosition(List<GameObject> randomEntities, Transform parent)
    {
        if (randomEntities == null) return;
        if (randomEntities.Count == 0) return;
        if (parent == null) return;

        foreach (Transform trans in parent)
        {
            var pickedEntity = ArchGeneric.RandomItem(randomEntities);

            await SpawnEntity(pickedEntity, trans);
        }
    }

    async Task<bool> HandleBossRoom(RoomInfo room)
    {
        if (room.GetType() != typeof(BossRoom)) return false;
        var bossRoom = (BossRoom)room;
        var bossPosition = bossRoom.bossPosition;
        var boss = bossRoom.possibleBosses.Count > 0 ? bossRoom.possibleBosses[UnityEngine.Random.Range(0, bossRoom.possibleBosses.Count)] : null;

        if (boss == null) return false;

        //int bossLevel = boss.GetComponent<EntityInfo>().entityStats.Level;

        if (Core.currentDungeon != null)
        {
            var dungeon = Core.currentDungeon[Core.dungeonIndex];

            if (dungeon.selectedBoss)
            {
                boss = dungeon.selectedBoss.gameObject;
            }

        }



        var entity = await SpawnEntity(boss, bossPosition);


        return true;
    }


    async void SpawnPatrolEntity(GameObject entity, Transform spot)
    {
        var newEntity = await SpawnEntity(entity, spot);
        newEntity.GetComponentInChildren<NoCombatBehavior>().patrolSpot = spot;
    }

    async Task<EntityInfo> SpawnEntity(GameObject entity, Transform spot)
    {
        //var normalRotation = new Quaternion();
        
        
        var roomInfo = spot.GetComponentInParent<RoomInfo>();

        var newEntity = world.SpawnEntity(entity, spot.position).GetComponent<EntityInfo>();
        newEntity.transform.SetParent(entityList, true);
            
            //Instantiate(entity, spot.position, normalRotation, entityList);

        newEntity.CharacterInfo().gameObject.transform.rotation = spot.rotation;

        HandleDungeonLevels();

        entitiesSpawned++;

        if (targetYield == 0)
        {
            await Task.Yield();
        }
        else if(entitiesSpawned % targetYield == 0)
        {
            await Task.Yield();
        }

        OnGenerateEntity?.Invoke(this, newEntity);
        return newEntity;

        void HandleDungeonLevels()
        {
            if (Core.currentDungeon == null) return;
            var difficulty = DifficultyModifications.active;


            float multiplier = 1 + (Core.dungeonIndex * difficulty.settings.dungeonCoreMultiplier);

            newEntity.entityStats.Level += (Core.dungeonIndex * 2);
            newEntity.entityStats.MultiplyCoreStats(multiplier);


        }
    }
}
