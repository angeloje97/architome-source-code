using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;
public class EntityPortraitManager : MonoBehaviour
{
    // Start is called before the first frame update


    public List<PortraitBehavior> memberPortraits;
    public PortraitBehavior currentTargetPortrait;
    
    void Start()
    {
        GameManager.active.OnNewPlayableEntity += OnNewPlayableEntity;
    }
    // Update is called once per frame

    public void OnNewPlayableEntity(EntityInfo newEntity, int index)
    {
        if(memberPortraits.Count > index)
        {
            memberPortraits[index].gameObject.SetActive(true);
            memberPortraits[index].SetEntity(newEntity);
        }
    }
}
