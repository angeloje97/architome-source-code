using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class RoomManifester : MonoBehaviour
    {
        void Start()
        {
            ManifestRoom();
        }

        // Update is called once per frame
        async void ManifestRoom()
        {
            var layerMasksData = LayerMasksData.active;

            while (layerMasksData == null)
            {
                layerMasksData = LayerMasksData.active;
                await Task.Delay(250);
            }

            var room = Entity.Room(transform.position);

            if (room == null) return;

            transform.SetParent(room.transform);
        }
    }
}
