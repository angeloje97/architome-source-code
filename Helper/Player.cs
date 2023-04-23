using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;

public static class Player
{

    public static bool HasLineOfSight(EntityInfo target)
    {
        if (GMHelper.GameManager() == null) { return false; }

        var gameManager = GMHelper.GameManager();

        foreach (EntityInfo playableEntity in gameManager.playableEntities)
        {
            if (playableEntity.CanSee(target.transform))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsPlayer(EntityInfo checkEntity)
    {
        var gameManager = GameManager.active;

        if (gameManager && gameManager.playableEntities.Contains(checkEntity)) return true;

        return false;
    }
}
