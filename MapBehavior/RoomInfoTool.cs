using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomInfoTool : MonoBehaviour
{
    // Start is called before the first frame update

    public bool showEnemyPositions;
    public bool showPatrolSpots;
    public bool showPatrolGroupSpots;

    public RoomInfo info;
    private void OnValidate()
    {
        if(GetComponent<RoomInfo>())
        {
            info = GetComponent<RoomInfo>();
        }

        ShowEnemiesPos(showEnemyPositions);
        ShowPatrolSpots(showPatrolSpots);
        ShowPatrolGroupSpots(showPatrolGroupSpots);
    }

    private void Awake()
    {
        showEnemyPositions = false;
        showPatrolSpots = false;
        showPatrolSpots = false;

        OnValidate();
    }

    void ShowEnemiesPos(bool val)
    {
        if(info.tier1EnemyPos)
        {
            foreach (Transform child in info.tier1EnemyPos)
            {
                child.gameObject.SetActive(val);
            }
        }

        if(info.tier2EnemyPos)
        {
            foreach(Transform child in info.tier2EnemyPos)
            {
                child.gameObject.SetActive(val);
            }
        }

        if(info.tier3EnemyPos)
        {
            foreach(Transform child in info.tier3EnemyPos)
            {
                child.gameObject.SetActive(val);    
            }
        }
    }

    void ShowPatrolSpots(bool val)
    {
        if(info.patrolPoints)
        {
            foreach(Transform child in info.patrolPoints)
            {
                child.gameObject.SetActive(val);
            }
        }
    }

    void ShowPatrolGroupSpots(bool val)
    {
        if(info.patrolGroups)
        {
            foreach(Transform group in info.patrolGroups)
            {
                foreach(Transform child in group)
                {
                    child.gameObject.SetActive(val);
                }
            }
        }
    }
}
