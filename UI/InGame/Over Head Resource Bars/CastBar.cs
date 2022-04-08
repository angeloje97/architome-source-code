using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architome.Enums;
using Architome;
public class CastBar : MonoBehaviour
{
    // Start is called before the first frame update
    public Image slider;
    public EntityInfo entityInfo;
    public AbilityManager abilityManager;

    public List<GameObject> children;

    public void GetDependencies()
    {   if(abilityManager == null)
        {
            if (GetComponentInParent<GraphicsInfo>())
            {
                if (GetComponentInParent<GraphicsInfo>().entityInfo)
                {
                    entityInfo = GetComponentInParent<GraphicsInfo>().entityInfo;
                    
                    if(entityInfo.AbilityManager())
                    {
                        abilityManager = entityInfo.AbilityManager();
                    }
                }
            }
        }

        if(children.Count == 0)
        {
            foreach(Transform child in transform)
            {
                children.Add(child.gameObject);
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

    void SetActive(bool status)
    {
        foreach(GameObject child in children)
        {
            if(child.name != "Text")
            {
                child.SetActive(status);
            }
        }
    }
}
