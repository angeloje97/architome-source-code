using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class Teleporter : MonoBehaviour
    {
        public Transform spot;

        LayerMask groundLayer;

        private void Start()
        {
            GetDependencies();

        }
        void GetDependencies()
        {
            var layerMasksData = LayerMasksData.active;

            if (layerMasksData)
            {
                groundLayer = layerMasksData.walkableLayer;
            }
        }


        // Update is called once per frame
        public void TeleportEntityToSpot(EntityInfo entity)
        {
            if (spot == null) return;

            var offset = entity.GetComponent<BoxCollider>().size.y / 2;


            var groundPosition = V3Helper.GroundPosition(spot.position, groundLayer, 0, offset);

            entity.transform.position = groundPosition;

            entity.infoEvents.OnSignificantMovementChange?.Invoke(groundPosition);


        }
    }
}
