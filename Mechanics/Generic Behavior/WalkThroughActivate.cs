using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Architome;
public class WalkThroughActivate : MonoBehaviour
{
    public List<GameObject> activatedObjects;
    public List<GameObject> deactivatedObjects;
    
    public bool isActive = false;

    public AstarPath astarPath;



    // Start is called before the first frame update
    private void Awake()
    {
        activatedObjects ??= new();
        deactivatedObjects ??= new();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        return;   
        if(isActive) { return; }

        var entityInfo = other.GetComponent<EntityInfo>();

        if (entityInfo == null) return;
        
        if (GMHelper.GameManager() && !GMHelper.GameManager().playableEntities.Contains(other.GetComponent<EntityInfo>())) { return; }
        isActive = true;
        foreach (GameObject activatedObject in activatedObjects)
        {
            HandleActivatedParts(activatedObject);
        }

        foreach(GameObject deactivatedObject in deactivatedObjects)
        {
            if(deactivatedObject.GetComponent<BoxCollider>())
            {
                deactivatedObject.GetComponent<BoxCollider>().enabled = false;
            }
            if (deactivatedObject) { deactivatedObject.SetActive(false); }
        }

        void HandleActivatedParts(GameObject activatedObject)
        {
            if(activatedObject == null) { return; }
            activatedObject.SetActive(true);
            HandleRoom();

            void HandleRoom()
            {
                if(activatedObject.GetComponent<RoomInfo>())
                {
                    activatedObject.GetComponent<RoomInfo>().ShowRoom(true);
                }
            }
        }
        
    }

}
