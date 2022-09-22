using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public class Teleporter : MonoBehaviour
    {
        public Transform spot;
        public float teleportDelay;

        LayerMask groundLayer;



        public Action<EntityInfo, Vector3> BeforeTeleportEntity, AfterTeleportEntity;

        public enum Event
        {
            BeforeTeleport,
            AfterTeleport,
        }

        public void AddEventAction(Event trigger, Action<EntityInfo, Vector3> action)
        {
            switch (trigger)
            {
                case Event.BeforeTeleport:
                    BeforeTeleportEntity += action;
                    break;
                case Event.AfterTeleport:
                    AfterTeleportEntity += action;
                    break;
            }
        }


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
        public async void TeleportEntityToSpot(EntityInfo entity)
        {
            if (spot == null) return;

            var offset = entity.GetComponent<BoxCollider>().size.y / 2;


            var groundPosition = V3Helper.GroundPosition(spot.position, groundLayer, 0, offset);

            BeforeTeleportEntity?.Invoke(entity, groundPosition);

            await Task.Delay((int)(teleportDelay * 1000));

            entity.transform.position = groundPosition;
            entity.infoEvents.OnSignificantMovementChange?.Invoke(groundPosition);

            AfterTeleportEntity?.Invoke(entity, groundPosition);
        }
    }
}
