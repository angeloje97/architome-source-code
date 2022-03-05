using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalInfo : MonoBehaviour
{
    // Start is called before the first frame update
    public static List<PortalInfo> portals;

    public List<PortalInfo> portalList;
    public Transform portalSpot;
    public Clickable clickable;
    public int portalNum;

    void GetDependencies()
    {
        if(GetComponent<Clickable>())
        {
            clickable = GetComponent<Clickable>();
        }
    }
    void Start()
    {
        GetDependencies();
        if(portals == null) { portals = new List<PortalInfo>(); }
        portals.Add(this);
        portalList = portals;
        portalNum = portals.IndexOf(this);
    }

    // Update is called once per frame
    void Update()
    {
        HandleMoveTargets();
    }

    void HandleMoveTargets()
    {
        if(clickable == null) { return; }
        if(clickable.clickedEntities.Count >0)
        {
            var entities = clickable.clickedEntities;
            for(int i = 0; i < entities.Count; i++)
            {
                entities[i].Movement();
                entities[i].Movement().MoveTo(portalSpot.position);
                entities.RemoveAt(i);
                i--;
            }
        }
        
    }
}
