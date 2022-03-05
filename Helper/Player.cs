using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static bool HasLineOfSight(GameObject target)
    {
        if (GMHelper.GameManager() == null) { return false; }

        var gameManager = GMHelper.GameManager();

        foreach (EntityInfo playableEntity in gameManager.playableEntities)
        {
            if (playableEntity.LineOfSight().HasLineOfSight(target))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsPlayer(EntityInfo checkEntity)
    {
        if (GMHelper.GameManager() &&
            GMHelper.GameManager().playableEntities.Contains(checkEntity))
        {
            return true;
        }

        return false;
    }
}
