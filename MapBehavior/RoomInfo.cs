using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Threading.Tasks;

[RequireComponent(typeof(RoomInfoTool))]
public class RoomInfo : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Room Properties")]
    public bool isEntranceRoom;
    public bool badSpawn;
    public Transform roomCenter;
    public PathInfo originPath;
    public MapInfo mapInfo;

    [Header("Path Properties")]
    public List<PathInfo> paths;
    public List<PathInfo> incompatablePaths;
    //public Transform bin;


    [Header("Spawn Positions")]
    public Transform tier1EnemyPos;
    public Transform tier2EnemyPos;
    public Transform tier3EnemyPos;
    public Transform bossPos;
    public Transform tier1SpawnerPos;
    public Transform tier2SpawnerPos;
    public Transform chestPos;
    
    [Header("Patrol Properties")]
    public Transform patrolPoints;
    public Transform patrolGroups;

    [Header("RoomBehavior")]
    public Transform[] allObjects;
    public Renderer[] allRenderers;
    public bool hideOnStart;
    public bool ignoreCheckRoomCollison;
    public bool isRevealed = true;

    //events

    public Action<RoomInfo, bool> OnShowRoom;

    //Private properties
    private float percentReveal;

    void GetDependencies()
    {
        if(MapHelper.MapInfo())
        {
            mapInfo = MapHelper.MapInfo();
            mapInfo.RoomGenerator().roomsInUse.Add(gameObject);
        }


        GetAllObjects();
        isRevealed = true;
    }
    public void GetAllObjects()
    {
        allObjects = this.transform.GetComponentsInChildren<Transform>();
    }

    public void GetAllRenderers()
    {
        allRenderers = GetComponentsInChildren<Renderer>();
    }
    public void ShowRoom(bool val, Vector3 point = new Vector3())
    {
        ShowRoomAsyncPoint(val, point, percentReveal);
        return;

        GetAllObjects();
        for (int i = 0; i < allObjects.Length; i++)
        {
            if (HandleActivator(allObjects[i].gameObject)) { continue; }
            if (Entity.IsOfEntity(allObjects[i].gameObject)) { continue; }


            if (allObjects[i].GetComponent<Renderer>())
            {
                allObjects[i].GetComponent<Renderer>().enabled = val;
            }
        }

        isRevealed = val;
        OnShowRoom?.Invoke(this, val);

    }

    public async void ShowRoomAsync(bool val)
    {
        GetAllRenderers();
        List<Task> tasks = new List<Task>();
        var lights = GetComponentsInChildren<Light>().ToList();


        for (int i = 0; i < allRenderers.Length; i++)
        {
            if(Entity.IsOfEntity(allRenderers[i].gameObject) ||
                HandleActivator(allRenderers[i].gameObject))
            {
                continue;
            }
            tasks.Add(Render(allRenderers[i], val));
        }

        foreach(var i in lights)
        {
            tasks.Add(Set(i, val));
        }

        await Task.WhenAll(tasks);
        isRevealed = val;
        OnShowRoom?.Invoke(this, val);
       

    }

    public async void ShowRoomAsyncPoint(bool val, Vector3 pointPosition, float percent = .025f)
    {
        
        var orderedRenders = new List<Renderer>();
        
        GetAllRenderers();
        isRevealed = val;
        OnShowRoom?.Invoke(this, val);
        if (val)
        {
            orderedRenders = allRenderers.OrderBy(render => V3Helper.Distance(render.gameObject.transform.position, pointPosition)).ToList();
        }
        else
        {
            orderedRenders = allRenderers.OrderByDescending(render => V3Helper.Distance(render.transform.position, pointPosition)).ToList();
        }

        int increments = (int)Mathf.Ceil(orderedRenders.Count * percent);

        if (val)
        {
            SetLights(val);
        }

        var count = 0;
        try
        {
            while(count < orderedRenders.Count)
            {
                if(isRevealed != val) { break; }
                if (HandleActivator(orderedRenders[count].gameObject))
                {
                    count++; 
                    continue;
                }

                orderedRenders[count].enabled = val;
                count++;
                if(count % increments == 0){ await Task.Yield(); }
            }
        }
        catch 
        {
            Debugger.InConsole(5439, $"{orderedRenders[count]} bugged out");
        }

        if(!val)
        {
            SetLights(val);
        }


        void SetLights(bool val)
        {
            foreach (var light in GetComponentsInChildren<Light>())
            {
                if (isRevealed != val) { break; }
                light.enabled = val;
            }
        }
    }
    bool HandleActivator(GameObject activator)
    {
        if (!activator.GetComponent<WalkThroughActivate>()) { return false; }


        return true;
    }

    public async Task Render(Renderer current, bool val)
    {
        current.enabled = val;
        await Task.Yield();
    }

    public async Task Set(Light light, bool val)
    {
        light.enabled = val;
        await Task.Yield();
    }
    void Start()
    {
        GetDependencies();
        
        if(!ignoreCheckRoomCollison)
        {
            badSpawn = false;
            Invoke("CheckRoomCollision", .0625f);
        }
        foreach(PathInfo path in paths)
        {
            if(!path.isEntrance)
            {
                path.isUsed = false;
            }
        }

        if(!badSpawn)
        {
            percentReveal = MapInfo.active.RoomGenerator().roomRevealPercent;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void CheckRoomCollision()
    {
        if (roomCenter == null) { return; }
        if (isEntranceRoom) { return; }
        foreach(Transform probe in roomCenter)
        {
            foreach (Transform child in allObjects)
            {
                var direction = V3Helper.Direction(child.position, probe.position);
                var distance = V3Helper.Distance(child.position, probe.position);

                Ray ray = new Ray(probe.position, direction);

                if (Physics.Raycast(ray, out RaycastHit hit, distance))
                {
                    if (!allObjects.Contains(hit.transform))
                    {
                        badSpawn = true;
                        incompatablePaths.Add(originPath);
                        if (mapInfo) { mapInfo.RoomGenerator().badSpawnRooms.Add(gameObject); }
                        return;
                    }
                }
            }
        }
        CheckRoomAbove();
    }
    public void CheckRoomAbove()
    {
        if(roomCenter == null) { return; }
        if (isEntranceRoom) { return; }

        foreach(Transform probe in roomCenter)
        {
            var distance = 35f;
            var direction = Vector3.up;
            Ray ray = new Ray(probe.position, direction);
            if(Physics.Raycast(ray, out RaycastHit hit, distance))
            {
                if (!allObjects.Contains(hit.transform))
                {
                    badSpawn = true;
                    incompatablePaths.Add(originPath);
                    if (mapInfo) { mapInfo.RoomGenerator().badSpawnRooms.Add(gameObject); }
                    return;
                }
            }
        }

    }
    public void CheckRoomBelow()
    {
        if (isEntranceRoom) { return; }


    }
}
