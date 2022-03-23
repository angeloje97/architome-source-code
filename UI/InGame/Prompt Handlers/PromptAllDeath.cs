using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Architome
{

    public class PromptAllDeath : MonoBehaviour
    {
        // Start is called before the first frame update
    
        public void SelectRevive()
        {
            var playerSpawnBeacon = GMHelper.WorldInfo().lastPlayerSpawnBeacon;

            var spawnBeaconHandler = playerSpawnBeacon.GetComponentInChildren<PlayerSpawnBeaconHandler>();

            spawnBeaconHandler.ReviveDeadPartyMembers();
        }
    }
}
