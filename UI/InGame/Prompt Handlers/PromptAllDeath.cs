using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;
using System;

namespace Architome
{

    public class PromptAllDeath : MonoBehaviour
    {
        // Start is called before the first frame update
        [Serializable]
        public struct Info
        {
            public TextMeshProUGUI ReviveText;
            public Button buttonToEnable;
            public Image buttonImage;
            public Color inactiveColor;
            public Color activeColor;
        }

        public Info reviveInfo;

        public void Start()
        {
            DelayReviveButton();
        }

        async void DelayReviveButton()
        {
            reviveInfo.buttonToEnable.enabled = false;
            reviveInfo.buttonImage.color = reviveInfo.inactiveColor;
            var timer = 3;
            reviveInfo.ReviveText.text = $"Revive in {timer}s";

            while (timer > 0)
            {
                await Task.Delay(1000);

                timer -= 1;

                reviveInfo.ReviveText.text = $"Revive in {timer}s";
            }

            reviveInfo.buttonToEnable.enabled = true;
            reviveInfo.buttonImage.color = reviveInfo.activeColor;
            reviveInfo.ReviveText.text = "Revive";
            
        }



        public void SelectRevive()
        {
            var playerSpawnBeacon = GMHelper.WorldInfo().lastPlayerSpawnBeacon;

            var spawnBeaconHandler = playerSpawnBeacon.GetComponentInChildren<PlayerSpawnBeaconHandler>();

            spawnBeaconHandler.ReviveDeadPartyMembers();
        }
    }
}
