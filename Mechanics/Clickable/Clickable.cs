using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;

public class Clickable : MonoBehaviour
{
    // Start is called before the first frame update
    public List<EntityInfo> clickedEntities;
    public bool partyCanClick;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Click(EntityInfo entity)
    {
        if (!clickedEntities.Contains(entity))
        {
            clickedEntities.Add(entity);
        }
    }
}
