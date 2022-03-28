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
    public bool generatedEntities;

    public Transform entityList;
    public Transform miscEntities;

    [Serializable] public class PatrolGroup
    {
        public List<GameObject> entityMembers;
    }
    
    [Header("Entity Properties")]
    public List<GameObject> tier1Entities;
    public List<GameObject> tier2Entities;
    public List<GameObject> tier3Entities;
    public List<GameObject> neutralEntities;
    public List<GameObject> bossEntities;
    public List<PatrolGroup> patrolGroups;

    //Events
    public Action<MapEntityGenerator> OnEntitiesGenerated;

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

                HandleEntities();
            }, 1f);
            
        }
        else
        {
            OnEntitiesGenerated?.Invoke(this);
        }
    }

    async void HandleEntities()
    {
        
        await SpawnEnemies();
        generatedEntities = true;
        OnEntitiesGenerated?.Invoke(this);

        async Task SpawnEnemies()
        {
            if (tier1Entities.Count == 0 &&
                tier2Entities.Count == 0)
            { return; }
            var enemySpots = new List<Transform>();

            await HandleEntitiesAsync();

            async Task HandleEntitiesAsync()
            {
                foreach (GameObject room in mapInfo.rooms)
                {
                    var roomInfo = room.GetComponent<RoomInfo>() ? room.GetComponent<RoomInfo>() : null;
                    if (roomInfo == null) { continue; }

                    if (roomInfo.tier1EnemyPos && tier1Entities.Count > 0)
                    {
                        foreach (Transform trans in roomInfo.tier1EnemyPos)
                        {
                            SpawnEntity(tier1Entities[0], trans);
                            await Task.Yield();
                        }

                        
                    }

                
                    if (roomInfo.tier2EnemyPos && tier2Entities.Count > 0)
                    {
                        foreach (Transform trans in roomInfo.tier2EnemyPos)
                        {
                            SpawnEntity(tier2Entities[0], trans);
                            await Task.Yield();
                        }

                        
                    }

                

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

                
               
                    if (roomInfo.bossPos &&
                    bossEntities.Count > 0)
                    {
                        foreach (Transform trans in roomInfo.bossPos)
                        {
                            SpawnEntity(bossEntities[0], trans);
                            await Task.Yield();

                            
                        }

                        
                    }

                }
            }
        }
        
    }

    void SpawnPatrolEntity(GameObject entity, Transform spot)
    {
        var newEntity = SpawnEntity(entity, spot);
        newEntity.GetComponentInChildren<NoCombatBehavior>().patrolSpot = spot;
    }

    GameObject SpawnEntity(GameObject entity, Transform spot)
    {
        var normalRotation = new Quaternion();
        var roomInfo = spot.GetComponentInParent<RoomInfo>();

        var newEntity = Instantiate(entity, spot.position, normalRotation, entityList);

        newEntity.GetComponent<EntityInfo>().CharacterInfo().gameObject.transform.rotation = spot.rotation;

        return newEntity;
    }
}
