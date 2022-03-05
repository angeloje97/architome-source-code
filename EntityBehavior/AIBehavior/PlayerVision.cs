using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
public class PlayerVision : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject entityObject;

    public AIBehavior behavior;
    public EntityInfo entityInfo;
    public LineOfSight lineOfSight;

    public List<GameObject> seenEntities;

    public float meshResolution;

    public void GetDependencies()
    {
        if(lineOfSight == null)
        {
            if(entityInfo == null)
            {
                if(GetComponentInParent<AIBehavior>())
                {
                    behavior = GetComponentInParent<AIBehavior>();
                    entityInfo = behavior.entityInfo;
                    entityObject = entityInfo.gameObject;
                }
            }

            if(behavior)
            {
                if(behavior.LineOfSight())
                {
                    lineOfSight = behavior.LineOfSight();
                }
            }

        }
    }

    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        GetDependencies();
    }

    
}
