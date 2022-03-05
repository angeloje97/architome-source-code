using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{

    public class PromptAllDeath : MonoBehaviour
    {
        // Start is called before the first frame update
    
        public void SelectRevive()
        {
            var deadPlayers = GMHelper.EntityDeathHandler().DeadPlayableEntities();
            var worldActions = GMHelper.WorldActions();

            try
            {
                foreach(var deadPlayer in deadPlayers)
                {
                    worldActions.ReviveAtSpawnBeacon(deadPlayer.gameObject);
                }
            }
            catch
            {

            }
        }
    }
}
